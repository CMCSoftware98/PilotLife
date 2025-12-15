using PilotLife.Domain.Entities;

namespace PilotLife.UnitTests.Entities;

public class SkillXpEventTests
{
    [Fact]
    public void NewSkillXpEvent_HasValidId()
    {
        var xpEvent = new SkillXpEvent();

        Assert.NotEqual(Guid.Empty, xpEvent.Id);
    }

    [Fact]
    public void NewSkillXpEvent_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var xpEvent = new SkillXpEvent();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(xpEvent.CreatedAt, before, after);
    }

    [Fact]
    public void NewSkillXpEvent_HasOccurredAtSet()
    {
        var before = DateTimeOffset.UtcNow;
        var xpEvent = new SkillXpEvent();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(xpEvent.OccurredAt, before, after);
    }

    [Fact]
    public void NewSkillXpEvent_HasDefaultSource()
    {
        var xpEvent = new SkillXpEvent();

        Assert.Equal(string.Empty, xpEvent.Source);
    }

    [Fact]
    public void NewSkillXpEvent_HasNoRelatedJobOrFlight()
    {
        var xpEvent = new SkillXpEvent();

        Assert.Null(xpEvent.RelatedJobId);
        Assert.Null(xpEvent.RelatedJob);
        Assert.Null(xpEvent.RelatedFlightId);
        Assert.Null(xpEvent.RelatedFlight);
    }

    [Fact]
    public void NewSkillXpEvent_HasNullDescription()
    {
        var xpEvent = new SkillXpEvent();

        Assert.Null(xpEvent.Description);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(500)]
    public void XpGained_CanBeSet(int xp)
    {
        var xpEvent = new SkillXpEvent { XpGained = xp };

        Assert.Equal(xp, xpEvent.XpGained);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(4000)]
    public void ResultingXp_CanBeSet(int xp)
    {
        var xpEvent = new SkillXpEvent { ResultingXp = xp };

        Assert.Equal(xp, xpEvent.ResultingXp);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(8)]
    public void ResultingLevel_CanBeSet(int level)
    {
        var xpEvent = new SkillXpEvent { ResultingLevel = level };

        Assert.Equal(level, xpEvent.ResultingLevel);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CausedLevelUp_CanBeSet(bool value)
    {
        var xpEvent = new SkillXpEvent { CausedLevelUp = value };

        Assert.Equal(value, xpEvent.CausedLevelUp);
    }

    [Fact]
    public void CreateFromFlight_SetsAllProperties()
    {
        var playerSkillId = Guid.NewGuid();
        var flightId = Guid.NewGuid();
        var xpGained = 10;
        var resultingXp = 100;
        var resultingLevel = 2;
        var causedLevelUp = true;
        var source = "Flight time";
        var description = "1 hour of flight";

        var xpEvent = SkillXpEvent.CreateFromFlight(
            playerSkillId, xpGained, resultingXp, resultingLevel, causedLevelUp,
            flightId, source, description);

        Assert.Equal(playerSkillId, xpEvent.PlayerSkillId);
        Assert.Equal(xpGained, xpEvent.XpGained);
        Assert.Equal(resultingXp, xpEvent.ResultingXp);
        Assert.Equal(resultingLevel, xpEvent.ResultingLevel);
        Assert.Equal(causedLevelUp, xpEvent.CausedLevelUp);
        Assert.Equal(flightId, xpEvent.RelatedFlightId);
        Assert.Equal(source, xpEvent.Source);
        Assert.Equal(description, xpEvent.Description);
        Assert.Null(xpEvent.RelatedJobId);
    }

    [Fact]
    public void CreateFromFlight_WithoutDescription_SetsNullDescription()
    {
        var xpEvent = SkillXpEvent.CreateFromFlight(
            Guid.NewGuid(), 10, 100, 2, true, Guid.NewGuid(), "Flight time");

        Assert.Null(xpEvent.Description);
    }

    [Fact]
    public void CreateFromJob_SetsAllProperties()
    {
        var playerSkillId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var xpGained = 25;
        var resultingXp = 150;
        var resultingLevel = 2;
        var causedLevelUp = false;
        var source = "Cargo job completed";
        var description = "Delivered medical supplies";

        var xpEvent = SkillXpEvent.CreateFromJob(
            playerSkillId, xpGained, resultingXp, resultingLevel, causedLevelUp,
            jobId, source, description);

        Assert.Equal(playerSkillId, xpEvent.PlayerSkillId);
        Assert.Equal(xpGained, xpEvent.XpGained);
        Assert.Equal(resultingXp, xpEvent.ResultingXp);
        Assert.Equal(resultingLevel, xpEvent.ResultingLevel);
        Assert.Equal(causedLevelUp, xpEvent.CausedLevelUp);
        Assert.Equal(jobId, xpEvent.RelatedJobId);
        Assert.Equal(source, xpEvent.Source);
        Assert.Equal(description, xpEvent.Description);
        Assert.Null(xpEvent.RelatedFlightId);
    }

    [Fact]
    public void CreateFromJob_WithoutDescription_SetsNullDescription()
    {
        var xpEvent = SkillXpEvent.CreateFromJob(
            Guid.NewGuid(), 25, 150, 2, false, Guid.NewGuid(), "Cargo job completed");

        Assert.Null(xpEvent.Description);
    }

    [Fact]
    public void CreateFromFlight_HasValidId()
    {
        var xpEvent = SkillXpEvent.CreateFromFlight(
            Guid.NewGuid(), 10, 100, 2, true, Guid.NewGuid(), "Test");

        Assert.NotEqual(Guid.Empty, xpEvent.Id);
    }

    [Fact]
    public void CreateFromJob_HasValidId()
    {
        var xpEvent = SkillXpEvent.CreateFromJob(
            Guid.NewGuid(), 25, 150, 2, false, Guid.NewGuid(), "Test");

        Assert.NotEqual(Guid.Empty, xpEvent.Id);
    }
}
