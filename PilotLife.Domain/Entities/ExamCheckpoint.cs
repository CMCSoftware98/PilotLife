using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a checkpoint/waypoint that must be reached during an exam.
/// </summary>
public class ExamCheckpoint : BaseEntity
{
    /// <summary>
    /// The exam this checkpoint belongs to.
    /// </summary>
    public Guid ExamId { get; set; }
    public LicenseExam Exam { get; set; } = null!;

    /// <summary>
    /// Order of this checkpoint in the route.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Name or identifier of the checkpoint (e.g., airport ICAO, VOR, waypoint name).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Latitude of the checkpoint.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude of the checkpoint.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Required altitude at this checkpoint (if any).
    /// </summary>
    public int? RequiredAltitudeFt { get; set; }

    /// <summary>
    /// Radius in nautical miles to consider the checkpoint reached.
    /// </summary>
    public double RadiusNm { get; set; } = 1.0;

    /// <summary>
    /// Whether this checkpoint was reached.
    /// </summary>
    public bool WasReached { get; set; }

    /// <summary>
    /// When this checkpoint was reached.
    /// </summary>
    public DateTimeOffset? ReachedAt { get; set; }

    /// <summary>
    /// Altitude when the checkpoint was reached.
    /// </summary>
    public int? AltitudeAtReach { get; set; }

    /// <summary>
    /// Speed in knots when the checkpoint was reached.
    /// </summary>
    public int? SpeedAtReachKts { get; set; }

    /// <summary>
    /// Points awarded for reaching this checkpoint correctly.
    /// </summary>
    public int PointsAwarded { get; set; }

    /// <summary>
    /// Maximum points available for this checkpoint.
    /// </summary>
    public int MaxPoints { get; set; }
}
