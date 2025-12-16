using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PilotLife.Application.Common;
using PilotLife.Application.Jobs;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services.Jobs;

/// <summary>
/// Service for generating cargo and passenger jobs at airports across the world.
/// Uses parallel processing to utilize multiple CPU cores.
/// </summary>
public class JobGenerationService : IJobGenerator
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobGenerationService> _logger;
    private readonly JobGenerationConfiguration _config;

    // Airport type constants
    private const string LargeAirport = "large_airport";
    private const string MediumAirport = "medium_airport";
    private const string SmallAirport = "small_airport";

    // Base rates per nautical mile for distance pricing
    private const decimal BaseRatePerNmShort = 2.50m;   // < 150nm
    private const decimal BaseRatePerNmMedium = 2.00m;  // 150-500nm
    private const decimal BaseRatePerNmLong = 1.50m;    // 500-1500nm
    private const decimal BaseRatePerNmUltraLong = 1.20m; // > 1500nm

    // Urgency multipliers
    private static readonly Dictionary<JobUrgency, decimal> UrgencyMultipliers = new()
    {
        { JobUrgency.Standard, 1.0m },
        { JobUrgency.Priority, 1.2m },
        { JobUrgency.Express, 1.5m },
        { JobUrgency.Urgent, 2.0m },
        { JobUrgency.Critical, 3.0m }
    };

    // Urgency expiry times in hours (base, before world multiplier)
    private static readonly Dictionary<JobUrgency, (int min, int max)> UrgencyExpiryHours = new()
    {
        { JobUrgency.Standard, (24, 48) },
        { JobUrgency.Priority, (12, 24) },
        { JobUrgency.Express, (6, 12) },
        { JobUrgency.Urgent, (2, 6) },
        { JobUrgency.Critical, (1, 2) }
    };

    // Passenger rates per person per nautical mile
    private static readonly Dictionary<PassengerClass, decimal> PassengerRatesPerNm = new()
    {
        { PassengerClass.Economy, 0.15m },
        { PassengerClass.Business, 0.35m },
        { PassengerClass.First, 0.60m },
        { PassengerClass.Charter, 0.80m },
        { PassengerClass.Vip, 1.50m }
    };

    public JobGenerationService(
        IServiceScopeFactory scopeFactory,
        ILogger<JobGenerationService> logger,
        IOptions<JobGenerationConfiguration> config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = config.Value;
    }

    public async Task PopulateWorldJobsAsync(Guid worldId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting parallel job population for world {WorldId} with {DOP} workers",
            worldId, _config.MaxDegreeOfParallelism);

        // Pre-load shared read-only data using a temporary scope
        World? world;
        List<CargoType> cargoTypes;
        List<Airport> allAirports;
        Dictionary<int, int> existingJobCounts;

        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();

            world = await context.Worlds.AsNoTracking().FirstOrDefaultAsync(w => w.Id == worldId, cancellationToken);
            if (world == null)
            {
                _logger.LogWarning("World {WorldId} not found", worldId);
                return;
            }

            cargoTypes = await context.CargoTypes
                .AsNoTracking()
                .Where(c => c.IsActive)
                .ToListAsync(cancellationToken);

            if (cargoTypes.Count == 0)
            {
                _logger.LogWarning("No active cargo types found. Cannot generate jobs.");
                return;
            }

            existingJobCounts = await context.Jobs
                .Where(j => j.WorldId == worldId && j.Status == JobStatus.Available && j.ExpiresAt > DateTimeOffset.UtcNow)
                .GroupBy(j => j.DepartureAirportId)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);

            allAirports = await context.Airports
                .AsNoTracking()
                .Where(a => a.Type == LargeAirport || a.Type == MediumAirport || a.Type == SmallAirport)
                .ToListAsync(cancellationToken);
        }

        // In dev mode, filter airports to only those within radius of center airport
        if (_config.DevModeEnabled)
        {
            var centerAirport = allAirports.FirstOrDefault(a => a.Ident == _config.DevCenterAirportIcao);
            if (centerAirport != null)
            {
                allAirports = allAirports
                    .Where(a => CalculateDistance(centerAirport.Latitude, centerAirport.Longitude, a.Latitude, a.Longitude) <= _config.DevCenterRadiusNm)
                    .ToList();

                _logger.LogInformation(
                    "Dev mode: Limited to {Count} airports within {Radius}nm of {Center}",
                    allAirports.Count, _config.DevCenterRadiusNm, _config.DevCenterAirportIcao);
            }
            else
            {
                _logger.LogWarning("Dev mode center airport {Icao} not found, processing all airports", _config.DevCenterAirportIcao);
            }
        }

        // Build spatial index (read-only, safe to share across threads)
        var airportsByRegion = BuildAirportSpatialIndex(allAirports);

        var totalAirports = allAirports.Count;
        var processedCount = 0;
        var jobsCreated = 0;

        _logger.LogInformation("Processing {Total} airports in parallel", totalAirports);

        // Process airports in parallel
        await Parallel.ForEachAsync(
            allAirports,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = _config.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            },
            async (airport, token) =>
            {
                var targetJobCount = GetTargetJobCount(airport.Type);
                existingJobCounts.TryGetValue(airport.Id, out var currentCount);
                var jobsNeeded = targetJobCount - currentCount;

                if (jobsNeeded <= 0)
                {
                    Interlocked.Increment(ref processedCount);
                    return;
                }

                // Each parallel iteration gets its own scope and DbContext
                using var innerScope = _scopeFactory.CreateScope();
                var innerContext = innerScope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();

                var destinations = GetNearbyAirports(airport, airportsByRegion, allAirports);
                var jobs = GenerateJobsForAirport(world, airport, destinations, cargoTypes, jobsNeeded);

                if (jobs.Count > 0)
                {
                    await innerContext.Jobs.AddRangeAsync(jobs, token);
                    await innerContext.SaveChangesAsync(token);
                    Interlocked.Add(ref jobsCreated, jobs.Count);
                }

                var currentProcessed = Interlocked.Increment(ref processedCount);

                // Log progress more frequently in development (every 100 airports or configured interval)
                var logInterval = Math.Min(100, _config.ProgressLogInterval);
                if (currentProcessed % logInterval == 0 || currentProcessed == totalAirports)
                {
                    _logger.LogInformation(
                        "Job population progress: {Processed}/{Total} airports, {Jobs} jobs created",
                        currentProcessed, totalAirports, jobsCreated);
                }
            });

        _logger.LogInformation(
            "=== Job population COMPLETE for world {WorldId}: {Airports} airports processed, {Jobs} jobs created ===",
            worldId, processedCount, jobsCreated);
    }

    public async Task RefreshStaleJobsAsync(Guid worldId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Refreshing stale jobs for world {WorldId} with {DOP} workers",
            worldId, _config.MaxDegreeOfParallelism);

        // Pre-load shared read-only data
        World? world;
        List<CargoType> cargoTypes;
        List<Airport> allAirports;
        List<(Airport Airport, int JobCount)> airportsNeedingRefresh;

        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();

            world = await context.Worlds.AsNoTracking().FirstOrDefaultAsync(w => w.Id == worldId, cancellationToken);
            if (world == null) return;

            cargoTypes = await context.CargoTypes
                .AsNoTracking()
                .Where(c => c.IsActive)
                .ToListAsync(cancellationToken);

            if (cargoTypes.Count == 0) return;

            // Single query to get all airports with their current job counts
            airportsNeedingRefresh = await context.Airports
                .AsNoTracking()
                .Where(a => a.Type == LargeAirport || a.Type == MediumAirport || a.Type == SmallAirport)
                .GroupJoin(
                    context.Jobs.Where(j =>
                        j.WorldId == worldId &&
                        j.Status == JobStatus.Available &&
                        j.ExpiresAt > DateTimeOffset.UtcNow),
                    airport => airport.Id,
                    job => job.DepartureAirportId,
                    (airport, jobs) => new { Airport = airport, JobCount = jobs.Count() })
                .Where(x => x.JobCount < _config.MinJobsPerAirport)
                .Select(x => ValueTuple.Create(x.Airport, x.JobCount))
                .ToListAsync(cancellationToken);

            allAirports = await context.Airports
                .AsNoTracking()
                .Where(a => a.Type == LargeAirport || a.Type == MediumAirport || a.Type == SmallAirport)
                .ToListAsync(cancellationToken);
        }

        // In dev mode, filter airports to only those within radius of center airport
        if (_config.DevModeEnabled)
        {
            var centerAirport = allAirports.FirstOrDefault(a => a.Ident == _config.DevCenterAirportIcao);
            if (centerAirport != null)
            {
                var airportIdsInRange = allAirports
                    .Where(a => CalculateDistance(centerAirport.Latitude, centerAirport.Longitude, a.Latitude, a.Longitude) <= _config.DevCenterRadiusNm)
                    .Select(a => a.Id)
                    .ToHashSet();

                allAirports = allAirports.Where(a => airportIdsInRange.Contains(a.Id)).ToList();
                airportsNeedingRefresh = airportsNeedingRefresh.Where(x => airportIdsInRange.Contains(x.Airport.Id)).ToList();

                _logger.LogInformation(
                    "Dev mode: Limited to {Count} airports within {Radius}nm of {Center}",
                    allAirports.Count, _config.DevCenterRadiusNm, _config.DevCenterAirportIcao);
            }
        }

        if (airportsNeedingRefresh.Count == 0)
        {
            _logger.LogInformation("No airports need job refresh for world {WorldId}", worldId);
            return;
        }

        _logger.LogInformation("Found {Count} airports needing job refresh", airportsNeedingRefresh.Count);

        var airportsByRegion = BuildAirportSpatialIndex(allAirports);
        var jobsCreated = 0;
        var processedCount = 0;
        var totalAirports = airportsNeedingRefresh.Count;

        // Process airports in parallel
        await Parallel.ForEachAsync(
            airportsNeedingRefresh,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = _config.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            },
            async (item, token) =>
            {
                var (airport, jobCount) = item;
                var targetJobCount = GetTargetJobCount(airport.Type);
                var jobsNeeded = targetJobCount - jobCount;

                if (jobsNeeded <= 0)
                {
                    Interlocked.Increment(ref processedCount);
                    return;
                }

                // Each parallel iteration gets its own scope and DbContext
                using var innerScope = _scopeFactory.CreateScope();
                var innerContext = innerScope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();

                var destinations = GetNearbyAirports(airport, airportsByRegion, allAirports);
                var jobs = GenerateJobsForAirport(world, airport, destinations, cargoTypes, jobsNeeded);

                if (jobs.Count > 0)
                {
                    await innerContext.Jobs.AddRangeAsync(jobs, token);
                    await innerContext.SaveChangesAsync(token);
                    Interlocked.Add(ref jobsCreated, jobs.Count);
                }

                var currentProcessed = Interlocked.Increment(ref processedCount);

                if (currentProcessed % _config.ProgressLogInterval == 0)
                {
                    _logger.LogInformation(
                        "Job refresh progress: {Processed}/{Total} airports, {Jobs} jobs created",
                        currentProcessed, totalAirports, jobsCreated);
                }
            });

        _logger.LogInformation("Refreshed jobs for world {WorldId}: {Jobs} new jobs created", worldId, jobsCreated);
    }

    public async Task<int> CleanupExpiredJobsAsync(Guid? worldId = null, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();

        var query = context.Jobs
            .Where(j => j.Status == JobStatus.Available && j.ExpiresAt <= DateTimeOffset.UtcNow);

        if (worldId.HasValue)
        {
            query = query.Where(j => j.WorldId == worldId.Value);
        }

        var expiredJobs = await query.ToListAsync(cancellationToken);

        if (expiredJobs.Count > 0)
        {
            foreach (var job in expiredJobs)
            {
                job.Status = JobStatus.Expired;
            }
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Marked {Count} jobs as expired", expiredJobs.Count);
        }

        return expiredJobs.Count;
    }

    private int GetTargetJobCount(string airportType)
    {
        return airportType switch
        {
            LargeAirport => _config.JobsPerLargeAirport,
            MediumAirport => _config.JobsPerMediumAirport,
            SmallAirport => _config.JobsPerSmallAirport,
            _ => _config.JobsPerSmallAirport / 2
        };
    }

    private static Dictionary<(int latBucket, int lonBucket), List<Airport>> BuildAirportSpatialIndex(List<Airport> airports)
    {
        // Bucket airports into 10-degree grid cells for faster nearby lookups
        var index = new Dictionary<(int, int), List<Airport>>();

        foreach (var airport in airports)
        {
            var latBucket = (int)(airport.Latitude / 10);
            var lonBucket = (int)(airport.Longitude / 10);
            var key = (latBucket, lonBucket);

            if (!index.ContainsKey(key))
            {
                index[key] = [];
            }
            index[key].Add(airport);
        }

        return index;
    }

    private List<Airport> GetNearbyAirports(
        Airport departure,
        Dictionary<(int latBucket, int lonBucket), List<Airport>> spatialIndex,
        List<Airport> allAirports)
    {
        var latBucket = (int)(departure.Latitude / 10);
        var lonBucket = (int)(departure.Longitude / 10);

        var candidates = new List<Airport>();

        // Check surrounding cells (3x3 grid = roughly 30 degrees range)
        for (int dLat = -3; dLat <= 3; dLat++)
        {
            for (int dLon = -3; dLon <= 3; dLon++)
            {
                var key = (latBucket + dLat, lonBucket + dLon);
                if (spatialIndex.TryGetValue(key, out var airports))
                {
                    candidates.AddRange(airports);
                }
            }
        }

        // Filter by actual distance and shuffle using thread-safe random
        return candidates
            .Where(a => a.Id != departure.Id)
            .Select(a => new
            {
                Airport = a,
                Distance = CalculateDistance(departure.Latitude, departure.Longitude, a.Latitude, a.Longitude)
            })
            .Where(x => x.Distance >= _config.MinRouteDistanceNm && x.Distance <= _config.MaxRouteDistanceNm)
            .OrderBy(_ => ThreadSafeRandom.Next(int.MaxValue))
            .Take(100)
            .Select(x => x.Airport)
            .ToList();
    }

    private List<Job> GenerateJobsForAirport(
        World world,
        Airport departure,
        List<Airport> destinations,
        List<CargoType> cargoTypes,
        int count)
    {
        var jobs = new List<Job>();

        if (destinations.Count == 0) return jobs;

        // Use thread-safe random for this generation
        var random = ThreadSafeRandom.Instance;

        for (int i = 0; i < count; i++)
        {
            var destination = destinations[random.Next(destinations.Count)];
            var distance = CalculateDistance(
                departure.Latitude, departure.Longitude,
                destination.Latitude, destination.Longitude);

            // Skip invalid distances
            if (distance < _config.MinRouteDistanceNm || distance > _config.MaxRouteDistanceNm)
                continue;

            // Decide job type based on config
            bool isCargoJob = random.NextDouble() < _config.CargoJobPercentage;

            Job? job;
            if (isCargoJob)
            {
                job = GenerateCargoJob(world, departure, destination, distance, cargoTypes, random);
            }
            else
            {
                job = GeneratePassengerJob(world, departure, destination, distance, random);
            }

            if (job != null)
            {
                jobs.Add(job);
            }
        }

        return jobs;
    }

    private Job GenerateCargoJob(World world, Airport departure, Airport destination,
        double distance, List<CargoType> cargoTypes, Random random)
    {
        var cargoType = cargoTypes[random.Next(cargoTypes.Count)];

        int weight = random.Next(cargoType.MinWeightLbs, cargoType.MaxWeightLbs + 1);
        weight = (weight / 10) * 10; // Round to nearest 10 lbs
        weight = Math.Max(weight, cargoType.MinWeightLbs);

        var urgency = PickUrgency(cargoType.IsTimeCritical, random);
        var basePayout = CalculateCargoBasePayout(distance, weight, cargoType);
        var finalPayout = ApplyMultipliers(basePayout, urgency, world.JobPayoutMultiplier);
        var expiryHours = CalculateExpiryHours(urgency, world.JobExpiryMultiplier, random);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(expiryHours);
        int estimatedMinutes = EstimateFlightTime(distance);
        int riskLevel = cargoType.IsIllegal ? (cargoType.IllegalRiskLevel ?? 3) : 1;

        return new Job
        {
            WorldId = world.Id,
            DepartureAirportId = departure.Id,
            DepartureIcao = departure.Ident,
            ArrivalAirportId = destination.Id,
            ArrivalIcao = destination.Ident,
            DistanceNm = Math.Round(distance, 1),
            DistanceCategory = GetDistanceCategory(distance),
            RouteDifficulty = RouteDifficulty.Easy,
            Type = JobType.Cargo,
            Status = JobStatus.Available,
            Urgency = urgency,
            CargoTypeId = cargoType.Id,
            CargoType = cargoType.Name,
            WeightLbs = weight,
            VolumeCuFt = cargoType.CalculateVolume(weight),
            RequiresSpecialCertification = cargoType.RequiresSpecialHandling,
            RequiredCertification = cargoType.SpecialHandlingType,
            RiskLevel = riskLevel,
            BasePayout = basePayout,
            Payout = finalPayout,
            EstimatedFlightTimeMinutes = estimatedMinutes,
            ExpiresAt = expiresAt,
            Title = GenerateCargoTitle(cargoType, urgency),
            Description = GenerateCargoDescription(cargoType, weight, departure, destination, urgency)
        };
    }

    private Job GeneratePassengerJob(World world, Airport departure, Airport destination, double distance, Random random)
    {
        var passengerClass = PickPassengerClass(random);

        int passengerCount = passengerClass switch
        {
            PassengerClass.Economy => random.Next(1, 20),
            PassengerClass.Business => random.Next(1, 10),
            PassengerClass.First => random.Next(1, 6),
            PassengerClass.Charter => random.Next(2, 12),
            PassengerClass.Vip => random.Next(1, 4),
            _ => random.Next(1, 10)
        };

        var urgency = PickUrgency(passengerClass == PassengerClass.Vip || passengerClass == PassengerClass.Charter, random);
        var basePayout = CalculatePassengerBasePayout(distance, passengerCount, passengerClass);
        var finalPayout = ApplyMultipliers(basePayout, urgency, world.JobPayoutMultiplier);
        var expiryHours = CalculateExpiryHours(urgency, world.JobExpiryMultiplier, random);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(expiryHours);
        int estimatedMinutes = EstimateFlightTime(distance);

        // Calculate approximate passenger weight (180 lbs avg + 40 lbs baggage)
        int totalPassengerWeight = passengerCount * 220;

        return new Job
        {
            WorldId = world.Id,
            DepartureAirportId = departure.Id,
            DepartureIcao = departure.Ident,
            ArrivalAirportId = destination.Id,
            ArrivalIcao = destination.Ident,
            DistanceNm = Math.Round(distance, 1),
            DistanceCategory = GetDistanceCategory(distance),
            RouteDifficulty = RouteDifficulty.Easy,
            Type = JobType.Passenger,
            Status = JobStatus.Available,
            Urgency = urgency,
            PassengerCount = passengerCount,
            PassengerClass = passengerClass,
            WeightLbs = totalPassengerWeight,
            RiskLevel = 1,
            BasePayout = basePayout,
            Payout = finalPayout,
            EstimatedFlightTimeMinutes = estimatedMinutes,
            ExpiresAt = expiresAt,
            Title = GeneratePassengerTitle(passengerClass, passengerCount),
            Description = GeneratePassengerDescription(passengerClass, passengerCount, departure, destination)
        };
    }

    private static decimal CalculateCargoBasePayout(double distance, int weight, CargoType cargoType)
    {
        var distanceRate = GetDistanceRate(distance);
        var distancePayout = (decimal)distance * distanceRate;
        var weightPayout = weight * cargoType.EffectiveRatePerLb;
        var basePayout = distancePayout + (weightPayout * 0.5m);
        basePayout *= cargoType.PayoutMultiplier;
        basePayout = Math.Max(basePayout, 100m);
        return Math.Round(basePayout, 2);
    }

    private static decimal CalculatePassengerBasePayout(double distance, int passengerCount, PassengerClass passengerClass)
    {
        var ratePerNm = PassengerRatesPerNm[passengerClass];
        var basePayout = (decimal)distance * ratePerNm * passengerCount;

        decimal perPassengerFee = passengerClass switch
        {
            PassengerClass.Economy => 25m,
            PassengerClass.Business => 75m,
            PassengerClass.First => 150m,
            PassengerClass.Charter => 200m,
            PassengerClass.Vip => 500m,
            _ => 50m
        };

        basePayout += perPassengerFee * passengerCount;
        basePayout = Math.Max(basePayout, 150m);
        return Math.Round(basePayout, 2);
    }

    private static decimal GetDistanceRate(double distance)
    {
        return distance switch
        {
            < 150 => BaseRatePerNmShort,
            < 500 => BaseRatePerNmMedium,
            < 1500 => BaseRatePerNmLong,
            _ => BaseRatePerNmUltraLong
        };
    }

    private static decimal ApplyMultipliers(decimal basePayout, JobUrgency urgency, decimal worldMultiplier)
    {
        var urgencyMultiplier = UrgencyMultipliers[urgency];
        var finalPayout = basePayout * urgencyMultiplier * worldMultiplier;
        return Math.Round(finalPayout, 2);
    }

    private static double CalculateExpiryHours(JobUrgency urgency, decimal worldExpiryMultiplier, Random random)
    {
        var (min, max) = UrgencyExpiryHours[urgency];
        var baseHours = random.Next(min, max + 1);
        return baseHours * (double)worldExpiryMultiplier;
    }

    private static int EstimateFlightTime(double distance)
    {
        double avgSpeed = distance switch
        {
            < 100 => 120,
            < 300 => 150,
            < 800 => 200,
            < 1500 => 300,
            _ => 450
        };
        return (int)Math.Ceiling((distance / avgSpeed) * 60);
    }

    private static JobUrgency PickUrgency(bool preferUrgent, Random random)
    {
        var roll = random.NextDouble();

        if (preferUrgent)
        {
            return roll switch
            {
                < 0.20 => JobUrgency.Critical,
                < 0.45 => JobUrgency.Urgent,
                < 0.70 => JobUrgency.Express,
                < 0.90 => JobUrgency.Priority,
                _ => JobUrgency.Standard
            };
        }
        else
        {
            return roll switch
            {
                < 0.02 => JobUrgency.Critical,
                < 0.08 => JobUrgency.Urgent,
                < 0.20 => JobUrgency.Express,
                < 0.40 => JobUrgency.Priority,
                _ => JobUrgency.Standard
            };
        }
    }

    private static PassengerClass PickPassengerClass(Random random)
    {
        var roll = random.NextDouble();
        return roll switch
        {
            < 0.50 => PassengerClass.Economy,
            < 0.75 => PassengerClass.Business,
            < 0.85 => PassengerClass.Charter,
            < 0.95 => PassengerClass.First,
            _ => PassengerClass.Vip
        };
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusNm = 3440.065;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusNm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    private static DistanceCategory GetDistanceCategory(double distance)
    {
        return distance switch
        {
            < 50 => DistanceCategory.VeryShort,
            < 150 => DistanceCategory.Short,
            < 500 => DistanceCategory.Medium,
            < 1500 => DistanceCategory.Long,
            _ => DistanceCategory.UltraLong
        };
    }

    private static string GenerateCargoTitle(CargoType cargoType, JobUrgency urgency)
    {
        var prefix = urgency switch
        {
            JobUrgency.Critical => "CRITICAL: ",
            JobUrgency.Urgent => "URGENT: ",
            JobUrgency.Express => "Express ",
            _ => ""
        };
        return $"{prefix}{cargoType.Name} Delivery";
    }

    private static string GenerateCargoDescription(
        CargoType cargoType, int weight, Airport departure, Airport destination, JobUrgency urgency)
    {
        var urgencyText = urgency switch
        {
            JobUrgency.Critical => "This is a critical delivery requiring immediate attention. ",
            JobUrgency.Urgent => "This shipment is urgently needed. ",
            JobUrgency.Express => "Express delivery required. ",
            _ => ""
        };

        var specialHandling = cargoType.RequiresSpecialHandling
            ? $"Special handling required: {cargoType.SpecialHandlingType}. "
            : "";

        return $"{urgencyText}Transport {weight:N0} lbs of {cargoType.Name} from {departure.Ident} ({departure.Name}) to {destination.Ident} ({destination.Name}). {specialHandling}";
    }

    private static string GeneratePassengerTitle(PassengerClass passengerClass, int count)
    {
        var classText = passengerClass switch
        {
            PassengerClass.Vip => "VIP",
            PassengerClass.First => "First Class",
            PassengerClass.Business => "Business Class",
            PassengerClass.Charter => "Charter",
            _ => ""
        };

        var passengerText = count == 1 ? "Passenger" : "Passengers";

        return string.IsNullOrEmpty(classText)
            ? $"{count} {passengerText}"
            : $"{classText} - {count} {passengerText}";
    }

    private static string GeneratePassengerDescription(
        PassengerClass passengerClass, int count, Airport departure, Airport destination)
    {
        var classDescription = passengerClass switch
        {
            PassengerClass.Vip => "VIP passengers requiring premium service and discretion",
            PassengerClass.First => "First class passengers expecting luxury service",
            PassengerClass.Business => "Business travelers on a tight schedule",
            PassengerClass.Charter => "Charter group requiring dedicated transport",
            _ => "Passengers"
        };

        var passengerWord = count == 1 ? "passenger" : "passengers";

        return $"Transport {count} {passengerWord} ({classDescription}) from {departure.Ident} ({departure.Name}) to {destination.Ident} ({destination.Name}).";
    }
}
