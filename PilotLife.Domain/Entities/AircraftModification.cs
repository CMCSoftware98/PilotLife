using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a modification applied to an owned aircraft.
/// Modifications can change cargo capacity, performance, or capabilities.
/// </summary>
public class AircraftModification : BaseEntity
{
    // World isolation
    /// <summary>
    /// The world this modification belongs to.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    // Parent aircraft
    /// <summary>
    /// The aircraft this modification is installed on.
    /// </summary>
    public Guid OwnedAircraftId { get; set; }
    public OwnedAircraft OwnedAircraft { get; set; } = null!;

    // Modification details
    /// <summary>
    /// Type of modification.
    /// </summary>
    public ModificationType ModificationType { get; set; }

    /// <summary>
    /// Custom name or description of the modification.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Detailed description of what this modification does.
    /// </summary>
    public string? Description { get; set; }

    // Installation info
    /// <summary>
    /// When the modification was installed.
    /// </summary>
    public DateTimeOffset InstalledAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Cost to install this modification.
    /// </summary>
    public decimal InstallationCost { get; set; }

    /// <summary>
    /// Where the modification was installed (airport ICAO).
    /// </summary>
    public string? InstalledAtAirport { get; set; }

    // Status
    /// <summary>
    /// Whether the modification is currently active/functional.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the modification can be removed.
    /// </summary>
    public bool IsRemovable { get; set; } = true;

    /// <summary>
    /// Cost to remove this modification (if removable).
    /// </summary>
    public decimal? RemovalCost { get; set; }

    // Effects - Cargo conversions
    /// <summary>
    /// Cargo capacity change in lbs (positive = increase).
    /// </summary>
    public double CargoCapacityChangeLbs { get; set; }

    /// <summary>
    /// Passenger capacity change (typically negative for cargo conversions).
    /// </summary>
    public int PassengerCapacityChange { get; set; }

    // Effects - Performance
    /// <summary>
    /// Range change in nautical miles (positive = increase).
    /// </summary>
    public double RangeChangeNm { get; set; }

    /// <summary>
    /// Cruise speed change in knots (positive = increase).
    /// </summary>
    public double CruiseSpeedChangeKts { get; set; }

    /// <summary>
    /// Fuel consumption change as a multiplier (0.95 = 5% reduction).
    /// </summary>
    public decimal FuelConsumptionMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Takeoff distance change as a multiplier (0.8 = 20% shorter).
    /// </summary>
    public decimal TakeoffDistanceMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Landing distance change as a multiplier.
    /// </summary>
    public decimal LandingDistanceMultiplier { get; set; } = 1.0m;

    // Effects - Weight
    /// <summary>
    /// Empty weight change in lbs (typically positive for modifications).
    /// </summary>
    public double EmptyWeightChangeLbs { get; set; }

    /// <summary>
    /// Maximum gross weight change in lbs.
    /// </summary>
    public double MaxGrossWeightChangeLbs { get; set; }

    // Effects - Capabilities
    /// <summary>
    /// Whether this modification enables water operations.
    /// </summary>
    public bool EnablesWaterOperations { get; set; }

    /// <summary>
    /// Whether this modification enables snow/ice operations.
    /// </summary>
    public bool EnablesSnowOperations { get; set; }

    /// <summary>
    /// Whether this modification enables STOL (short field) operations.
    /// </summary>
    public bool EnablesStolOperations { get; set; }

    /// <summary>
    /// Whether this modification enables IFR operations.
    /// </summary>
    public bool EnablesIfrOperations { get; set; }

    /// <summary>
    /// Whether this modification enables night operations.
    /// </summary>
    public bool EnablesNightOperations { get; set; }

    /// <summary>
    /// Whether this modification enables known-icing operations.
    /// </summary>
    public bool EnablesKnownIcing { get; set; }

    // Maintenance requirements
    /// <summary>
    /// Additional maintenance cost per flight hour.
    /// </summary>
    public decimal AdditionalMaintenanceCostPerHour { get; set; }

    /// <summary>
    /// Special inspection interval required (in flight minutes).
    /// </summary>
    public int? SpecialInspectionIntervalMinutes { get; set; }

    // Notes
    /// <summary>
    /// Additional notes about this modification.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Creates a standard cargo conversion modification.
    /// </summary>
    public static AircraftModification CreateCargoConversion(
        Guid worldId,
        Guid ownedAircraftId,
        int conversionPercent,
        double cargoCapacityLbs,
        int passengerCapacity)
    {
        var modificationType = conversionPercent switch
        {
            25 => ModificationType.CargoConversion25,
            50 => ModificationType.CargoConversion50,
            75 => ModificationType.CargoConversion75,
            100 => ModificationType.CargoConversion100,
            _ => ModificationType.CargoConversion50
        };

        return new AircraftModification
        {
            WorldId = worldId,
            OwnedAircraftId = ownedAircraftId,
            ModificationType = modificationType,
            Name = $"{conversionPercent}% Cargo Conversion",
            Description = $"Converts {conversionPercent}% of passenger space to cargo capacity.",
            CargoCapacityChangeLbs = cargoCapacityLbs * (conversionPercent / 100.0),
            PassengerCapacityChange = -(int)(passengerCapacity * (conversionPercent / 100.0)),
            IsRemovable = true
        };
    }
}
