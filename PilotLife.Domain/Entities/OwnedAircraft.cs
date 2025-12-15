using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a player-owned aircraft instance within a specific world.
/// Each owned aircraft is tied to a world and has its own condition, hours, and location.
/// </summary>
public class OwnedAircraft : BaseEntity
{
    // World isolation
    /// <summary>
    /// The world this aircraft belongs to.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    // Ownership
    /// <summary>
    /// The player who owns this aircraft in this world.
    /// </summary>
    public Guid PlayerWorldId { get; set; }
    public PlayerWorld Owner { get; set; } = null!;

    // Aircraft template reference
    /// <summary>
    /// Reference to the aircraft template (master data).
    /// </summary>
    public Guid AircraftId { get; set; }
    public Aircraft Aircraft { get; set; } = null!;

    // Identification
    /// <summary>
    /// Custom registration/tail number (e.g., "N12345", "G-ABCD").
    /// </summary>
    public string? Registration { get; set; }

    /// <summary>
    /// Custom name given by the player (e.g., "Spirit of Adventure").
    /// </summary>
    public string? Nickname { get; set; }

    // Condition & Hours
    /// <summary>
    /// Overall aircraft condition percentage (0-100).
    /// 100 = new/perfect, 0 = unairworthy.
    /// </summary>
    public int Condition { get; set; } = 100;

    /// <summary>
    /// Total airframe hours (flight time in minutes).
    /// </summary>
    public int TotalFlightMinutes { get; set; }

    /// <summary>
    /// Total number of flight cycles (takeoffs/landings).
    /// </summary>
    public int TotalCycles { get; set; }

    /// <summary>
    /// Hours since last major maintenance/inspection.
    /// </summary>
    public int HoursSinceLastInspection { get; set; }

    // Location
    /// <summary>
    /// ICAO code of the airport where the aircraft is currently parked.
    /// </summary>
    public string CurrentLocationIcao { get; set; } = string.Empty;

    /// <summary>
    /// When the aircraft was last moved to its current location.
    /// </summary>
    public DateTimeOffset LastMovedAt { get; set; } = DateTimeOffset.UtcNow;

    // Status flags
    /// <summary>
    /// Whether the aircraft is currently undergoing maintenance.
    /// </summary>
    public bool IsInMaintenance { get; set; }

    /// <summary>
    /// Whether the aircraft is currently in use (being flown).
    /// </summary>
    public bool IsInUse { get; set; }

    /// <summary>
    /// Whether the aircraft is listed for rental by other players.
    /// </summary>
    public bool IsListedForRental { get; set; }

    /// <summary>
    /// Whether the aircraft is listed for sale (auction).
    /// </summary>
    public bool IsListedForSale { get; set; }

    /// <summary>
    /// The asking price when listed for sale.
    /// </summary>
    public decimal? AskingPrice { get; set; }

    /// <summary>
    /// Whether the aircraft is airworthy (meets minimum condition requirements).
    /// </summary>
    public bool IsAirworthy => Condition >= 60 && !IsInMaintenance;

    // Purchase information
    /// <summary>
    /// Price paid when purchasing this aircraft.
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// When the aircraft was purchased.
    /// </summary>
    public DateTimeOffset PurchasedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Whether this aircraft was purchased new.
    /// </summary>
    public bool WasPurchasedNew { get; set; }

    // Warranty
    /// <summary>
    /// Whether the aircraft still has warranty coverage.
    /// </summary>
    public bool HasWarranty { get; set; }

    /// <summary>
    /// When the warranty expires.
    /// </summary>
    public DateTimeOffset? WarrantyExpiresAt { get; set; }

    // Insurance
    /// <summary>
    /// Whether the aircraft has active insurance.
    /// </summary>
    public bool HasInsurance { get; set; }

    /// <summary>
    /// When the insurance expires.
    /// </summary>
    public DateTimeOffset? InsuranceExpiresAt { get; set; }

    /// <summary>
    /// Monthly insurance premium.
    /// </summary>
    public decimal InsurancePremium { get; set; }

    // Navigation properties
    /// <summary>
    /// Components that make up this aircraft (engines, wings, etc.).
    /// </summary>
    public ICollection<AircraftComponent> Components { get; set; } = new List<AircraftComponent>();

    /// <summary>
    /// Modifications applied to this aircraft.
    /// </summary>
    public ICollection<AircraftModification> Modifications { get; set; } = new List<AircraftModification>();

    /// <summary>
    /// Maintenance history for this aircraft.
    /// </summary>
    public ICollection<MaintenanceLog> MaintenanceLogs { get; set; } = new List<MaintenanceLog>();

    // Calculated properties
    /// <summary>
    /// Gets total flight hours (minutes converted to hours).
    /// </summary>
    public double TotalFlightHours => TotalFlightMinutes / 60.0;

    /// <summary>
    /// Gets whether an inspection is due (every 100 hours for commercial use).
    /// </summary>
    public bool InspectionDue => HoursSinceLastInspection >= 6000; // 100 hours in minutes

    /// <summary>
    /// Calculates current market value based on condition, hours, and base price.
    /// </summary>
    public decimal EstimatedValue(decimal basePrice)
    {
        // Depreciation based on condition (0-100%)
        var conditionFactor = Condition / 100.0m;

        // Depreciation based on hours (roughly 5% per 1000 hours, max 50%)
        var hoursFactor = Math.Max(0.5m, 1.0m - (decimal)(TotalFlightHours / 1000.0 * 0.05));

        return basePrice * conditionFactor * hoursFactor;
    }
}
