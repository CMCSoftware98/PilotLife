using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a maintenance event in an aircraft's service history.
/// Tracks inspections, repairs, overhauls, and other maintenance actions.
/// </summary>
public class MaintenanceLog : BaseEntity
{
    // World isolation
    /// <summary>
    /// The world this maintenance record belongs to.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    // Parent aircraft
    /// <summary>
    /// The aircraft this maintenance was performed on.
    /// </summary>
    public Guid OwnedAircraftId { get; set; }
    public OwnedAircraft OwnedAircraft { get; set; } = null!;

    // Optionally linked to a specific component
    /// <summary>
    /// The specific component this maintenance was for (null = whole aircraft).
    /// </summary>
    public Guid? AircraftComponentId { get; set; }
    public AircraftComponent? AircraftComponent { get; set; }

    // Maintenance details
    /// <summary>
    /// Type of maintenance performed.
    /// </summary>
    public MaintenanceType MaintenanceType { get; set; }

    /// <summary>
    /// Short title/summary of the maintenance.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of work performed.
    /// </summary>
    public string? Description { get; set; }

    // Timing
    /// <summary>
    /// When the maintenance was started.
    /// </summary>
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When the maintenance was completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Expected duration in hours.
    /// </summary>
    public int EstimatedDurationHours { get; set; }

    /// <summary>
    /// Actual duration in hours.
    /// </summary>
    public int? ActualDurationHours { get; set; }

    // Aircraft state at maintenance
    /// <summary>
    /// Aircraft total flight minutes when maintenance was performed.
    /// </summary>
    public int AircraftFlightMinutesAtService { get; set; }

    /// <summary>
    /// Aircraft total cycles when maintenance was performed.
    /// </summary>
    public int AircraftCyclesAtService { get; set; }

    // Location
    /// <summary>
    /// Airport where maintenance was performed (ICAO code).
    /// </summary>
    public string PerformedAtAirport { get; set; } = string.Empty;

    /// <summary>
    /// Name of the maintenance facility/shop.
    /// </summary>
    public string? FacilityName { get; set; }

    // Costs
    /// <summary>
    /// Cost of labor.
    /// </summary>
    public decimal LaborCost { get; set; }

    /// <summary>
    /// Cost of parts.
    /// </summary>
    public decimal PartsCost { get; set; }

    /// <summary>
    /// Total maintenance cost (labor + parts).
    /// </summary>
    public decimal TotalCost => LaborCost + PartsCost;

    /// <summary>
    /// Whether the cost was covered by warranty.
    /// </summary>
    public bool CoveredByWarranty { get; set; }

    /// <summary>
    /// Whether the cost was covered by insurance.
    /// </summary>
    public bool CoveredByInsurance { get; set; }

    // Results
    /// <summary>
    /// Whether the maintenance was completed successfully.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Condition improvement achieved (percentage points).
    /// </summary>
    public int ConditionImprovement { get; set; }

    /// <summary>
    /// New condition of aircraft/component after maintenance.
    /// </summary>
    public int? ResultingCondition { get; set; }

    /// <summary>
    /// Parts that were replaced (JSON or comma-separated list).
    /// </summary>
    public string? PartsReplaced { get; set; }

    // Squawks and findings
    /// <summary>
    /// Issues found during inspection (squawks).
    /// </summary>
    public string? SquawksFound { get; set; }

    /// <summary>
    /// Issues that were deferred for later repair.
    /// </summary>
    public string? DeferredItems { get; set; }

    // Compliance tracking
    /// <summary>
    /// Airworthiness Directive number if this was AD compliance.
    /// </summary>
    public string? AirworthinessDirectiveNumber { get; set; }

    /// <summary>
    /// Service Bulletin number if this was SB compliance.
    /// </summary>
    public string? ServiceBulletinNumber { get; set; }

    // Sign-off
    /// <summary>
    /// Name of mechanic who performed the work.
    /// </summary>
    public string? MechanicName { get; set; }

    /// <summary>
    /// License number of the mechanic.
    /// </summary>
    public string? MechanicLicense { get; set; }

    /// <summary>
    /// Inspector who signed off the work (if required).
    /// </summary>
    public string? InspectorName { get; set; }

    // Notes
    /// <summary>
    /// Additional notes or comments.
    /// </summary>
    public string? Notes { get; set; }

    // Next service
    /// <summary>
    /// When the next related service is due (flight minutes).
    /// </summary>
    public int? NextServiceDueMinutes { get; set; }

    /// <summary>
    /// When the next related service is due (calendar date).
    /// </summary>
    public DateTimeOffset? NextServiceDueDate { get; set; }

    /// <summary>
    /// Creates an inspection log entry.
    /// </summary>
    public static MaintenanceLog CreateInspection(
        Guid worldId,
        Guid ownedAircraftId,
        MaintenanceType inspectionType,
        string airportIcao,
        int aircraftMinutes,
        int aircraftCycles,
        decimal cost)
    {
        var title = inspectionType switch
        {
            MaintenanceType.AnnualInspection => "Annual Inspection",
            MaintenanceType.HundredHourInspection => "100-Hour Inspection",
            MaintenanceType.PreFlight => "Pre-Flight Inspection",
            MaintenanceType.ProgressiveInspection => "Progressive Inspection",
            _ => "Inspection"
        };

        return new MaintenanceLog
        {
            WorldId = worldId,
            OwnedAircraftId = ownedAircraftId,
            MaintenanceType = inspectionType,
            Title = title,
            PerformedAtAirport = airportIcao,
            AircraftFlightMinutesAtService = aircraftMinutes,
            AircraftCyclesAtService = aircraftCycles,
            LaborCost = cost,
            IsCompleted = true,
            CompletedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a repair log entry.
    /// </summary>
    public static MaintenanceLog CreateRepair(
        Guid worldId,
        Guid ownedAircraftId,
        string title,
        string description,
        string airportIcao,
        int aircraftMinutes,
        int aircraftCycles,
        decimal laborCost,
        decimal partsCost,
        int conditionImprovement,
        Guid? componentId = null)
    {
        return new MaintenanceLog
        {
            WorldId = worldId,
            OwnedAircraftId = ownedAircraftId,
            AircraftComponentId = componentId,
            MaintenanceType = MaintenanceType.MinorRepair,
            Title = title,
            Description = description,
            PerformedAtAirport = airportIcao,
            AircraftFlightMinutesAtService = aircraftMinutes,
            AircraftCyclesAtService = aircraftCycles,
            LaborCost = laborCost,
            PartsCost = partsCost,
            ConditionImprovement = conditionImprovement,
            IsCompleted = true,
            CompletedAt = DateTimeOffset.UtcNow
        };
    }
}
