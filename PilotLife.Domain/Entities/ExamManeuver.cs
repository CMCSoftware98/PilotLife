using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a maneuver performed during a license exam.
/// Maneuvers are graded by the connector.
/// </summary>
public class ExamManeuver : BaseEntity
{
    /// <summary>
    /// The exam this maneuver belongs to.
    /// </summary>
    public Guid ExamId { get; set; }
    public LicenseExam Exam { get; set; } = null!;

    /// <summary>
    /// Type of maneuver (e.g., "Takeoff", "Landing", "SteepTurn", "Stall").
    /// </summary>
    public string ManeuverType { get; set; } = string.Empty;

    /// <summary>
    /// Order in which this maneuver should be performed.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Whether this maneuver is required to pass.
    /// </summary>
    public bool IsRequired { get; set; } = true;

    // Scoring

    /// <summary>
    /// Maximum points available for this maneuver.
    /// </summary>
    public int MaxPoints { get; set; }

    /// <summary>
    /// Points actually awarded.
    /// </summary>
    public int PointsAwarded { get; set; }

    /// <summary>
    /// Result of this maneuver.
    /// </summary>
    public ManeuverResult Result { get; set; } = ManeuverResult.NotAttempted;

    // Tolerances (what's allowed)

    /// <summary>
    /// Allowed altitude deviation in feet.
    /// </summary>
    public int? AltitudeToleranceFt { get; set; }

    /// <summary>
    /// Allowed heading deviation in degrees.
    /// </summary>
    public int? HeadingToleranceDeg { get; set; }

    /// <summary>
    /// Allowed speed deviation in knots.
    /// </summary>
    public int? SpeedToleranceKts { get; set; }

    // Actual Performance

    /// <summary>
    /// Actual altitude deviation in feet.
    /// </summary>
    public int? AltitudeDeviationFt { get; set; }

    /// <summary>
    /// Actual heading deviation in degrees.
    /// </summary>
    public int? HeadingDeviationDeg { get; set; }

    /// <summary>
    /// Actual speed deviation in knots.
    /// </summary>
    public int? SpeedDeviationKts { get; set; }

    /// <summary>
    /// When this maneuver was started.
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// When this maneuver was completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Additional notes about the maneuver performance.
    /// </summary>
    public string? Notes { get; set; }
}
