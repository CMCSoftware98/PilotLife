using PilotLife.Domain.Entities;

namespace PilotLife.UnitTests.Entities;

public class FlightJobTests
{
    [Fact]
    public void NewFlightJob_HasValidId()
    {
        var flightJob = new FlightJob();

        Assert.NotEqual(Guid.Empty, flightJob.Id);
    }

    [Fact]
    public void NewFlightJob_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var flightJob = new FlightJob();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(flightJob.CreatedAt, before, after);
    }

    [Fact]
    public void NewFlightJob_DefaultsToNotCompleted()
    {
        var flightJob = new FlightJob();

        Assert.False(flightJob.IsCompleted);
    }

    [Fact]
    public void NewFlightJob_DefaultsToNotFailed()
    {
        var flightJob = new FlightJob();

        Assert.False(flightJob.IsFailed);
    }

    [Fact]
    public void NewFlightJob_DefaultsToZeroXp()
    {
        var flightJob = new FlightJob();

        Assert.Equal(0, flightJob.XpEarned);
    }

    [Fact]
    public void NewFlightJob_DefaultsToZeroReputationChange()
    {
        var flightJob = new FlightJob();

        Assert.Equal(0, flightJob.ReputationChange);
    }

    [Fact]
    public void NewFlightJob_NullablePropertiesAreNull()
    {
        var flightJob = new FlightJob();

        Assert.Null(flightJob.FailureReason);
        Assert.Null(flightJob.ActualPayout);
    }

    [Fact]
    public void FlightJob_CanSetCompletedState()
    {
        var flightJob = new FlightJob
        {
            IsCompleted = true,
            ActualPayout = 1500m,
            XpEarned = 100,
            ReputationChange = 5
        };

        Assert.True(flightJob.IsCompleted);
        Assert.False(flightJob.IsFailed);
        Assert.Equal(1500m, flightJob.ActualPayout);
        Assert.Equal(100, flightJob.XpEarned);
        Assert.Equal(5, flightJob.ReputationChange);
    }

    [Fact]
    public void FlightJob_CanSetFailedState()
    {
        var flightJob = new FlightJob
        {
            IsFailed = true,
            FailureReason = "Crashed on landing",
            ReputationChange = -10
        };

        Assert.True(flightJob.IsFailed);
        Assert.False(flightJob.IsCompleted);
        Assert.Equal("Crashed on landing", flightJob.FailureReason);
        Assert.Equal(-10, flightJob.ReputationChange);
    }

    [Fact]
    public void FlightJob_CanHaveNegativeReputationChange()
    {
        var flightJob = new FlightJob { ReputationChange = -25 };

        Assert.Equal(-25, flightJob.ReputationChange);
    }

    [Fact]
    public void FlightJob_CanAssociateWithTrackedFlight()
    {
        var flight = new TrackedFlight();
        var flightJob = new FlightJob
        {
            TrackedFlightId = flight.Id,
            TrackedFlight = flight
        };

        Assert.Equal(flight.Id, flightJob.TrackedFlightId);
        Assert.Same(flight, flightJob.TrackedFlight);
    }
}
