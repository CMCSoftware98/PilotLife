namespace PilotLife.Application.Maintenance;

/// <summary>
/// Configuration options for the aircraft maintenance system.
/// </summary>
public class MaintenanceConfiguration
{
    public const string SectionName = "Maintenance";

    // ========================================
    // Condition Degradation Settings
    // ========================================

    /// <summary>
    /// Base condition degradation per flight hour (percentage points).
    /// Default: 0.1% per hour.
    /// </summary>
    public double BaseConditionDegradationPerHour { get; set; } = 0.1;

    /// <summary>
    /// Additional condition penalty for hard landings (> 600 fpm).
    /// Default: 2%.
    /// </summary>
    public double HardLandingConditionPenalty { get; set; } = 2.0;

    /// <summary>
    /// Additional condition penalty for overspeed events.
    /// Default: 1%.
    /// </summary>
    public double OverspeedConditionPenalty { get; set; } = 1.0;

    /// <summary>
    /// Component degradation rate per flight hour (percentage points).
    /// Default: 0.15% per hour (components wear faster than airframe).
    /// </summary>
    public double ComponentDegradationPerHour { get; set; } = 0.15;

    // ========================================
    // Inspection Intervals
    // ========================================

    /// <summary>
    /// Annual inspection interval in days.
    /// Default: 365 days.
    /// </summary>
    public int AnnualInspectionIntervalDays { get; set; } = 365;

    /// <summary>
    /// 100-hour inspection interval in minutes.
    /// Default: 6000 minutes (100 hours).
    /// </summary>
    public int HundredHourInspectionMinutes { get; set; } = 6000;

    // ========================================
    // Cost Settings
    // ========================================

    /// <summary>
    /// Labor cost per hour in dollars.
    /// Default: $85/hour.
    /// </summary>
    public decimal LaborCostPerHour { get; set; } = 85m;

    /// <summary>
    /// Markup on parts cost.
    /// Default: 1.2 (20% markup).
    /// </summary>
    public decimal PartsCostMultiplier { get; set; } = 1.2m;

    /// <summary>
    /// Base cost for annual inspection.
    /// Default: $1,200.
    /// </summary>
    public decimal AnnualInspectionBaseCost { get; set; } = 1200m;

    /// <summary>
    /// Base cost for 100-hour inspection.
    /// Default: $600.
    /// </summary>
    public decimal HundredHourInspectionBaseCost { get; set; } = 600m;

    /// <summary>
    /// Base cost for minor repairs.
    /// Default: $250.
    /// </summary>
    public decimal MinorRepairBaseCost { get; set; } = 250m;

    /// <summary>
    /// Base cost for major repairs.
    /// Default: $2,500.
    /// </summary>
    public decimal MajorRepairBaseCost { get; set; } = 2500m;

    // ========================================
    // Duration Settings (hours)
    // ========================================

    /// <summary>
    /// Base duration for annual inspection in hours.
    /// Default: 12 hours.
    /// </summary>
    public int AnnualInspectionDurationHours { get; set; } = 12;

    /// <summary>
    /// Base duration for 100-hour inspection in hours.
    /// Default: 6 hours.
    /// </summary>
    public int HundredHourInspectionDurationHours { get; set; } = 6;

    /// <summary>
    /// Base duration for minor repairs in hours.
    /// Default: 2 hours.
    /// </summary>
    public int MinorRepairDurationHours { get; set; } = 2;

    /// <summary>
    /// Base duration for major repairs in hours.
    /// Default: 24 hours.
    /// </summary>
    public int MajorRepairDurationHours { get; set; } = 24;

    /// <summary>
    /// Base duration for engine overhaul in hours.
    /// Default: 60 hours.
    /// </summary>
    public int EngineOverhaulDurationHours { get; set; } = 60;

    // ========================================
    // Condition Improvement Settings
    // ========================================

    /// <summary>
    /// Condition improvement from annual inspection (percentage points).
    /// Default: 8%.
    /// </summary>
    public int AnnualInspectionConditionImprovement { get; set; } = 8;

    /// <summary>
    /// Condition improvement from 100-hour inspection (percentage points).
    /// Default: 4%.
    /// </summary>
    public int HundredHourInspectionConditionImprovement { get; set; } = 4;

    /// <summary>
    /// Condition improvement from minor repair (percentage points).
    /// Default: 5%.
    /// </summary>
    public int MinorRepairConditionImprovement { get; set; } = 5;

    /// <summary>
    /// Condition improvement from major repair (percentage points).
    /// Default: 15%.
    /// </summary>
    public int MajorRepairConditionImprovement { get; set; } = 15;

    // ========================================
    // Warranty & Insurance
    // ========================================

    /// <summary>
    /// Default warranty period for new aircraft in months.
    /// Default: 24 months.
    /// </summary>
    public int DefaultWarrantyMonths { get; set; } = 24;

    /// <summary>
    /// Percentage of costs covered by insurance.
    /// Default: 80%.
    /// </summary>
    public decimal InsuranceCoveragePercent { get; set; } = 80m;

    /// <summary>
    /// Percentage of costs covered by warranty.
    /// Default: 100%.
    /// </summary>
    public decimal WarrantyCoveragePercent { get; set; } = 100m;

    // ========================================
    // Airworthiness
    // ========================================

    /// <summary>
    /// Minimum condition percentage for aircraft to be airworthy.
    /// Default: 60%.
    /// </summary>
    public int MinAirworthyCondition { get; set; } = 60;

    /// <summary>
    /// Minimum component condition for it to be serviceable.
    /// Default: 40%.
    /// </summary>
    public int MinServiceableComponentCondition { get; set; } = 40;
}
