namespace PilotLife.Domain.Enums;

/// <summary>
/// Types of events that affect player reputation.
/// </summary>
public enum ReputationEventType
{
    // Job-related events
    /// <summary>
    /// Job completed within the expected time frame.
    /// </summary>
    JobCompletedOnTime = 1,

    /// <summary>
    /// Job completed earlier than expected.
    /// </summary>
    JobCompletedEarly = 2,

    /// <summary>
    /// Job completed but after the deadline.
    /// </summary>
    JobCompletedLate = 3,

    /// <summary>
    /// Job failed to complete (cargo damaged, passengers not delivered).
    /// </summary>
    JobFailed = 4,

    /// <summary>
    /// Job cancelled after acceptance.
    /// </summary>
    JobCancelled = 5,

    // Landing-related events
    /// <summary>
    /// Exceptional landing (< 100 fpm descent rate).
    /// </summary>
    SmoothLanding = 10,

    /// <summary>
    /// Good landing (100-200 fpm descent rate).
    /// </summary>
    GoodLanding = 11,

    /// <summary>
    /// Hard landing (> 600 fpm descent rate).
    /// </summary>
    HardLanding = 12,

    // Safety violations
    /// <summary>
    /// Aircraft exceeded maximum speed limits.
    /// </summary>
    OverspeedViolation = 20,

    /// <summary>
    /// Aircraft entered stall condition.
    /// </summary>
    StallWarning = 21,

    /// <summary>
    /// General safety violation.
    /// </summary>
    SafetyViolation = 22,

    /// <summary>
    /// Flight resulted in crash/accident.
    /// </summary>
    Accident = 23,

    // Positive events
    /// <summary>
    /// Bonus reputation from special achievement.
    /// </summary>
    BonusReputation = 30,

    /// <summary>
    /// Completed a high-risk job successfully.
    /// </summary>
    HighRiskJobCompleted = 31,

    /// <summary>
    /// Completed VIP/special passenger job.
    /// </summary>
    VipJobCompleted = 32,

    // Administrative
    /// <summary>
    /// Reputation decay over time of inactivity.
    /// </summary>
    InactivityDecay = 40,

    /// <summary>
    /// Manual adjustment by system/admin.
    /// </summary>
    ManualAdjustment = 50
}
