using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents an event that affects a player's credit score.
/// Provides a complete audit trail of all credit score changes.
/// </summary>
public class CreditScoreEvent : BaseEntity
{
    /// <summary>
    /// The player whose credit score was affected.
    /// </summary>
    public Guid PlayerWorldId { get; set; }
    public PlayerWorld PlayerWorld { get; set; } = null!;

    /// <summary>
    /// The world this event occurred in.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    /// <summary>
    /// Type of event that caused the credit score change.
    /// </summary>
    public CreditScoreEventType EventType { get; set; }

    /// <summary>
    /// Credit score before this event.
    /// </summary>
    public int ScoreBefore { get; set; }

    /// <summary>
    /// Credit score after this event.
    /// </summary>
    public int ScoreAfter { get; set; }

    /// <summary>
    /// Change in credit score (positive or negative).
    /// </summary>
    public int ScoreChange { get; set; }

    /// <summary>
    /// Description of what caused this event.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Related loan (if applicable).
    /// </summary>
    public Guid? RelatedLoanId { get; set; }
    public Loan? RelatedLoan { get; set; }

    /// <summary>
    /// Related job (if applicable).
    /// </summary>
    public Guid? RelatedJobId { get; set; }
    public Job? RelatedJob { get; set; }

    /// <summary>
    /// Creates an initial credit score event when a player joins a world.
    /// </summary>
    public static CreditScoreEvent CreateInitial(Guid playerWorldId, Guid worldId, int initialScore = 650)
    {
        return new CreditScoreEvent
        {
            PlayerWorldId = playerWorldId,
            WorldId = worldId,
            EventType = CreditScoreEventType.Initial,
            ScoreBefore = 0,
            ScoreAfter = initialScore,
            ScoreChange = initialScore,
            Description = "Initial credit score assigned"
        };
    }

    /// <summary>
    /// Creates a credit score event for an on-time loan payment.
    /// </summary>
    public static CreditScoreEvent CreatePaymentOnTime(
        Guid playerWorldId,
        Guid worldId,
        int currentScore,
        Guid loanId,
        int pointsEarned = 5)
    {
        return new CreditScoreEvent
        {
            PlayerWorldId = playerWorldId,
            WorldId = worldId,
            EventType = CreditScoreEventType.PaymentOnTime,
            ScoreBefore = currentScore,
            ScoreAfter = Math.Min(850, currentScore + pointsEarned),
            ScoreChange = Math.Min(850 - currentScore, pointsEarned),
            Description = "On-time loan payment",
            RelatedLoanId = loanId
        };
    }

    /// <summary>
    /// Creates a credit score event for a late loan payment.
    /// </summary>
    public static CreditScoreEvent CreatePaymentLate(
        Guid playerWorldId,
        Guid worldId,
        int currentScore,
        Guid loanId,
        int daysLate,
        int pointsLost = 15)
    {
        return new CreditScoreEvent
        {
            PlayerWorldId = playerWorldId,
            WorldId = worldId,
            EventType = CreditScoreEventType.PaymentLate,
            ScoreBefore = currentScore,
            ScoreAfter = Math.Max(300, currentScore - pointsLost),
            ScoreChange = -Math.Min(currentScore - 300, pointsLost),
            Description = $"Late loan payment ({daysLate} days late)",
            RelatedLoanId = loanId
        };
    }

    /// <summary>
    /// Creates a credit score event for a missed loan payment.
    /// </summary>
    public static CreditScoreEvent CreatePaymentMissed(
        Guid playerWorldId,
        Guid worldId,
        int currentScore,
        Guid loanId,
        int pointsLost = 50)
    {
        return new CreditScoreEvent
        {
            PlayerWorldId = playerWorldId,
            WorldId = worldId,
            EventType = CreditScoreEventType.PaymentMissed,
            ScoreBefore = currentScore,
            ScoreAfter = Math.Max(300, currentScore - pointsLost),
            ScoreChange = -Math.Min(currentScore - 300, pointsLost),
            Description = "Missed loan payment",
            RelatedLoanId = loanId
        };
    }

    /// <summary>
    /// Creates a credit score event for paying off a loan.
    /// </summary>
    public static CreditScoreEvent CreateLoanPaidOff(
        Guid playerWorldId,
        Guid worldId,
        int currentScore,
        Guid loanId,
        int pointsEarned = 25)
    {
        return new CreditScoreEvent
        {
            PlayerWorldId = playerWorldId,
            WorldId = worldId,
            EventType = CreditScoreEventType.LoanPaidOff,
            ScoreBefore = currentScore,
            ScoreAfter = Math.Min(850, currentScore + pointsEarned),
            ScoreChange = Math.Min(850 - currentScore, pointsEarned),
            Description = "Loan fully paid off",
            RelatedLoanId = loanId
        };
    }

    /// <summary>
    /// Creates a credit score event for a loan default.
    /// </summary>
    public static CreditScoreEvent CreateLoanDefaulted(
        Guid playerWorldId,
        Guid worldId,
        int currentScore,
        Guid loanId,
        int pointsLost = 150)
    {
        return new CreditScoreEvent
        {
            PlayerWorldId = playerWorldId,
            WorldId = worldId,
            EventType = CreditScoreEventType.LoanDefaulted,
            ScoreBefore = currentScore,
            ScoreAfter = Math.Max(300, currentScore - pointsLost),
            ScoreChange = -Math.Min(currentScore - 300, pointsLost),
            Description = "Loan defaulted",
            RelatedLoanId = loanId
        };
    }

    /// <summary>
    /// Creates a credit score event for completing a job successfully.
    /// </summary>
    public static CreditScoreEvent CreateJobCompleted(
        Guid playerWorldId,
        Guid worldId,
        int currentScore,
        Guid jobId,
        int pointsEarned = 2)
    {
        return new CreditScoreEvent
        {
            PlayerWorldId = playerWorldId,
            WorldId = worldId,
            EventType = CreditScoreEventType.JobCompleted,
            ScoreBefore = currentScore,
            ScoreAfter = Math.Min(850, currentScore + pointsEarned),
            ScoreChange = Math.Min(850 - currentScore, pointsEarned),
            Description = "Job completed successfully",
            RelatedJobId = jobId
        };
    }

    /// <summary>
    /// Creates a credit score event for a failed/abandoned job.
    /// </summary>
    public static CreditScoreEvent CreateJobFailed(
        Guid playerWorldId,
        Guid worldId,
        int currentScore,
        Guid jobId,
        int pointsLost = 10)
    {
        return new CreditScoreEvent
        {
            PlayerWorldId = playerWorldId,
            WorldId = worldId,
            EventType = CreditScoreEventType.JobFailed,
            ScoreBefore = currentScore,
            ScoreAfter = Math.Max(300, currentScore - pointsLost),
            ScoreChange = -Math.Min(currentScore - 300, pointsLost),
            Description = "Job failed or abandoned",
            RelatedJobId = jobId
        };
    }

    /// <summary>
    /// Creates a credit score event for time-based recovery.
    /// Time recovery only applies to scores below the natural cap (650).
    /// Scores at or above the cap are not modified.
    /// </summary>
    public static CreditScoreEvent CreateTimeRecovery(
        Guid playerWorldId,
        Guid worldId,
        int currentScore,
        int pointsRecovered = 1)
    {
        // Time recovery only applies below the natural cap
        var newScore = currentScore >= 650
            ? currentScore
            : Math.Min(650, currentScore + pointsRecovered);
        var change = newScore - currentScore;

        return new CreditScoreEvent
        {
            PlayerWorldId = playerWorldId,
            WorldId = worldId,
            EventType = CreditScoreEventType.TimeRecovery,
            ScoreBefore = currentScore,
            ScoreAfter = newScore,
            ScoreChange = change,
            Description = "Time-based credit recovery"
        };
    }
}
