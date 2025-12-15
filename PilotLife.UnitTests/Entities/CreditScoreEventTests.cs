using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class CreditScoreEventTests
{
    [Fact]
    public void NewCreditScoreEvent_HasValidId()
    {
        var evt = new CreditScoreEvent();

        Assert.NotEqual(Guid.Empty, evt.Id);
    }

    [Fact]
    public void NewCreditScoreEvent_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var evt = new CreditScoreEvent();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(evt.CreatedAt, before, after);
    }

    [Fact]
    public void CreateInitial_SetsCorrectEventType()
    {
        var evt = CreditScoreEvent.CreateInitial(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(CreditScoreEventType.Initial, evt.EventType);
    }

    [Fact]
    public void CreateInitial_SetsDefaultScore()
    {
        var evt = CreditScoreEvent.CreateInitial(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(0, evt.ScoreBefore);
        Assert.Equal(650, evt.ScoreAfter);
        Assert.Equal(650, evt.ScoreChange);
    }

    [Fact]
    public void CreateInitial_AllowsCustomInitialScore()
    {
        var evt = CreditScoreEvent.CreateInitial(Guid.NewGuid(), Guid.NewGuid(), 700);

        Assert.Equal(700, evt.ScoreAfter);
        Assert.Equal(700, evt.ScoreChange);
    }

    [Fact]
    public void CreatePaymentOnTime_IncreasesScore()
    {
        var playerWorldId = Guid.NewGuid();
        var worldId = Guid.NewGuid();
        var loanId = Guid.NewGuid();
        var currentScore = 650;

        var evt = CreditScoreEvent.CreatePaymentOnTime(playerWorldId, worldId, currentScore, loanId);

        Assert.Equal(CreditScoreEventType.PaymentOnTime, evt.EventType);
        Assert.Equal(currentScore, evt.ScoreBefore);
        Assert.True(evt.ScoreAfter > evt.ScoreBefore);
        Assert.True(evt.ScoreChange > 0);
        Assert.Equal(loanId, evt.RelatedLoanId);
    }

    [Fact]
    public void CreatePaymentOnTime_CapsAtMaxScore()
    {
        var evt = CreditScoreEvent.CreatePaymentOnTime(
            Guid.NewGuid(),
            Guid.NewGuid(),
            848, // Near max
            Guid.NewGuid(),
            10); // Would be 858 without cap

        Assert.Equal(850, evt.ScoreAfter);
        Assert.Equal(2, evt.ScoreChange); // Only 2 points gained
    }

    [Fact]
    public void CreatePaymentLate_DecreasesScore()
    {
        var playerWorldId = Guid.NewGuid();
        var worldId = Guid.NewGuid();
        var loanId = Guid.NewGuid();
        var currentScore = 650;

        var evt = CreditScoreEvent.CreatePaymentLate(playerWorldId, worldId, currentScore, loanId, 5);

        Assert.Equal(CreditScoreEventType.PaymentLate, evt.EventType);
        Assert.Equal(currentScore, evt.ScoreBefore);
        Assert.True(evt.ScoreAfter < evt.ScoreBefore);
        Assert.True(evt.ScoreChange < 0);
        Assert.Contains("5 days late", evt.Description);
    }

    [Fact]
    public void CreatePaymentLate_CapsAtMinScore()
    {
        var evt = CreditScoreEvent.CreatePaymentLate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            310, // Near min
            Guid.NewGuid(),
            5,
            50); // Would be 260 without cap

        Assert.Equal(300, evt.ScoreAfter);
    }

    [Fact]
    public void CreatePaymentMissed_HasLargerPenalty()
    {
        var currentScore = 650;

        var lateEvt = CreditScoreEvent.CreatePaymentLate(
            Guid.NewGuid(), Guid.NewGuid(), currentScore, Guid.NewGuid(), 1);
        var missedEvt = CreditScoreEvent.CreatePaymentMissed(
            Guid.NewGuid(), Guid.NewGuid(), currentScore, Guid.NewGuid());

        Assert.True(Math.Abs(missedEvt.ScoreChange) > Math.Abs(lateEvt.ScoreChange));
    }

    [Fact]
    public void CreateLoanPaidOff_GivesBonus()
    {
        var currentScore = 650;

        var evt = CreditScoreEvent.CreateLoanPaidOff(
            Guid.NewGuid(),
            Guid.NewGuid(),
            currentScore,
            Guid.NewGuid());

        Assert.Equal(CreditScoreEventType.LoanPaidOff, evt.EventType);
        Assert.True(evt.ScoreChange > 0);
        Assert.Contains("paid off", evt.Description.ToLower());
    }

    [Fact]
    public void CreateLoanDefaulted_HasLargePenalty()
    {
        var currentScore = 650;

        var evt = CreditScoreEvent.CreateLoanDefaulted(
            Guid.NewGuid(),
            Guid.NewGuid(),
            currentScore,
            Guid.NewGuid());

        Assert.Equal(CreditScoreEventType.LoanDefaulted, evt.EventType);
        Assert.True(evt.ScoreChange < -100); // Large penalty
        Assert.Contains("defaulted", evt.Description.ToLower());
    }

    [Fact]
    public void CreateJobCompleted_IncreasesScore()
    {
        var currentScore = 650;
        var jobId = Guid.NewGuid();

        var evt = CreditScoreEvent.CreateJobCompleted(
            Guid.NewGuid(),
            Guid.NewGuid(),
            currentScore,
            jobId);

        Assert.Equal(CreditScoreEventType.JobCompleted, evt.EventType);
        Assert.True(evt.ScoreChange > 0);
        Assert.Equal(jobId, evt.RelatedJobId);
    }

    [Fact]
    public void CreateJobFailed_DecreasesScore()
    {
        var currentScore = 650;
        var jobId = Guid.NewGuid();

        var evt = CreditScoreEvent.CreateJobFailed(
            Guid.NewGuid(),
            Guid.NewGuid(),
            currentScore,
            jobId);

        Assert.Equal(CreditScoreEventType.JobFailed, evt.EventType);
        Assert.True(evt.ScoreChange < 0);
        Assert.Equal(jobId, evt.RelatedJobId);
    }

    [Fact]
    public void CreateTimeRecovery_CapsAtNaturalRecoveryLimit()
    {
        // Time recovery should only recover up to 650 (natural limit)
        var currentScore = 649;

        var evt = CreditScoreEvent.CreateTimeRecovery(
            Guid.NewGuid(),
            Guid.NewGuid(),
            currentScore,
            5); // Would be 654 without cap

        Assert.Equal(650, evt.ScoreAfter);
        Assert.Equal(1, evt.ScoreChange); // Only 1 point gained
    }

    [Fact]
    public void CreateTimeRecovery_DoesNotRecoverAboveCap()
    {
        var currentScore = 700; // Already above recovery cap

        var evt = CreditScoreEvent.CreateTimeRecovery(
            Guid.NewGuid(),
            Guid.NewGuid(),
            currentScore);

        Assert.Equal(700, evt.ScoreAfter);
        Assert.Equal(0, evt.ScoreChange);
    }
}
