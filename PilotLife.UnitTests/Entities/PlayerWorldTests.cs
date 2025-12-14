using PilotLife.Domain.Entities;

namespace PilotLife.UnitTests.Entities;

public class PlayerWorldTests
{
    [Fact]
    public void NewPlayerWorld_HasValidId()
    {
        var playerWorld = new PlayerWorld();

        Assert.NotEqual(Guid.Empty, playerWorld.Id);
    }

    [Fact]
    public void NewPlayerWorld_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var playerWorld = new PlayerWorld();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(playerWorld.CreatedAt, before, after);
    }

    [Fact]
    public void NewPlayerWorld_HasDefaultCreditScore()
    {
        var playerWorld = new PlayerWorld();

        Assert.Equal(650, playerWorld.CreditScore);
    }

    [Fact]
    public void NewPlayerWorld_HasDefaultReputationScore()
    {
        var playerWorld = new PlayerWorld();

        Assert.Equal(3.0m, playerWorld.ReputationScore);
    }

    [Fact]
    public void NewPlayerWorld_IsActiveByDefault()
    {
        var playerWorld = new PlayerWorld();

        Assert.True(playerWorld.IsActive);
    }

    [Fact]
    public void NewPlayerWorld_HasJoinedAtSet()
    {
        var before = DateTimeOffset.UtcNow;
        var playerWorld = new PlayerWorld();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(playerWorld.JoinedAt, before, after);
    }

    [Fact]
    public void NewPlayerWorld_HasLastActiveAtSet()
    {
        var before = DateTimeOffset.UtcNow;
        var playerWorld = new PlayerWorld();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(playerWorld.LastActiveAt, before, after);
    }

    [Fact]
    public void NewPlayerWorld_HasZeroStatistics()
    {
        var playerWorld = new PlayerWorld();

        Assert.Equal(0m, playerWorld.Balance);
        Assert.Equal(0, playerWorld.TotalFlightMinutes);
        Assert.Equal(0, playerWorld.TotalFlights);
        Assert.Equal(0, playerWorld.TotalJobsCompleted);
        Assert.Equal(0m, playerWorld.TotalEarnings);
        Assert.Equal(0m, playerWorld.TotalSpent);
        Assert.Equal(0, playerWorld.OnTimeDeliveries);
        Assert.Equal(0, playerWorld.LateDeliveries);
        Assert.Equal(0, playerWorld.FailedDeliveries);
        Assert.Equal(0, playerWorld.ViolationPoints);
    }

    [Fact]
    public void NewPlayerWorld_HasNoLastViolation()
    {
        var playerWorld = new PlayerWorld();

        Assert.Null(playerWorld.LastViolationAt);
    }

    [Fact]
    public void NewPlayerWorld_HasNoCurrentAirport()
    {
        var playerWorld = new PlayerWorld();

        Assert.Null(playerWorld.CurrentAirportId);
        Assert.Null(playerWorld.CurrentAirport);
    }

    [Fact]
    public void NewPlayerWorld_HasNoHomeAirport()
    {
        var playerWorld = new PlayerWorld();

        Assert.Null(playerWorld.HomeAirportId);
        Assert.Null(playerWorld.HomeAirport);
    }

    [Fact]
    public void Balance_CanBeSet()
    {
        var playerWorld = new PlayerWorld
        {
            Balance = 50000m
        };

        Assert.Equal(50000m, playerWorld.Balance);
    }

    [Fact]
    public void Balance_CanBeNegative()
    {
        var playerWorld = new PlayerWorld
        {
            Balance = -1000m
        };

        Assert.Equal(-1000m, playerWorld.Balance);
    }

    [Fact]
    public void CreditScore_CanBeInValidRange()
    {
        var playerWorld = new PlayerWorld
        {
            CreditScore = 300
        };
        Assert.Equal(300, playerWorld.CreditScore);

        playerWorld.CreditScore = 850;
        Assert.Equal(850, playerWorld.CreditScore);
    }

    [Fact]
    public void ReputationScore_CanBeInValidRange()
    {
        var playerWorld = new PlayerWorld
        {
            ReputationScore = 0.0m
        };
        Assert.Equal(0.0m, playerWorld.ReputationScore);

        playerWorld.ReputationScore = 5.0m;
        Assert.Equal(5.0m, playerWorld.ReputationScore);
    }

    [Fact]
    public void ViolationPoints_CanBeIncremented()
    {
        var playerWorld = new PlayerWorld();

        playerWorld.ViolationPoints += 10;
        playerWorld.LastViolationAt = DateTimeOffset.UtcNow;

        Assert.Equal(10, playerWorld.ViolationPoints);
        Assert.NotNull(playerWorld.LastViolationAt);
    }
}
