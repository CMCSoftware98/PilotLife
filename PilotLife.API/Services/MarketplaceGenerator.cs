using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PilotLife.Application.Common;
using PilotLife.Application.Marketplace;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services;

/// <summary>
/// Generates and populates marketplace dealers and inventory based on airport size.
/// Uses parallel batch processing to utilize multiple CPU cores.
/// </summary>
public class MarketplaceGenerator : IMarketplaceGenerator
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MarketplaceGenerator> _logger;
    private readonly MarketplaceConfiguration _config;

    // Airport type constants from the CSV
    private const string LargeAirport = "large_airport";
    private const string MediumAirport = "medium_airport";
    private const string SmallAirport = "small_airport";

    // Dealer name prefixes for variety
    private static readonly string[] DealerNamePrefixes =
    {
        "Sky", "Wing", "Aero", "Flight", "Air", "Cloud", "Eagle", "Horizon", "Summit", "Blue",
        "Premier", "Elite", "Pro", "Classic", "Golden", "Silver", "Ace", "Star", "Chief", "Prime"
    };

    private static readonly string[] DealerNameSuffixes =
    {
        "Aviation", "Aircraft", "Aero", "Wings", "Flight", "Air Services", "Planes", "Jets"
    };

    public MarketplaceGenerator(
        IServiceScopeFactory scopeFactory,
        ILogger<MarketplaceGenerator> logger,
        IOptions<MarketplaceConfiguration> config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = config.Value;
    }

    public async Task PopulateWorldMarketplaceAsync(Guid worldId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting parallel marketplace population for world {WorldId} with {BatchCount} parallel batches",
            worldId, _config.ParallelBatchCount);

        // Pre-load shared read-only data using a temporary scope
        World? world;
        List<Aircraft> availableAircraft;
        List<Airport> allAirports;
        Dictionary<string, List<AircraftDealer>> existingDealersByAirport;

        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();

            world = await context.Worlds.AsNoTracking().FirstOrDefaultAsync(w => w.Id == worldId, cancellationToken);
            if (world == null)
            {
                _logger.LogWarning("World {WorldId} not found", worldId);
                return;
            }

            availableAircraft = await context.Aircraft
                .AsNoTracking()
                .Where(a => a.IsApproved)
                .ToListAsync(cancellationToken);

            if (availableAircraft.Count == 0)
            {
                _logger.LogWarning("No approved aircraft found in database");
                return;
            }

            // Load all airports at once
            allAirports = await context.Airports
                .AsNoTracking()
                .OrderBy(a => a.Id)
                .ToListAsync(cancellationToken);

            // Apply DevMode filtering if enabled
            if (_config.DevModeEnabled)
            {
                var centerAirport = allAirports.FirstOrDefault(a => a.Ident == _config.DevCenterAirportIcao);
                if (centerAirport != null)
                {
                    allAirports = allAirports
                        .Where(a => CalculateDistance(centerAirport.Latitude, centerAirport.Longitude,
                            a.Latitude, a.Longitude) <= _config.DevCenterRadiusNm)
                        .ToList();
                    _logger.LogInformation(
                        "Dev mode: Limited marketplace to {Count} airports within {Radius}nm of {Center}",
                        allAirports.Count, _config.DevCenterRadiusNm, _config.DevCenterAirportIcao);
                }
                else
                {
                    _logger.LogWarning("Dev mode center airport {Icao} not found, processing all airports",
                        _config.DevCenterAirportIcao);
                }
            }

            existingDealersByAirport = await context.AircraftDealers
                .AsNoTracking()
                .Where(d => d.WorldId == worldId)
                .GroupBy(d => d.AirportIcao)
                .ToDictionaryAsync(g => g.Key, g => g.ToList(), cancellationToken);
        }

        var totalAirports = allAirports.Count;
        _logger.LogInformation("Processing {Total} airports in parallel batches", totalAirports);

        // Split airports into batches for parallel processing
        var batchCount = _config.ParallelBatchCount;
        var batchSize = (totalAirports + batchCount - 1) / batchCount; // Ceiling division
        var batches = allAirports.Chunk(batchSize).ToList();

        // Results collection
        var results = new ConcurrentBag<BatchResult>();

        // Process batches in parallel
        var tasks = batches.Select((batch, index) =>
            ProcessBatchAsync(
                index,
                batch,
                worldId,
                availableAircraft,
                existingDealersByAirport,
                results,
                cancellationToken));

        await Task.WhenAll(tasks);

        // Aggregate results
        var totalProcessed = results.Sum(r => r.AirportsProcessed);
        var totalDealersCreated = results.Sum(r => r.DealersCreated);
        var totalInventoryCreated = results.Sum(r => r.InventoryCreated);

        _logger.LogInformation(
            "Marketplace population complete for world {WorldId}: {Airports} airports, {Dealers} dealers, {Inventory} inventory items",
            worldId, totalProcessed, totalDealersCreated, totalInventoryCreated);
    }

    private async Task ProcessBatchAsync(
        int batchIndex,
        Airport[] airports,
        Guid worldId,
        List<Aircraft> availableAircraft,
        Dictionary<string, List<AircraftDealer>> existingDealersByAirport,
        ConcurrentBag<BatchResult> results,
        CancellationToken cancellationToken)
    {
        var result = new BatchResult { BatchIndex = batchIndex };

        // Each batch gets its own Random instance with unique seed
        var random = new Random(Environment.TickCount ^ batchIndex);

        // Create a scope and DbContext for this batch
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();

        var saveCounter = 0;
        const int saveInterval = 100;

        foreach (var airport in airports)
        {
            var dealerTypes = GetDealerTypesForAirport(airport.Type);
            var targetDealerCount = GetTargetDealerCount(airport.Type);

            // Get existing dealers for this airport (from pre-loaded data)
            existingDealersByAirport.TryGetValue(airport.Ident, out var existingDealersSnapshot);
            var existingTypes = existingDealersSnapshot?.Select(d => d.DealerType).ToHashSet() ?? [];

            // Track dealers created in this batch for this airport
            var dealersCreatedForAirport = new List<AircraftDealer>();

            // Create missing dealer types
            foreach (var dealerType in dealerTypes.Take(targetDealerCount))
            {
                if (!existingTypes.Contains(dealerType))
                {
                    var dealer = CreateDealer(worldId, airport, dealerType, random);
                    context.AircraftDealers.Add(dealer);
                    dealersCreatedForAirport.Add(dealer);
                    existingTypes.Add(dealerType);
                    result.DealersCreated++;
                }
            }

            // Save to get dealer IDs before generating inventory
            if (dealersCreatedForAirport.Count > 0)
            {
                await context.SaveChangesAsync(cancellationToken);
            }

            // Generate inventory for newly created dealers
            foreach (var dealer in dealersCreatedForAirport)
            {
                var inventoryCount = GenerateInventoryForDealer(dealer, availableAircraft, context, random);
                result.InventoryCreated += inventoryCount;
            }

            result.AirportsProcessed++;
            saveCounter++;

            // Periodic saves within batch
            if (saveCounter >= saveInterval)
            {
                await context.SaveChangesAsync(cancellationToken);
                saveCounter = 0;

                // Log progress
                _logger.LogDebug(
                    "Batch {Index} progress: {Processed} airports, {Dealers} dealers, {Inventory} inventory",
                    batchIndex, result.AirportsProcessed, result.DealersCreated, result.InventoryCreated);
            }
        }

        // Final save for this batch
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Batch {Index} complete: {Processed} airports, {Dealers} dealers, {Inventory} inventory",
            batchIndex, result.AirportsProcessed, result.DealersCreated, result.InventoryCreated);

        results.Add(result);
    }

    public async Task RefreshStaleInventoryAsync(Guid worldId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Refreshing stale inventory for world {WorldId} with {BatchCount} parallel batches",
            worldId, _config.ParallelBatchCount);

        // Pre-load shared read-only data
        List<Aircraft> availableAircraft;
        List<AircraftDealer> staleDealers;

        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();

            availableAircraft = await context.Aircraft
                .AsNoTracking()
                .Where(a => a.IsApproved)
                .ToListAsync(cancellationToken);

            if (availableAircraft.Count == 0) return;

            var refreshThreshold = DateTimeOffset.UtcNow.AddDays(-_config.InventoryExpirationDays);

            staleDealers = await context.AircraftDealers
                .AsNoTracking()
                .Where(d => d.WorldId == worldId &&
                            d.IsActive &&
                            d.LastInventoryRefresh < refreshThreshold)
                .ToListAsync(cancellationToken);
        }

        if (staleDealers.Count == 0)
        {
            _logger.LogInformation("No dealers need inventory refresh for world {WorldId}", worldId);
            return;
        }

        _logger.LogInformation("Found {Count} dealers needing inventory refresh", staleDealers.Count);

        // Split dealers into batches
        var batchCount = _config.ParallelBatchCount;
        var batchSize = (staleDealers.Count + batchCount - 1) / batchCount;
        var batches = staleDealers.Chunk(batchSize).ToList();

        var results = new ConcurrentBag<RefreshResult>();

        var tasks = batches.Select((batch, index) =>
            RefreshDealerBatchAsync(
                index,
                batch,
                availableAircraft,
                results,
                cancellationToken));

        await Task.WhenAll(tasks);

        var totalRefreshed = results.Sum(r => r.DealersRefreshed);
        var totalInventory = results.Sum(r => r.InventoryCreated);

        _logger.LogInformation(
            "Refreshed {Dealers} dealers with {Inventory} new inventory items for world {WorldId}",
            totalRefreshed, totalInventory, worldId);
    }

    private async Task RefreshDealerBatchAsync(
        int batchIndex,
        AircraftDealer[] dealers,
        List<Aircraft> availableAircraft,
        ConcurrentBag<RefreshResult> results,
        CancellationToken cancellationToken)
    {
        var result = new RefreshResult { BatchIndex = batchIndex };
        var random = new Random(Environment.TickCount ^ (batchIndex + 1000)); // Different seed from populate

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();

        var saveCounter = 0;
        const int saveInterval = 50;

        foreach (var dealerSnapshot in dealers)
        {
            // Re-fetch the dealer to get a tracked entity
            var dealer = await context.AircraftDealers
                .Include(d => d.Inventory.Where(i => !i.IsSold))
                .FirstOrDefaultAsync(d => d.Id == dealerSnapshot.Id, cancellationToken);

            if (dealer == null) continue;

            // Remove expired inventory
            var expiredInventory = dealer.Inventory
                .Where(i => i.ExpiresAt != null && i.ExpiresAt < DateTimeOffset.UtcNow)
                .ToList();

            foreach (var expired in expiredInventory)
            {
                context.DealerInventory.Remove(expired);
            }

            // Generate new inventory
            var inventoryCount = GenerateInventoryForDealer(dealer, availableAircraft, context, random);
            result.InventoryCreated += inventoryCount;
            result.DealersRefreshed++;

            saveCounter++;
            if (saveCounter >= saveInterval)
            {
                await context.SaveChangesAsync(cancellationToken);
                saveCounter = 0;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Refresh batch {Index} complete: {Dealers} dealers, {Inventory} inventory",
            batchIndex, result.DealersRefreshed, result.InventoryCreated);

        results.Add(result);
    }

    private DealerType[] GetDealerTypesForAirport(string airportType)
    {
        return airportType switch
        {
            LargeAirport => [
                DealerType.ManufacturerShowroom,
                DealerType.CertifiedPreOwned,
                DealerType.ExecutiveDealer,
                DealerType.CargoSpecialist
            ],
            MediumAirport => [
                DealerType.CertifiedPreOwned,
                DealerType.RegionalDealer,
                DealerType.FlightSchool,
                DealerType.BudgetLot
            ],
            SmallAirport => [
                DealerType.FlightSchool,
                DealerType.BudgetLot,
                DealerType.SpecialtyDealer
            ],
            // Heliports, seaplane bases, closed airports
            _ => [DealerType.SpecialtyDealer]
        };
    }

    private int GetTargetDealerCount(string airportType)
    {
        return airportType switch
        {
            LargeAirport => _config.DealersPerLargeAirport,
            MediumAirport => _config.DealersPerMediumAirport,
            SmallAirport => _config.DealersPerSmallAirport,
            _ => 1
        };
    }

    private AircraftDealer CreateDealer(Guid worldId, Airport airport, DealerType dealerType, Random random)
    {
        var name = GenerateDealerName(airport, dealerType, random);

        return dealerType switch
        {
            DealerType.ManufacturerShowroom => CreateManufacturerShowroom(worldId, airport.Ident, name),
            DealerType.CertifiedPreOwned => AircraftDealer.CreateCertifiedPreOwned(worldId, airport.Ident, name),
            DealerType.BudgetLot => AircraftDealer.CreateBudgetLot(worldId, airport.Ident, name),
            DealerType.FlightSchool => AircraftDealer.CreateFlightSchool(worldId, airport.Ident, name),
            DealerType.ExecutiveDealer => CreateExecutiveDealer(worldId, airport.Ident, name),
            DealerType.CargoSpecialist => CreateCargoSpecialist(worldId, airport.Ident, name),
            DealerType.RegionalDealer => CreateRegionalDealer(worldId, airport.Ident, name),
            DealerType.SpecialtyDealer => CreateSpecialtyDealer(worldId, airport.Ident, name),
            _ => AircraftDealer.CreateBudgetLot(worldId, airport.Ident, name)
        };
    }

    private static string GenerateDealerName(Airport airport, DealerType dealerType, Random random)
    {
        var prefix = DealerNamePrefixes[random.Next(DealerNamePrefixes.Length)];
        var suffix = DealerNameSuffixes[random.Next(DealerNameSuffixes.Length)];

        return dealerType switch
        {
            DealerType.ManufacturerShowroom => $"{prefix} Aircraft Factory Outlet",
            DealerType.FlightSchool => $"{airport.Municipality ?? airport.Ident} Flight Academy",
            DealerType.ExecutiveDealer => $"{prefix} Executive {suffix}",
            DealerType.CargoSpecialist => $"{prefix} Cargo Aircraft",
            DealerType.SpecialtyDealer => $"{prefix} Specialty {suffix}",
            _ => $"{prefix} {suffix}"
        };
    }

    private static AircraftDealer CreateManufacturerShowroom(Guid worldId, string airportIcao, string name)
    {
        return new AircraftDealer
        {
            WorldId = worldId,
            AirportIcao = airportIcao,
            DealerType = DealerType.ManufacturerShowroom,
            Name = name,
            Description = "Authorized dealer with brand new aircraft and full manufacturer warranty.",
            MinInventory = 5,
            MaxInventory = 15,
            InventoryRefreshDays = 14,
            PriceMultiplier = 1.0m,
            OffersFinancing = true,
            FinancingDownPaymentPercent = 0.10m,
            FinancingInterestRate = 0.035m,
            MinCondition = 100,
            MaxCondition = 100,
            MinHours = 0,
            MaxHours = 0,
            ReputationScore = 5.0,
            LastInventoryRefresh = DateTimeOffset.UtcNow
        };
    }

    private static AircraftDealer CreateExecutiveDealer(Guid worldId, string airportIcao, string name)
    {
        return new AircraftDealer
        {
            WorldId = worldId,
            AirportIcao = airportIcao,
            DealerType = DealerType.ExecutiveDealer,
            Name = name,
            Description = "Premium business jets and high-end turboprops for discerning buyers.",
            MinInventory = 3,
            MaxInventory = 10,
            InventoryRefreshDays = 10,
            PriceMultiplier = 0.90m,
            OffersFinancing = true,
            FinancingDownPaymentPercent = 0.20m,
            FinancingInterestRate = 0.04m,
            MinCondition = 85,
            MaxCondition = 100,
            MinHours = 0,
            MaxHours = 5000,
            ReputationScore = 4.5,
            LastInventoryRefresh = DateTimeOffset.UtcNow
        };
    }

    private static AircraftDealer CreateCargoSpecialist(Guid worldId, string airportIcao, string name)
    {
        return new AircraftDealer
        {
            WorldId = worldId,
            AirportIcao = airportIcao,
            DealerType = DealerType.CargoSpecialist,
            Name = name,
            Description = "Freighter aircraft and cargo conversions for commercial operations.",
            MinInventory = 3,
            MaxInventory = 12,
            InventoryRefreshDays = 7,
            PriceMultiplier = 0.70m,
            OffersFinancing = true,
            FinancingDownPaymentPercent = 0.25m,
            FinancingInterestRate = 0.055m,
            MinCondition = 70,
            MaxCondition = 90,
            MinHours = 2000,
            MaxHours = 12000,
            ReputationScore = 3.5,
            LastInventoryRefresh = DateTimeOffset.UtcNow
        };
    }

    private static AircraftDealer CreateRegionalDealer(Guid worldId, string airportIcao, string name)
    {
        return new AircraftDealer
        {
            WorldId = worldId,
            AirportIcao = airportIcao,
            DealerType = DealerType.RegionalDealer,
            Name = name,
            Description = "Regional aircraft for commuter and short-haul operations.",
            MinInventory = 5,
            MaxInventory = 20,
            InventoryRefreshDays = 7,
            PriceMultiplier = 0.75m,
            OffersFinancing = true,
            FinancingDownPaymentPercent = 0.15m,
            FinancingInterestRate = 0.05m,
            MinCondition = 70,
            MaxCondition = 100,
            MinHours = 0,
            MaxHours = 8000,
            ReputationScore = 3.8,
            LastInventoryRefresh = DateTimeOffset.UtcNow
        };
    }

    private static AircraftDealer CreateSpecialtyDealer(Guid worldId, string airportIcao, string name)
    {
        return new AircraftDealer
        {
            WorldId = worldId,
            AirportIcao = airportIcao,
            DealerType = DealerType.SpecialtyDealer,
            Name = name,
            Description = "Bush planes, floatplanes, and specialty aircraft for unique missions.",
            MinInventory = 3,
            MaxInventory = 10,
            InventoryRefreshDays = 10,
            PriceMultiplier = 0.80m,
            OffersFinancing = false,
            MinCondition = 65,
            MaxCondition = 95,
            MinHours = 500,
            MaxHours = 10000,
            ReputationScore = 4.0,
            LastInventoryRefresh = DateTimeOffset.UtcNow
        };
    }

    private int GenerateInventoryForDealer(
        AircraftDealer dealer,
        List<Aircraft> allAircraft,
        PilotLifeDbContext context,
        Random random)
    {
        // Determine how many to add
        var currentCount = dealer.Inventory?.Count(i => !i.IsSold) ?? 0;
        var targetCount = random.Next(dealer.MinInventory, dealer.MaxInventory + 1);
        var toAdd = Math.Max(0, targetCount - currentCount);

        if (toAdd == 0)
        {
            dealer.LastInventoryRefresh = DateTimeOffset.UtcNow;
            return 0;
        }

        // Filter aircraft suitable for this dealer
        var suitableAircraft = FilterAircraftForDealer(dealer.DealerType, allAircraft);

        if (suitableAircraft.Count == 0)
        {
            // Fallback to any aircraft if no suitable ones found
            suitableAircraft = allAircraft;
        }

        var inventoryAdded = 0;

        for (var i = 0; i < toAdd && suitableAircraft.Count > 0; i++)
        {
            var aircraft = suitableAircraft[random.Next(suitableAircraft.Count)];
            var inventory = CreateInventoryItem(dealer, aircraft, random);
            context.DealerInventory.Add(inventory);
            inventoryAdded++;
        }

        dealer.LastInventoryRefresh = DateTimeOffset.UtcNow;

        return inventoryAdded;
    }

    private static List<Aircraft> FilterAircraftForDealer(DealerType dealerType, List<Aircraft> allAircraft)
    {
        return dealerType switch
        {
            // Manufacturer showroom - all aircraft types
            DealerType.ManufacturerShowroom => allAircraft,

            // Flight school - single-engine piston trainers
            DealerType.FlightSchool => allAircraft
                .Where(a => a.NumberOfEngines == 1 &&
                            a.EngineType == 0 && // Piston
                            a.MaxGrossWeightLbs < 6000)
                .ToList(),

            // Executive - jets and turboprops
            DealerType.ExecutiveDealer => allAircraft
                .Where(a => a.EngineType >= 1 && // Jet or Turboprop
                            a.MaxGrossWeightLbs >= 5000)
                .ToList(),

            // Cargo - larger aircraft
            DealerType.CargoSpecialist => allAircraft
                .Where(a => a.MaxGrossWeightLbs >= 12500)
                .ToList(),

            // Regional - twin engine and turboprops
            DealerType.RegionalDealer => allAircraft
                .Where(a => a.NumberOfEngines >= 2 || a.EngineType == 1) // Turboprop
                .ToList(),

            // Budget - any aircraft (typically older/higher hours)
            DealerType.BudgetLot => allAircraft,

            // Certified Pre-Owned - multi-engine quality aircraft
            DealerType.CertifiedPreOwned => allAircraft
                .Where(a => a.NumberOfEngines >= 2 ||
                            a.EngineType >= 1 || // Turboprop or Jet
                            a.MaxGrossWeightLbs >= 4000)
                .ToList(),

            // Specialty - small/medium aircraft (bush planes, floats)
            DealerType.SpecialtyDealer => allAircraft
                .Where(a => a.MaxGrossWeightLbs < 12500)
                .ToList(),

            _ => allAircraft
        };
    }

    private static DealerInventory CreateInventoryItem(AircraftDealer dealer, Aircraft aircraft, Random random)
    {
        var isNew = dealer.DealerType == DealerType.ManufacturerShowroom ||
                    (dealer.MinCondition >= 95 && random.NextDouble() > 0.5);

        // Calculate base price (use empty weight as proxy for value if no price set)
        var basePrice = CalculateBasePrice(aircraft);

        if (isNew)
        {
            return DealerInventory.CreateNew(
                dealer.WorldId,
                dealer.Id,
                aircraft.Id,
                basePrice,
                basePrice * dealer.PriceMultiplier,
                warrantyMonths: 24
            );
        }

        // Generate used aircraft characteristics
        var condition = random.Next(dealer.MinCondition, dealer.MaxCondition + 1);
        var totalMinutes = random.Next(dealer.MinHours * 60, dealer.MaxHours * 60 + 1);
        var totalCycles = totalMinutes / 90; // Rough estimate: 1.5 hour average flight

        // Adjust price based on condition and hours
        var conditionFactor = condition / 100.0m;
        var hoursFactor = 1.0m - (totalMinutes / 60.0m / 20000m); // Depreciate up to 20k hours
        hoursFactor = Math.Max(0.3m, hoursFactor); // Don't go below 30%

        var listPrice = basePrice * dealer.PriceMultiplier * conditionFactor * hoursFactor;

        var hasWarranty = dealer.DealerType == DealerType.CertifiedPreOwned && condition >= 85;

        return DealerInventory.CreateUsed(
            dealer.WorldId,
            dealer.Id,
            aircraft.Id,
            basePrice,
            Math.Round(listPrice, 2),
            condition,
            totalMinutes,
            totalCycles,
            hasWarranty,
            hasWarranty ? 6 : null
        );
    }

    private static decimal CalculateBasePrice(Aircraft aircraft)
    {
        // Estimate price based on aircraft characteristics
        // This is a simplified model - real prices would come from aircraft data
        var baseValue = (decimal)aircraft.EmptyWeightLbs * 50m; // $50 per lb as base

        // Engine type multiplier
        var engineMultiplier = aircraft.EngineType switch
        {
            0 => 1.0m,   // Piston
            1 => 3.0m,   // Turboprop
            2 => 5.0m,   // Jet
            _ => 1.5m
        };

        // Multi-engine bonus
        var engineCountMultiplier = aircraft.NumberOfEngines > 1 ? 1.5m : 1.0m;

        // Speed factor (faster = more expensive)
        var speedFactor = 1.0m + (decimal)(aircraft.CruiseSpeedKts / 500.0);

        var price = baseValue * engineMultiplier * engineCountMultiplier * speedFactor;

        // Ensure reasonable bounds
        price = Math.Max(50000m, price);  // Minimum $50k
        price = Math.Min(50000000m, price); // Maximum $50M

        return Math.Round(price, -3); // Round to nearest thousand
    }

    /// <summary>
    /// Calculates the great-circle distance between two points using the Haversine formula.
    /// </summary>
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusNm = 3440.065; // Earth radius in nautical miles

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusNm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    // Result tracking classes
    private class BatchResult
    {
        public int BatchIndex { get; set; }
        public int AirportsProcessed { get; set; }
        public int DealersCreated { get; set; }
        public int InventoryCreated { get; set; }
    }

    private class RefreshResult
    {
        public int BatchIndex { get; set; }
        public int DealersRefreshed { get; set; }
        public int InventoryCreated { get; set; }
    }
}
