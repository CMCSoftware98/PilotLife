namespace PilotLife.API.DTOs;

// ========================================
// Response DTOs
// ========================================

public class MaintenanceStatusResponse
{
    public required string AircraftId { get; set; }
    public required int OverallCondition { get; set; }
    public required bool IsAirworthy { get; set; }
    public required bool IsInMaintenance { get; set; }
    public required int HoursSinceLastInspection { get; set; }
    public required double HoursSinceLastInspectionHours { get; set; }
    public required bool InspectionDue { get; set; }
    public required bool AnnualInspectionDue { get; set; }
    public string? LastInspectionDate { get; set; }
    public string? LastAnnualInspectionDate { get; set; }
    public ActiveMaintenanceResponse? ActiveMaintenance { get; set; }
    public required List<ComponentStatusResponse> Components { get; set; }
    public required List<MaintenanceAlertResponse> Alerts { get; set; }
}

public class ActiveMaintenanceResponse
{
    public required string Id { get; set; }
    public required string MaintenanceType { get; set; }
    public required string Title { get; set; }
    public required string StartedAt { get; set; }
    public required int EstimatedDurationHours { get; set; }
    public required string EstimatedCompletionAt { get; set; }
    public required int ProgressPercent { get; set; }
}

public class ComponentStatusResponse
{
    public required string ComponentId { get; set; }
    public required string ComponentType { get; set; }
    public required int Condition { get; set; }
    public double? TboPercentUsed { get; set; }
    public double? LifePercentUsed { get; set; }
    public required bool NeedsAttention { get; set; }
    public required bool IsServiceable { get; set; }
}

public class MaintenanceAlertResponse
{
    public required string Severity { get; set; }
    public required string Message { get; set; }
    public required string RecommendedAction { get; set; }
    public string? ComponentId { get; set; }
}

public class MaintenanceOptionResponse
{
    public required string MaintenanceType { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required decimal EstimatedCost { get; set; }
    public required int EstimatedDurationHours { get; set; }
    public required int ConditionImprovement { get; set; }
    public required bool RequiredForAirworthy { get; set; }
    public string? ComponentId { get; set; }
    public decimal? WarrantyCoverage { get; set; }
    public decimal? InsuranceCoverage { get; set; }
}

public class MaintenanceResultResponse
{
    public required bool Success { get; set; }
    public string? MaintenanceLogId { get; set; }
    public string? Message { get; set; }
    public string? EstimatedCompletion { get; set; }
    public decimal? TotalCost { get; set; }
    public decimal? OutOfPocketCost { get; set; }
}

public class MaintenanceLogResponse
{
    public required string Id { get; set; }
    public required string MaintenanceType { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string PerformedAtAirport { get; set; }
    public string? FacilityName { get; set; }
    public required string StartedAt { get; set; }
    public string? CompletedAt { get; set; }
    public required int EstimatedDurationHours { get; set; }
    public int? ActualDurationHours { get; set; }
    public required decimal LaborCost { get; set; }
    public required decimal PartsCost { get; set; }
    public required decimal TotalCost { get; set; }
    public required bool CoveredByWarranty { get; set; }
    public required bool CoveredByInsurance { get; set; }
    public required bool IsCompleted { get; set; }
    public required int ConditionImprovement { get; set; }
    public int? ResultingCondition { get; set; }
    public required int AircraftFlightMinutesAtService { get; set; }
    public required double AircraftFlightHoursAtService { get; set; }
    public string? ComponentType { get; set; }
    public string? Notes { get; set; }
}

// ========================================
// Request DTOs
// ========================================

public class ScheduleMaintenanceRequest
{
    public required string MaintenanceType { get; set; }
    public string? ComponentId { get; set; }
}
