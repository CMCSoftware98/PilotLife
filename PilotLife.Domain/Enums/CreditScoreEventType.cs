namespace PilotLife.Domain.Enums;

/// <summary>
/// Types of events that affect a player's credit score.
/// </summary>
public enum CreditScoreEventType
{
    /// <summary>
    /// Initial credit score when joining a world.
    /// </summary>
    Initial,

    /// <summary>
    /// On-time loan payment made.
    /// </summary>
    PaymentOnTime,

    /// <summary>
    /// Late loan payment made.
    /// </summary>
    PaymentLate,

    /// <summary>
    /// Missed loan payment.
    /// </summary>
    PaymentMissed,

    /// <summary>
    /// Loan fully paid off.
    /// </summary>
    LoanPaidOff,

    /// <summary>
    /// Loan defaulted.
    /// </summary>
    LoanDefaulted,

    /// <summary>
    /// New loan opened.
    /// </summary>
    LoanOpened,

    /// <summary>
    /// Job completed successfully.
    /// </summary>
    JobCompleted,

    /// <summary>
    /// Job failed or abandoned.
    /// </summary>
    JobFailed,

    /// <summary>
    /// Violation or fine received.
    /// </summary>
    Violation,

    /// <summary>
    /// Exam passed.
    /// </summary>
    ExamPassed,

    /// <summary>
    /// Regular time-based credit recovery.
    /// </summary>
    TimeRecovery,

    /// <summary>
    /// Administrative adjustment.
    /// </summary>
    AdminAdjustment
}
