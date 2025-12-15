using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a license exam attempt by a player.
/// Exams are scheduled from the web app and tracked by the connector.
/// </summary>
public class LicenseExam : BaseEntity
{
    /// <summary>
    /// The player world taking this exam.
    /// </summary>
    public Guid PlayerWorldId { get; set; }
    public PlayerWorld PlayerWorld { get; set; } = null!;

    /// <summary>
    /// The world this exam is in.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    /// <summary>
    /// The type of license this exam is for.
    /// </summary>
    public Guid LicenseTypeId { get; set; }
    public LicenseType LicenseType { get; set; } = null!;

    // Exam Configuration

    /// <summary>
    /// Current status of the exam.
    /// </summary>
    public ExamStatus Status { get; set; } = ExamStatus.Scheduled;

    /// <summary>
    /// When the exam was scheduled.
    /// </summary>
    public DateTimeOffset ScheduledAt { get; set; }

    /// <summary>
    /// When the exam actually started (player spawned and began).
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// When the exam was completed (passed, failed, or abandoned).
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Time limit for this exam in minutes.
    /// </summary>
    public int TimeLimitMinutes { get; set; }

    // Requirements

    /// <summary>
    /// Required aircraft category for this exam.
    /// </summary>
    public AircraftCategory? RequiredAircraftCategory { get; set; }

    /// <summary>
    /// Specific aircraft type required (for type ratings).
    /// </summary>
    public string? RequiredAircraftType { get; set; }

    /// <summary>
    /// ICAO code of the departure airport.
    /// </summary>
    public string DepartureIcao { get; set; } = string.Empty;

    /// <summary>
    /// JSON containing the generated exam route (waypoints, checkpoints).
    /// </summary>
    public string? RouteJson { get; set; }

    /// <summary>
    /// Assigned altitude in feet for cruise portions.
    /// </summary>
    public int? AssignedAltitudeFt { get; set; }

    // Results

    /// <summary>
    /// Final score (0-100).
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Minimum passing score for this exam.
    /// </summary>
    public int PassingScore { get; set; } = 70;

    /// <summary>
    /// Reason for failure if applicable.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Auto-generated examiner feedback and notes.
    /// </summary>
    public string? ExaminerNotes { get; set; }

    // Attempt Tracking

    /// <summary>
    /// Which attempt number this is (1, 2, 3...).
    /// </summary>
    public int AttemptNumber { get; set; } = 1;

    /// <summary>
    /// Fee paid for this exam attempt.
    /// </summary>
    public decimal FeePaid { get; set; }

    /// <summary>
    /// When the player is eligible for a retake (cooldown period).
    /// </summary>
    public DateTimeOffset? EligibleForRetakeAt { get; set; }

    // Flight Tracking

    /// <summary>
    /// Total flight time during the exam in minutes.
    /// </summary>
    public int? FlightTimeMinutes { get; set; }

    /// <summary>
    /// Total distance flown during the exam in nautical miles.
    /// </summary>
    public double? DistanceFlownNm { get; set; }

    /// <summary>
    /// Aircraft used for the exam (ICAO type code).
    /// </summary>
    public string? AircraftUsed { get; set; }

    // Navigation Properties

    /// <summary>
    /// Maneuvers performed during the exam.
    /// </summary>
    public ICollection<ExamManeuver> Maneuvers { get; set; } = new List<ExamManeuver>();

    /// <summary>
    /// Checkpoints visited during the exam.
    /// </summary>
    public ICollection<ExamCheckpoint> Checkpoints { get; set; } = new List<ExamCheckpoint>();

    /// <summary>
    /// Landings performed during the exam.
    /// </summary>
    public ICollection<ExamLanding> Landings { get; set; } = new List<ExamLanding>();

    /// <summary>
    /// Violations recorded during the exam.
    /// </summary>
    public ICollection<ExamViolation> Violations { get; set; } = new List<ExamViolation>();

    /// <summary>
    /// License earned from this exam (if passed).
    /// </summary>
    public UserLicense? EarnedLicense { get; set; }
}
