using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a violation that occurred during an exam.
/// </summary>
public class ExamViolation : BaseEntity
{
    /// <summary>
    /// The exam this violation belongs to.
    /// </summary>
    public Guid ExamId { get; set; }
    public LicenseExam Exam { get; set; } = null!;

    /// <summary>
    /// When the violation occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>
    /// Type of violation.
    /// </summary>
    public ViolationType Type { get; set; }

    /// <summary>
    /// The actual value that triggered the violation (e.g., 2.3G, 280kts).
    /// </summary>
    public float Value { get; set; }

    /// <summary>
    /// The threshold/limit that was exceeded.
    /// </summary>
    public float Threshold { get; set; }

    /// <summary>
    /// Points deducted for this violation.
    /// </summary>
    public int PointsDeducted { get; set; }

    /// <summary>
    /// Whether this violation caused immediate exam failure.
    /// </summary>
    public bool CausedFailure { get; set; }

    /// <summary>
    /// Latitude where the violation occurred.
    /// </summary>
    public double LatitudeAtViolation { get; set; }

    /// <summary>
    /// Longitude where the violation occurred.
    /// </summary>
    public double LongitudeAtViolation { get; set; }

    /// <summary>
    /// Altitude where the violation occurred.
    /// </summary>
    public int? AltitudeAtViolation { get; set; }

    /// <summary>
    /// Description of the violation.
    /// </summary>
    public string? Description { get; set; }
}
