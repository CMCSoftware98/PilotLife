using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class ReputationEventTests
{
    [Fact]
    public void NewReputationEvent_HasValidId()
    {
        var reputationEvent = new ReputationEvent();

        Assert.NotEqual(Guid.Empty, reputationEvent.Id);
    }

    [Fact]
    public void NewReputationEvent_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var reputationEvent = new ReputationEvent();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(reputationEvent.CreatedAt, before, after);
    }

    [Fact]
    public void NewReputationEvent_HasOccurredAtSet()
    {
        var before = DateTimeOffset.UtcNow;
        var reputationEvent = new ReputationEvent();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(reputationEvent.OccurredAt, before, after);
    }

    [Fact]
    public void NewReputationEvent_HasDefaultDescription()
    {
        var reputationEvent = new ReputationEvent();

        Assert.Equal(string.Empty, reputationEvent.Description);
    }

    [Fact]
    public void NewReputationEvent_HasNoRelatedJobOrFlight()
    {
        var reputationEvent = new ReputationEvent();

        Assert.Null(reputationEvent.RelatedJobId);
        Assert.Null(reputationEvent.RelatedJob);
        Assert.Null(reputationEvent.RelatedFlightId);
        Assert.Null(reputationEvent.RelatedFlight);
    }

    [Theory]
    [InlineData(ReputationEventType.JobCompletedOnTime)]
    [InlineData(ReputationEventType.JobCompletedEarly)]
    [InlineData(ReputationEventType.JobCompletedLate)]
    [InlineData(ReputationEventType.JobFailed)]
    [InlineData(ReputationEventType.SmoothLanding)]
    [InlineData(ReputationEventType.HardLanding)]
    [InlineData(ReputationEventType.Accident)]
    public void EventType_CanBeSet(ReputationEventType eventType)
    {
        var reputationEvent = new ReputationEvent { EventType = eventType };

        Assert.Equal(eventType, reputationEvent.EventType);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(-0.1)]
    [InlineData(0.5)]
    [InlineData(-0.5)]
    public void PointChange_CanBePositiveOrNegative(decimal pointChange)
    {
        var reputationEvent = new ReputationEvent { PointChange = pointChange };

        Assert.Equal(pointChange, reputationEvent.PointChange);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(2.5)]
    [InlineData(5.0)]
    public void ResultingScore_CanBeInRange(decimal score)
    {
        var reputationEvent = new ReputationEvent { ResultingScore = score };

        Assert.Equal(score, reputationEvent.ResultingScore);
    }

    [Fact]
    public void CreateJobEvent_SetsAllProperties()
    {
        var playerWorldId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var eventType = ReputationEventType.JobCompletedOnTime;
        var pointChange = 0.1m;
        var resultingScore = 3.1m;
        var description = "Job completed on time";

        var reputationEvent = ReputationEvent.CreateJobEvent(
            playerWorldId, eventType, pointChange, resultingScore, jobId, description);

        Assert.Equal(playerWorldId, reputationEvent.PlayerWorldId);
        Assert.Equal(eventType, reputationEvent.EventType);
        Assert.Equal(pointChange, reputationEvent.PointChange);
        Assert.Equal(resultingScore, reputationEvent.ResultingScore);
        Assert.Equal(jobId, reputationEvent.RelatedJobId);
        Assert.Equal(description, reputationEvent.Description);
        Assert.Null(reputationEvent.RelatedFlightId);
    }

    [Fact]
    public void CreateFlightEvent_SetsAllProperties()
    {
        var playerWorldId = Guid.NewGuid();
        var flightId = Guid.NewGuid();
        var eventType = ReputationEventType.SmoothLanding;
        var pointChange = 0.02m;
        var resultingScore = 3.02m;
        var description = "Smooth landing at KJFK";

        var reputationEvent = ReputationEvent.CreateFlightEvent(
            playerWorldId, eventType, pointChange, resultingScore, flightId, description);

        Assert.Equal(playerWorldId, reputationEvent.PlayerWorldId);
        Assert.Equal(eventType, reputationEvent.EventType);
        Assert.Equal(pointChange, reputationEvent.PointChange);
        Assert.Equal(resultingScore, reputationEvent.ResultingScore);
        Assert.Equal(flightId, reputationEvent.RelatedFlightId);
        Assert.Equal(description, reputationEvent.Description);
        Assert.Null(reputationEvent.RelatedJobId);
    }

    [Fact]
    public void CreateJobEvent_HasValidId()
    {
        var reputationEvent = ReputationEvent.CreateJobEvent(
            Guid.NewGuid(), ReputationEventType.JobCompletedOnTime, 0.1m, 3.1m, Guid.NewGuid(), "Test");

        Assert.NotEqual(Guid.Empty, reputationEvent.Id);
    }

    [Fact]
    public void CreateFlightEvent_HasValidId()
    {
        var reputationEvent = ReputationEvent.CreateFlightEvent(
            Guid.NewGuid(), ReputationEventType.SmoothLanding, 0.02m, 3.02m, Guid.NewGuid(), "Test");

        Assert.NotEqual(Guid.Empty, reputationEvent.Id);
    }
}
