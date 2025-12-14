using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a specific component of an owned aircraft with its own condition tracking.
/// Components degrade independently and require separate maintenance.
/// </summary>
public class AircraftComponent : BaseEntity
{
    // World isolation
    /// <summary>
    /// The world this component belongs to.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    // Parent aircraft
    /// <summary>
    /// The aircraft this component belongs to.
    /// </summary>
    public Guid OwnedAircraftId { get; set; }
    public OwnedAircraft OwnedAircraft { get; set; } = null!;

    // Component identification
    /// <summary>
    /// Type of component (Engine1, Wings, LandingGear, etc.).
    /// </summary>
    public ComponentType ComponentType { get; set; }

    /// <summary>
    /// Serial number or identifier for this specific component.
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Manufacturer of this component.
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Model designation of this component.
    /// </summary>
    public string? Model { get; set; }

    // Condition tracking
    /// <summary>
    /// Current condition percentage (0-100).
    /// 100 = new/perfect, 0 = failed/unusable.
    /// </summary>
    public int Condition { get; set; } = 100;

    /// <summary>
    /// Operating hours on this specific component.
    /// </summary>
    public int OperatingMinutes { get; set; }

    /// <summary>
    /// Number of cycles (start/stop for engines, extend/retract for gear, etc.).
    /// </summary>
    public int Cycles { get; set; }

    /// <summary>
    /// Time since last overhaul (TSOH) in minutes.
    /// </summary>
    public int TimeSinceOverhaul { get; set; }

    /// <summary>
    /// Time between overhaul (TBO) limit in minutes. Null = no limit.
    /// </summary>
    public int? TimeBetweenOverhaul { get; set; }

    // Life limits
    /// <summary>
    /// Whether this component is life-limited (must be replaced after certain hours/cycles).
    /// </summary>
    public bool IsLifeLimited { get; set; }

    /// <summary>
    /// Total life limit in operating minutes. Null = unlimited.
    /// </summary>
    public int? LifeLimitMinutes { get; set; }

    /// <summary>
    /// Total life limit in cycles. Null = unlimited.
    /// </summary>
    public int? LifeLimitCycles { get; set; }

    // Installation info
    /// <summary>
    /// When this component was installed on the aircraft.
    /// </summary>
    public DateTimeOffset InstalledAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Aircraft hours when this component was installed.
    /// </summary>
    public int AircraftHoursAtInstall { get; set; }

    // Status
    /// <summary>
    /// Whether the component is serviceable.
    /// </summary>
    public bool IsServiceable { get; set; } = true;

    /// <summary>
    /// Notes about component condition or issues.
    /// </summary>
    public string? Notes { get; set; }

    // Calculated properties
    /// <summary>
    /// Gets operating hours.
    /// </summary>
    public double OperatingHours => OperatingMinutes / 60.0;

    /// <summary>
    /// Gets time since overhaul in hours.
    /// </summary>
    public double TimeSinceOverhaulHours => TimeSinceOverhaul / 60.0;

    /// <summary>
    /// Gets percentage of TBO used. Returns null if no TBO limit.
    /// </summary>
    public double? TboPercentUsed => TimeBetweenOverhaul.HasValue
        ? (double)TimeSinceOverhaul / TimeBetweenOverhaul.Value * 100
        : null;

    /// <summary>
    /// Gets whether the component is approaching TBO (within 10%).
    /// </summary>
    public bool IsTboApproaching => TboPercentUsed.HasValue && TboPercentUsed >= 90;

    /// <summary>
    /// Gets whether the component has exceeded TBO.
    /// </summary>
    public bool IsTboExceeded => TboPercentUsed.HasValue && TboPercentUsed >= 100;

    /// <summary>
    /// Gets percentage of life limit used. Returns null if not life-limited.
    /// </summary>
    public double? LifePercentUsed
    {
        get
        {
            if (!IsLifeLimited) return null;

            var hoursPercent = LifeLimitMinutes.HasValue
                ? (double)OperatingMinutes / LifeLimitMinutes.Value * 100
                : 0;

            var cyclesPercent = LifeLimitCycles.HasValue
                ? (double)Cycles / LifeLimitCycles.Value * 100
                : 0;

            return Math.Max(hoursPercent, cyclesPercent);
        }
    }

    /// <summary>
    /// Gets whether component needs attention (low condition or approaching limits).
    /// </summary>
    public bool NeedsAttention => Condition < 70 || IsTboApproaching || (LifePercentUsed ?? 0) >= 90;

    /// <summary>
    /// Creates default components for an aircraft based on number of engines.
    /// </summary>
    public static List<AircraftComponent> CreateDefaultComponents(Guid worldId, Guid ownedAircraftId, int numberOfEngines)
    {
        var components = new List<AircraftComponent>
        {
            new() { WorldId = worldId, OwnedAircraftId = ownedAircraftId, ComponentType = ComponentType.Wings },
            new() { WorldId = worldId, OwnedAircraftId = ownedAircraftId, ComponentType = ComponentType.LandingGear },
            new() { WorldId = worldId, OwnedAircraftId = ownedAircraftId, ComponentType = ComponentType.Fuselage },
            new() { WorldId = worldId, OwnedAircraftId = ownedAircraftId, ComponentType = ComponentType.Avionics },
        };

        // Add engines based on count
        for (int i = 1; i <= numberOfEngines && i <= 6; i++)
        {
            var engineType = (ComponentType)i; // Engine1 = 1, Engine2 = 2, etc.
            components.Add(new AircraftComponent
            {
                WorldId = worldId,
                OwnedAircraftId = ownedAircraftId,
                ComponentType = engineType,
                TimeBetweenOverhaul = 120000 // 2000 hours default TBO for engines
            });
        }

        return components;
    }
}
