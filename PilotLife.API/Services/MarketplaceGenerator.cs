using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PilotLife.Application.Marketplace;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services;

/// <summary>
/// Generates and populates marketplace dealers and inventory based on airport size.
/// </summary>
public class MarketplaceGenerator : IMarketplaceGenerator
{
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<MarketplaceGenerator> _logger;
    private readonly MarketplaceConfiguration _config;
    private readonly Random _random = new();

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
        PilotLifeDbContext context,
        ILogger<MarketplaceGenerator> logger,
        IOptions<MarketplaceConfiguration> config)
    {
        _context = context;
        _logger = logger;
        _config = config.Value;
    }

    public async Task PopulateWorldMarketplaceAsync(Guid worldId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting marketplace population for world {WorldId}", worldId);

        var world = await _context.Worlds.FindAsync([worldId], cancellationToken);
        if (world == null)
        {
            _logger.LogWarning("World {WorldId} not found", worldId);
            return;
        }

        // Get all approved aircraft
        var availableAircraft = await _context.Aircraft
            .Where(a => a.IsApproved)
            .ToListAsync(cancellationToken);

        if (availableAircraft.Count == 0)
        {
            _logger.LogWarning("No approved aircraft found in database");
            return;
        }

        // Get existing dealers for this world
        var existingDealersByAirport = await _context.AircraftDealers
            .Where(d => d.WorldId == worldId)
            .GroupBy(d => d.AirportIcao)
            .ToDictionaryAsync(g => g.Key, g => g.ToList(), cancellationToken);

        // Process airports in batches
        var totalAirports = await _context.Airports.CountAsync(cancellationToken);
        var processed = 0;
        var dealersCreated = 0;
        var inventoryCreated = 0;

        var batchSize = _config.AirportBatchSize;

        for (var skip = 0; skip < totalAirports; skip += batchSize)
        {
            var airports = await _context.Airports
                .OrderBy(a => a.Id)
                .Skip(skip)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            foreach (var airport in airports)
            {
                var dealerTypes = GetDealerTypesForAirport(airport.Type);
                var targetDealerCount = GetTargetDealerCount(airport.Type);

                // Get or create dealers for this airport
                existingDealersByAirport.TryGetValue(airport.Ident, out var existingDealers);
                existingDealers ??= [];

                // Create missing dealer types
                var existingTypes = existingDealers.Select(d => d.DealerType).ToHashSet();
                var dealersToCreate = new List<AircraftDealer>();

                foreach (var dealerType in dealerTypes.Take(targetDealerCount))
                {
                    if (!existingTypes.Contains(dealerType))
                    {
                        var dealer = CreateDealer(worldId, airport, dealerType);
                        dealersToCreate.Add(dealer);
                        dealersCreated++;
                    }
                }

                if (dealersToCreate.Count > 0)
                {
                    await _context.AircraftDealers.AddRangeAsync(dealersToCreate, cancellationToken);
                    existingDealers.AddRange(dealersToCreate);
                }

                // Generate inventory for dealers that need it
                foreach (var dealer in existingDealers)
                {
                    if (ShouldRefreshInventory(dealer))
                    {
                        var inventoryCount = await GenerateInventoryForDealerAsync(
                            dealer, availableAircraft, cancellationToken);
                        inventoryCreated += inventoryCount;
                    }
                }

                processed++;
            }

            // Save after each batch
            await _context.SaveChangesAsync(cancellationToken);

            if (processed % _config.ProgressLogInterval == 0)
            {
                _logger.LogInformation(
                    "Marketplace population progress: {Processed}/{Total} airports, {Dealers} dealers, {Inventory} inventory",
                    processed, totalAirports, dealersCreated, inventoryCreated);
            }
        }

        _logger.LogInformation(
            "Marketplace population complete for world {WorldId}: {Airports} airports, {Dealers} dealers, {Inventory} inventory items",
            worldId, processed, dealersCreated, inventoryCreated);
    }

    public async Task RefreshStaleInventoryAsync(Guid worldId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refreshing stale inventory for world {WorldId}", worldId);

        var availableAircraft = await _context.Aircraft
            .Where(a => a.IsApproved)
            .ToListAsync(cancellationToken);

        if (availableAircraft.Count == 0) return;

        var refreshThreshold = DateTimeOffset.UtcNow.AddDays(-_config.InventoryExpirationDays);

        // Get dealers needing refresh
        var staleDealers = await _context.AircraftDealers
            .Where(d => d.WorldId == worldId &&
                        d.IsActive &&
                        d.LastInventoryRefresh < refreshThreshold)
            .Include(d => d.Inventory.Where(i => !i.IsSold))
            .ToListAsync(cancellationToken);

        var refreshed = 0;
        var inventoryCreated = 0;

        foreach (var dealer in staleDealers)
        {
            var count = await GenerateInventoryForDealerAsync(dealer, availableAircraft, cancellationToken);
            inventoryCreated += count;
            refreshed++;

            if (refreshed % 100 == 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Refreshed {Dealers} dealers with {Inventory} new inventory items for world {WorldId}",
            refreshed, inventoryCreated, worldId);
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

    private AircraftDealer CreateDealer(Guid worldId, Airport airport, DealerType dealerType)
    {
        var name = GenerateDealerName(airport, dealerType);

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

    private string GenerateDealerName(Airport airport, DealerType dealerType)
    {
        var prefix = DealerNamePrefixes[_random.Next(DealerNamePrefixes.Length)];
        var suffix = DealerNameSuffixes[_random.Next(DealerNameSuffixes.Length)];

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

    private bool ShouldRefreshInventory(AircraftDealer dealer)
    {
        var daysSinceRefresh = (DateTimeOffset.UtcNow - dealer.LastInventoryRefresh).TotalDays;
        return daysSinceRefresh >= dealer.InventoryRefreshDays;
    }

    private async Task<int> GenerateInventoryForDealerAsync(
        AircraftDealer dealer,
        List<Aircraft> allAircraft,
        CancellationToken cancellationToken)
    {
        // Remove expired inventory
        var expiredInventory = await _context.DealerInventory
            .Where(i => i.DealerId == dealer.Id &&
                        !i.IsSold &&
                        i.ExpiresAt != null &&
                        i.ExpiresAt < DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);

        _context.DealerInventory.RemoveRange(expiredInventory);

        // Count current active inventory
        var currentCount = await _context.DealerInventory
            .CountAsync(i => i.DealerId == dealer.Id && !i.IsSold, cancellationToken);

        // Determine how many to add
        var targetCount = _random.Next(dealer.MinInventory, dealer.MaxInventory + 1);
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

        var inventoryToAdd = new List<DealerInventory>();

        for (var i = 0; i < toAdd && suitableAircraft.Count > 0; i++)
        {
            var aircraft = suitableAircraft[_random.Next(suitableAircraft.Count)];
            var inventory = CreateInventoryItem(dealer, aircraft);
            inventoryToAdd.Add(inventory);
        }

        await _context.DealerInventory.AddRangeAsync(inventoryToAdd, cancellationToken);
        dealer.LastInventoryRefresh = DateTimeOffset.UtcNow;

        return inventoryToAdd.Count;
    }

    private List<Aircraft> FilterAircraftForDealer(DealerType dealerType, List<Aircraft> allAircraft)
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

    private DealerInventory CreateInventoryItem(AircraftDealer dealer, Aircraft aircraft)
    {
        var isNew = dealer.DealerType == DealerType.ManufacturerShowroom ||
                    (dealer.MinCondition >= 95 && _random.NextDouble() > 0.5);

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
        var condition = _random.Next(dealer.MinCondition, dealer.MaxCondition + 1);
        var totalMinutes = _random.Next(dealer.MinHours * 60, dealer.MaxHours * 60 + 1);
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
}
