namespace PilotLife.Domain.Enums;

/// <summary>
/// Types of maintenance actions that can be performed on aircraft.
/// </summary>
public enum MaintenanceType
{
    /// <summary>
    /// Pre-flight inspection - basic safety check.
    /// </summary>
    PreFlight = 1,

    /// <summary>
    /// Annual inspection - comprehensive yearly check.
    /// </summary>
    AnnualInspection = 2,

    /// <summary>
    /// 100-hour inspection - required for commercial operations.
    /// </summary>
    HundredHourInspection = 3,

    /// <summary>
    /// Progressive inspection - phased maintenance program.
    /// </summary>
    ProgressiveInspection = 4,

    /// <summary>
    /// Minor repair - small fixes and adjustments.
    /// </summary>
    MinorRepair = 10,

    /// <summary>
    /// Major repair - significant structural or system repairs.
    /// </summary>
    MajorRepair = 11,

    /// <summary>
    /// Component replacement - swapping out parts.
    /// </summary>
    ComponentReplacement = 12,

    /// <summary>
    /// Engine overhaul - comprehensive engine rebuild.
    /// </summary>
    EngineOverhaul = 20,

    /// <summary>
    /// Propeller overhaul - prop rebuild/replacement.
    /// </summary>
    PropellerOverhaul = 21,

    /// <summary>
    /// Avionics update - software/hardware upgrades.
    /// </summary>
    AvionicsUpdate = 22,

    /// <summary>
    /// Landing gear service - gear inspection and service.
    /// </summary>
    LandingGearService = 23,

    /// <summary>
    /// Airworthiness directive compliance.
    /// </summary>
    AirworthinessDirective = 30,

    /// <summary>
    /// Service bulletin compliance.
    /// </summary>
    ServiceBulletin = 31,

    /// <summary>
    /// Damage repair from incident.
    /// </summary>
    DamageRepair = 40,

    /// <summary>
    /// Corrosion treatment.
    /// </summary>
    CorrosionTreatment = 41,

    /// <summary>
    /// Paint and cosmetic work.
    /// </summary>
    Cosmetic = 50
}
