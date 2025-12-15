using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a landing performed during an exam.
/// </summary>
public class ExamLanding : BaseEntity
{
    /// <summary>
    /// The exam this landing belongs to.
    /// </summary>
    public Guid ExamId { get; set; }
    public LicenseExam Exam { get; set; } = null!;

    /// <summary>
    /// Order of this landing (1, 2, 3...).
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// ICAO code of the airport where the landing occurred.
    /// </summary>
    public string AirportIcao { get; set; } = string.Empty;

    /// <summary>
    /// Type of landing performed.
    /// </summary>
    public LandingType Type { get; set; }

    // Landing Metrics

    /// <summary>
    /// Vertical speed at touchdown in feet per minute (negative = descending).
    /// </summary>
    public float VerticalSpeedFpm { get; set; }

    /// <summary>
    /// Deviation from runway centerline in feet.
    /// </summary>
    public float CenterlineDeviationFt { get; set; }

    /// <summary>
    /// Distance from runway threshold at touchdown in feet.
    /// </summary>
    public float TouchdownZoneDistanceFt { get; set; }

    /// <summary>
    /// Ground speed at touchdown in knots.
    /// </summary>
    public float? GroundSpeedKts { get; set; }

    /// <summary>
    /// Pitch angle at touchdown in degrees.
    /// </summary>
    public float? PitchDeg { get; set; }

    /// <summary>
    /// Bank angle at touchdown in degrees.
    /// </summary>
    public float? BankDeg { get; set; }

    /// <summary>
    /// Whether landing gear was down.
    /// </summary>
    public bool GearDown { get; set; } = true;

    /// <summary>
    /// Runway identifier used (e.g., "09L", "27R").
    /// </summary>
    public string? RunwayUsed { get; set; }

    // Scoring

    /// <summary>
    /// Points awarded for this landing.
    /// </summary>
    public int PointsAwarded { get; set; }

    /// <summary>
    /// Maximum points available for this landing.
    /// </summary>
    public int MaxPoints { get; set; }

    /// <summary>
    /// When the landing occurred.
    /// </summary>
    public DateTimeOffset LandedAt { get; set; }

    /// <summary>
    /// Examiner notes about this landing.
    /// </summary>
    public string? Notes { get; set; }
}
