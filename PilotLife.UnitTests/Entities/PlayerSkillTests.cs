using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class PlayerSkillTests
{
    [Fact]
    public void NewPlayerSkill_HasValidId()
    {
        var skill = new PlayerSkill();

        Assert.NotEqual(Guid.Empty, skill.Id);
    }

    [Fact]
    public void NewPlayerSkill_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var skill = new PlayerSkill();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(skill.CreatedAt, before, after);
    }

    [Fact]
    public void NewPlayerSkill_StartsAtLevel1()
    {
        var skill = new PlayerSkill();

        Assert.Equal(1, skill.Level);
    }

    [Fact]
    public void NewPlayerSkill_HasZeroXp()
    {
        var skill = new PlayerSkill();

        Assert.Equal(0, skill.CurrentXp);
    }

    [Fact]
    public void NewPlayerSkill_HasLastUpdatedAtSet()
    {
        var before = DateTimeOffset.UtcNow;
        var skill = new PlayerSkill();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(skill.LastUpdatedAt, before, after);
    }

    [Theory]
    [InlineData(SkillType.Piloting)]
    [InlineData(SkillType.Navigation)]
    [InlineData(SkillType.CargoHandling)]
    [InlineData(SkillType.PassengerService)]
    [InlineData(SkillType.AircraftKnowledge)]
    [InlineData(SkillType.WeatherFlying)]
    [InlineData(SkillType.NightFlying)]
    [InlineData(SkillType.MountainFlying)]
    public void SkillType_CanBeSet(SkillType skillType)
    {
        var skill = new PlayerSkill { SkillType = skillType };

        Assert.Equal(skillType, skill.SkillType);
    }

    [Fact]
    public void Create_SetsAllProperties()
    {
        var playerWorldId = Guid.NewGuid();
        var skillType = SkillType.Piloting;

        var skill = PlayerSkill.Create(playerWorldId, skillType);

        Assert.Equal(playerWorldId, skill.PlayerWorldId);
        Assert.Equal(skillType, skill.SkillType);
        Assert.Equal(0, skill.CurrentXp);
        Assert.Equal(1, skill.Level);
        Assert.NotEqual(Guid.Empty, skill.Id);
    }

    [Theory]
    [InlineData(1, "Beginner")]
    [InlineData(2, "Novice")]
    [InlineData(3, "Apprentice")]
    [InlineData(4, "Journeyman")]
    [InlineData(5, "Expert")]
    [InlineData(6, "Master")]
    [InlineData(7, "Grandmaster")]
    [InlineData(8, "Legend")]
    public void LevelName_ReturnsCorrectName(int level, string expectedName)
    {
        var skill = new PlayerSkill { Level = level };

        Assert.Equal(expectedName, skill.LevelName);
    }

    [Theory]
    [InlineData(1, 100)]
    [InlineData(2, 300)]
    [InlineData(3, 600)]
    [InlineData(4, 1000)]
    [InlineData(5, 1500)]
    [InlineData(6, 2500)]
    [InlineData(7, 4000)]
    public void XpForNextLevel_ReturnsCorrectThreshold(int level, int expectedXp)
    {
        var skill = new PlayerSkill { Level = level };

        Assert.Equal(expectedXp, skill.XpForNextLevel);
    }

    [Fact]
    public void XpForNextLevel_AtMaxLevel_ReturnsMaxValue()
    {
        var skill = new PlayerSkill { Level = 8 };

        Assert.Equal(int.MaxValue, skill.XpForNextLevel);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 100)]
    [InlineData(3, 300)]
    [InlineData(4, 600)]
    [InlineData(5, 1000)]
    [InlineData(6, 1500)]
    [InlineData(7, 2500)]
    [InlineData(8, 4000)]
    public void XpForCurrentLevel_ReturnsCorrectThreshold(int level, int expectedXp)
    {
        var skill = new PlayerSkill { Level = level };

        Assert.Equal(expectedXp, skill.XpForCurrentLevel);
    }

    [Fact]
    public void AddXp_IncrementsCurrentXp()
    {
        var skill = PlayerSkill.Create(Guid.NewGuid(), SkillType.Piloting);

        skill.AddXp(50);

        Assert.Equal(50, skill.CurrentXp);
    }

    [Fact]
    public void AddXp_WithZeroOrNegative_DoesNothing()
    {
        var skill = PlayerSkill.Create(Guid.NewGuid(), SkillType.Piloting);

        var levelsGained1 = skill.AddXp(0);
        var levelsGained2 = skill.AddXp(-10);

        Assert.Equal(0, skill.CurrentXp);
        Assert.Equal(0, levelsGained1);
        Assert.Equal(0, levelsGained2);
    }

    [Fact]
    public void AddXp_CausesLevelUp_WhenThresholdReached()
    {
        var skill = PlayerSkill.Create(Guid.NewGuid(), SkillType.Piloting);

        var levelsGained = skill.AddXp(100);

        Assert.Equal(2, skill.Level);
        Assert.Equal(1, levelsGained);
        Assert.Equal(100, skill.CurrentXp);
    }

    [Fact]
    public void AddXp_CausesMultipleLevelUps()
    {
        var skill = PlayerSkill.Create(Guid.NewGuid(), SkillType.Piloting);

        var levelsGained = skill.AddXp(600); // Should jump from 1 to 4 (0->100->300->600)

        Assert.Equal(4, skill.Level);
        Assert.Equal(3, levelsGained);
        Assert.Equal(600, skill.CurrentXp);
    }

    [Fact]
    public void AddXp_StopsAtMaxLevel()
    {
        var skill = new PlayerSkill
        {
            Level = 7,
            CurrentXp = 2500
        };

        var levelsGained = skill.AddXp(10000);

        Assert.Equal(8, skill.Level);
        Assert.Equal(1, levelsGained);
        Assert.Equal(12500, skill.CurrentXp);
    }

    [Fact]
    public void AddXp_AtMaxLevel_NoLevelUp()
    {
        var skill = new PlayerSkill
        {
            Level = 8,
            CurrentXp = 4000
        };

        var levelsGained = skill.AddXp(1000);

        Assert.Equal(8, skill.Level);
        Assert.Equal(0, levelsGained);
        Assert.Equal(5000, skill.CurrentXp);
    }

    [Fact]
    public void AddXp_UpdatesLastUpdatedAt()
    {
        var skill = PlayerSkill.Create(Guid.NewGuid(), SkillType.Piloting);
        var before = skill.LastUpdatedAt;

        Thread.Sleep(10);
        skill.AddXp(50);

        Assert.True(skill.LastUpdatedAt > before);
    }

    [Fact]
    public void ProgressToNextLevel_AtZeroXp_ReturnsZero()
    {
        var skill = PlayerSkill.Create(Guid.NewGuid(), SkillType.Piloting);

        Assert.Equal(0, skill.ProgressToNextLevel);
    }

    [Fact]
    public void ProgressToNextLevel_AtHalfway_Returns50()
    {
        var skill = new PlayerSkill
        {
            Level = 1,
            CurrentXp = 50 // Halfway to level 2 (0->100)
        };

        Assert.Equal(50, skill.ProgressToNextLevel);
    }

    [Fact]
    public void ProgressToNextLevel_AtMaxLevel_Returns100()
    {
        var skill = new PlayerSkill
        {
            Level = 8,
            CurrentXp = 5000
        };

        Assert.Equal(100, skill.ProgressToNextLevel);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(50, 1)]
    [InlineData(100, 2)]
    [InlineData(299, 2)]
    [InlineData(300, 3)]
    [InlineData(600, 4)]
    [InlineData(1000, 5)]
    [InlineData(1500, 6)]
    [InlineData(2500, 7)]
    [InlineData(4000, 8)]
    [InlineData(10000, 8)]
    public void GetLevelForXp_ReturnsCorrectLevel(int xp, int expectedLevel)
    {
        var level = PlayerSkill.GetLevelForXp(xp);

        Assert.Equal(expectedLevel, level);
    }

    [Fact]
    public void GetLevelForXp_WithNegativeXp_ReturnsLevel1()
    {
        var level = PlayerSkill.GetLevelForXp(-100);

        Assert.Equal(1, level);
    }
}
