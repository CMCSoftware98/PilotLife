using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents an aircraft dealer at an airport.
/// Dealers have different types that affect their inventory characteristics.
/// </summary>
public class AircraftDealer : BaseEntity
{
    /// <summary>
    /// The world this dealer exists in.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    /// <summary>
    /// ICAO code of the airport where this dealer is located.
    /// </summary>
    public string AirportIcao { get; set; } = string.Empty;

    /// <summary>
    /// Type of dealer which determines inventory characteristics.
    /// </summary>
    public DealerType DealerType { get; set; }

    /// <summary>
    /// Display name of the dealership.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the dealership.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Minimum number of aircraft in inventory.
    /// </summary>
    public int MinInventory { get; set; } = 5;

    /// <summary>
    /// Maximum number of aircraft in inventory.
    /// </summary>
    public int MaxInventory { get; set; } = 20;

    /// <summary>
    /// How often inventory refreshes (in game days).
    /// </summary>
    public int InventoryRefreshDays { get; set; } = 7;

    /// <summary>
    /// Price multiplier applied to base aircraft prices.
    /// </summary>
    public decimal PriceMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Whether this dealer offers financing options.
    /// </summary>
    public bool OffersFinancing { get; set; }

    /// <summary>
    /// Required down payment percentage for financing (0-1).
    /// </summary>
    public decimal? FinancingDownPaymentPercent { get; set; }

    /// <summary>
    /// Annual interest rate for financing (0-1).
    /// </summary>
    public decimal? FinancingInterestRate { get; set; }

    /// <summary>
    /// Minimum aircraft condition this dealer stocks.
    /// </summary>
    public int MinCondition { get; set; } = 60;

    /// <summary>
    /// Maximum aircraft condition this dealer stocks.
    /// </summary>
    public int MaxCondition { get; set; } = 100;

    /// <summary>
    /// Minimum aircraft hours this dealer stocks.
    /// </summary>
    public int MinHours { get; set; } = 0;

    /// <summary>
    /// Maximum aircraft hours this dealer stocks.
    /// </summary>
    public int MaxHours { get; set; } = 10000;

    /// <summary>
    /// Reputation score of this dealer (affects trade-in values).
    /// </summary>
    public double ReputationScore { get; set; } = 3.0;

    /// <summary>
    /// Total number of aircraft sold by this dealer.
    /// </summary>
    public int TotalSales { get; set; }

    /// <summary>
    /// When the inventory was last refreshed.
    /// </summary>
    public DateTimeOffset LastInventoryRefresh { get; set; }

    /// <summary>
    /// Whether this dealer is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Current inventory items at this dealer.
    /// </summary>
    public ICollection<DealerInventory> Inventory { get; set; } = new List<DealerInventory>();

    /// <summary>
    /// Creates a manufacturer showroom dealer.
    /// </summary>
    public static AircraftDealer CreateManufacturerShowroom(
        Guid worldId,
        string airportIcao,
        string manufacturerName)
    {
        return new AircraftDealer
        {
            WorldId = worldId,
            AirportIcao = airportIcao,
            DealerType = DealerType.ManufacturerShowroom,
            Name = $"{manufacturerName} Factory Showroom",
            Description = $"Authorized {manufacturerName} dealer with new aircraft and full warranty coverage.",
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

    /// <summary>
    /// Creates a certified pre-owned dealer.
    /// </summary>
    public static AircraftDealer CreateCertifiedPreOwned(
        Guid worldId,
        string airportIcao,
        string name)
    {
        return new AircraftDealer
        {
            WorldId = worldId,
            AirportIcao = airportIcao,
            DealerType = DealerType.CertifiedPreOwned,
            Name = name,
            Description = "Quality pre-owned aircraft with limited warranty and thorough inspection.",
            MinInventory = 8,
            MaxInventory = 25,
            InventoryRefreshDays = 7,
            PriceMultiplier = 0.80m,
            OffersFinancing = true,
            FinancingDownPaymentPercent = 0.15m,
            FinancingInterestRate = 0.045m,
            MinCondition = 80,
            MaxCondition = 95,
            MinHours = 500,
            MaxHours = 3000,
            ReputationScore = 4.0,
            LastInventoryRefresh = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a budget lot dealer.
    /// </summary>
    public static AircraftDealer CreateBudgetLot(
        Guid worldId,
        string airportIcao,
        string name)
    {
        return new AircraftDealer
        {
            WorldId = worldId,
            AirportIcao = airportIcao,
            DealerType = DealerType.BudgetLot,
            Name = name,
            Description = "Affordable aircraft sold as-is. Perfect for budget-conscious pilots.",
            MinInventory = 10,
            MaxInventory = 30,
            InventoryRefreshDays = 5,
            PriceMultiplier = 0.55m,
            OffersFinancing = true,
            FinancingDownPaymentPercent = 0.25m,
            FinancingInterestRate = 0.08m,
            MinCondition = 60,
            MaxCondition = 80,
            MinHours = 3000,
            MaxHours = 15000,
            ReputationScore = 2.5,
            LastInventoryRefresh = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a flight school dealer.
    /// </summary>
    public static AircraftDealer CreateFlightSchool(
        Guid worldId,
        string airportIcao,
        string name)
    {
        return new AircraftDealer
        {
            WorldId = worldId,
            AirportIcao = airportIcao,
            DealerType = DealerType.FlightSchool,
            Name = name,
            Description = "Training aircraft perfect for new pilots. Starter-friendly financing available.",
            MinInventory = 5,
            MaxInventory = 15,
            InventoryRefreshDays = 10,
            PriceMultiplier = 0.75m,
            OffersFinancing = true,
            FinancingDownPaymentPercent = 0.05m,
            FinancingInterestRate = 0.04m,
            MinCondition = 75,
            MaxCondition = 95,
            MinHours = 1000,
            MaxHours = 8000,
            ReputationScore = 4.5,
            LastInventoryRefresh = DateTimeOffset.UtcNow
        };
    }
}
