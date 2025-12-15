using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.Application.Maintenance;

/// <summary>
/// Service interface for aircraft maintenance operations.
/// </summary>
public interface IMaintenanceService
{
    /// <summary>
    /// Gets the maintenance status for an owned aircraft.
    /// </summary>
    Task<MaintenanceStatusResult> GetMaintenanceStatusAsync(Guid ownedAircraftId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available maintenance options for an aircraft at its current location.
    /// </summary>
    Task<IEnumerable<MaintenanceOption>> GetAvailableMaintenanceAsync(Guid ownedAircraftId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules maintenance for an aircraft.
    /// </summary>
    Task<MaintenanceResult> ScheduleMaintenanceAsync(Guid ownedAircraftId, MaintenanceType maintenanceType, Guid userId, Guid? componentId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a maintenance job (called when duration has elapsed).
    /// </summary>
    Task<MaintenanceResult> CompleteMaintenanceAsync(Guid maintenanceLogId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels scheduled maintenance.
    /// </summary>
    Task<MaintenanceResult> CancelMaintenanceAsync(Guid maintenanceLogId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets maintenance history for an aircraft.
    /// </summary>
    Task<IEnumerable<MaintenanceLog>> GetMaintenanceHistoryAsync(Guid ownedAircraftId, Guid userId, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies wear and tear to an aircraft after a flight.
    /// </summary>
    Task ApplyFlightWearAsync(Guid ownedAircraftId, int flightMinutes, int? landingRate, bool hadOverspeed = false, bool hadStallWarning = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an aircraft is currently in maintenance.
    /// </summary>
    Task<MaintenanceLog?> GetActiveMaintenanceAsync(Guid ownedAircraftId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a maintenance status query.
/// </summary>
public class MaintenanceStatusResult
{
    public required Guid OwnedAircraftId { get; set; }
    public required int OverallCondition { get; set; }
    public required bool IsAirworthy { get; set; }
    public required bool IsInMaintenance { get; set; }
    public required int HoursSinceLastInspection { get; set; }
    public required bool InspectionDue { get; set; }
    public required bool AnnualInspectionDue { get; set; }
    public DateTimeOffset? LastInspectionDate { get; set; }
    public DateTimeOffset? LastAnnualInspectionDate { get; set; }
    public MaintenanceLog? ActiveMaintenance { get; set; }
    public required List<ComponentStatus> Components { get; set; }
    public required List<MaintenanceAlert> Alerts { get; set; }
}

/// <summary>
/// Status of an aircraft component.
/// </summary>
public class ComponentStatus
{
    public required Guid ComponentId { get; set; }
    public required ComponentType ComponentType { get; set; }
    public required int Condition { get; set; }
    public double? TboPercentUsed { get; set; }
    public double? LifePercentUsed { get; set; }
    public required bool NeedsAttention { get; set; }
    public required bool IsServiceable { get; set; }
}

/// <summary>
/// An alert about aircraft maintenance needs.
/// </summary>
public class MaintenanceAlert
{
    public required string Severity { get; set; } // "warning", "critical"
    public required string Message { get; set; }
    public required string RecommendedAction { get; set; }
    public Guid? ComponentId { get; set; }
}

/// <summary>
/// A maintenance option available for an aircraft.
/// </summary>
public class MaintenanceOption
{
    public required MaintenanceType MaintenanceType { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required decimal EstimatedCost { get; set; }
    public required int EstimatedDurationHours { get; set; }
    public required int ConditionImprovement { get; set; }
    public required bool RequiredForAirworthy { get; set; }
    public Guid? ComponentId { get; set; }
    public decimal? WarrantyCoverage { get; set; }
    public decimal? InsuranceCoverage { get; set; }
}

/// <summary>
/// Result of a maintenance operation.
/// </summary>
public class MaintenanceResult
{
    public required bool Success { get; set; }
    public Guid? MaintenanceLogId { get; set; }
    public string? Message { get; set; }
    public DateTimeOffset? EstimatedCompletion { get; set; }
    public decimal? TotalCost { get; set; }
    public decimal? OutOfPocketCost { get; set; }
}
