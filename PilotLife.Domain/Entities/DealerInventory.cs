using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents an aircraft available for sale at a dealer.
/// </summary>
public class DealerInventory : BaseEntity
{
    /// <summary>
    /// The world this inventory item exists in.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    /// <summary>
    /// The dealer selling this aircraft.
    /// </summary>
    public Guid DealerId { get; set; }
    public AircraftDealer Dealer { get; set; } = null!;

    /// <summary>
    /// Reference to the aircraft template/type being sold.
    /// </summary>
    public Guid AircraftId { get; set; }
    public Aircraft Aircraft { get; set; } = null!;

    /// <summary>
    /// Pre-assigned registration for this aircraft.
    /// </summary>
    public string? Registration { get; set; }

    /// <summary>
    /// Current condition of the aircraft (0-100).
    /// </summary>
    public int Condition { get; set; } = 100;

    /// <summary>
    /// Total flight hours on the airframe.
    /// </summary>
    public int TotalFlightMinutes { get; set; }

    /// <summary>
    /// Total cycles (takeoffs/landings) on the airframe.
    /// </summary>
    public int TotalCycles { get; set; }

    /// <summary>
    /// Base MSRP of the aircraft type.
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Current listing price after adjustments.
    /// </summary>
    public decimal ListPrice { get; set; }

    /// <summary>
    /// Whether this is a new aircraft.
    /// </summary>
    public bool IsNew { get; set; }

    /// <summary>
    /// Whether this aircraft comes with a warranty.
    /// </summary>
    public bool HasWarranty { get; set; }

    /// <summary>
    /// Warranty duration in game months.
    /// </summary>
    public int? WarrantyMonths { get; set; }

    /// <summary>
    /// Description of included avionics package.
    /// </summary>
    public string? AvionicsPackage { get; set; }

    /// <summary>
    /// Description of any included modifications.
    /// </summary>
    public string? IncludedModifications { get; set; }

    /// <summary>
    /// Seller's notes about the aircraft.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// When this listing was created.
    /// </summary>
    public DateTimeOffset ListedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When this listing expires.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Whether this aircraft has been sold.
    /// </summary>
    public bool IsSold { get; set; }

    /// <summary>
    /// When the aircraft was sold.
    /// </summary>
    public DateTimeOffset? SoldAt { get; set; }

    /// <summary>
    /// Whether this listing is currently active.
    /// </summary>
    public bool IsActive => !IsSold && (ExpiresAt == null || ExpiresAt > DateTimeOffset.UtcNow);

    /// <summary>
    /// Total flight hours on the airframe.
    /// </summary>
    public double TotalFlightHours => TotalFlightMinutes / 60.0;

    /// <summary>
    /// Discount percentage from base price.
    /// </summary>
    public decimal DiscountPercent => BasePrice > 0
        ? Math.Round((1 - (ListPrice / BasePrice)) * 100, 1)
        : 0;

    /// <summary>
    /// Creates a new aircraft inventory item.
    /// </summary>
    public static DealerInventory CreateNew(
        Guid worldId,
        Guid dealerId,
        Guid aircraftId,
        decimal basePrice,
        decimal listPrice,
        int warrantyMonths = 12)
    {
        return new DealerInventory
        {
            WorldId = worldId,
            DealerId = dealerId,
            AircraftId = aircraftId,
            Condition = 100,
            TotalFlightMinutes = 0,
            TotalCycles = 0,
            BasePrice = basePrice,
            ListPrice = listPrice,
            IsNew = true,
            HasWarranty = true,
            WarrantyMonths = warrantyMonths,
            ListedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a used aircraft inventory item.
    /// </summary>
    public static DealerInventory CreateUsed(
        Guid worldId,
        Guid dealerId,
        Guid aircraftId,
        decimal basePrice,
        decimal listPrice,
        int condition,
        int totalFlightMinutes,
        int totalCycles,
        bool hasWarranty = false,
        int? warrantyMonths = null)
    {
        return new DealerInventory
        {
            WorldId = worldId,
            DealerId = dealerId,
            AircraftId = aircraftId,
            Condition = condition,
            TotalFlightMinutes = totalFlightMinutes,
            TotalCycles = totalCycles,
            BasePrice = basePrice,
            ListPrice = listPrice,
            IsNew = false,
            HasWarranty = hasWarranty,
            WarrantyMonths = warrantyMonths,
            ListedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Marks this inventory item as sold.
    /// </summary>
    public void MarkAsSold()
    {
        IsSold = true;
        SoldAt = DateTimeOffset.UtcNow;
    }
}
