using PilotLife.Application.Skills;

namespace PilotLife.UnitTests.Skills;

public class SkillsConfigurationTests
{
    [Fact]
    public void SectionName_IsCorrect()
    {
        Assert.Equal("Skills", SkillsConfiguration.SectionName);
    }

    [Fact]
    public void DefaultValues_FlightXp_AreCorrect()
    {
        var config = new SkillsConfiguration();

        Assert.Equal(10, config.XpPerFlightHour);
        Assert.Equal(1.0, config.XpPerTenNm);
        Assert.Equal(5, config.XpSmoothLanding);
        Assert.Equal(2, config.XpGoodLanding);
    }

    [Fact]
    public void DefaultValues_JobXp_AreCorrect()
    {
        var config = new SkillsConfiguration();

        Assert.Equal(25, config.XpPerCargoJob);
        Assert.Equal(25, config.XpPerPassengerJob);
        Assert.Equal(15, config.XpHighRiskJobBonus);
        Assert.Equal(20, config.XpVipJobBonus);
    }

    [Fact]
    public void DefaultValues_SpecialFlightXp_AreCorrect()
    {
        var config = new SkillsConfiguration();

        Assert.Equal(5, config.XpNightFlightHourBonus);
        Assert.Equal(5, config.XpIfrFlightHourBonus);
        Assert.Equal(10, config.XpMountainLandingBonus);
    }

    [Fact]
    public void DefaultValues_AircraftXp_AreCorrect()
    {
        var config = new SkillsConfiguration();

        Assert.Equal(20, config.XpNewAircraftTypeBonus);
    }

    [Fact]
    public void DefaultValues_LevelThresholds_AreCorrect()
    {
        var config = new SkillsConfiguration();

        Assert.Equal(8, config.LevelThresholds.Length);
        Assert.Equal([0, 100, 300, 600, 1000, 1500, 2500, 4000], config.LevelThresholds);
    }

    [Fact]
    public void MaxLevel_Returns8()
    {
        var config = new SkillsConfiguration();

        Assert.Equal(8, config.MaxLevel);
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
    public void GetXpForLevel_ReturnsCorrectThreshold(int level, int expectedXp)
    {
        var config = new SkillsConfiguration();

        Assert.Equal(expectedXp, config.GetXpForLevel(level));
    }

    [Fact]
    public void GetXpForLevel_BelowMinLevel_ReturnsZero()
    {
        var config = new SkillsConfiguration();

        Assert.Equal(0, config.GetXpForLevel(0));
        Assert.Equal(0, config.GetXpForLevel(-1));
    }

    [Fact]
    public void GetXpForLevel_AboveMaxLevel_ReturnsMaxValue()
    {
        var config = new SkillsConfiguration();

        Assert.Equal(int.MaxValue, config.GetXpForLevel(9));
        Assert.Equal(int.MaxValue, config.GetXpForLevel(100));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(50, 1)]
    [InlineData(99, 1)]
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
        var config = new SkillsConfiguration();

        Assert.Equal(expectedLevel, config.GetLevelForXp(xp));
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void XpPerFlightHour_CanBeSet(int xp)
    {
        var config = new SkillsConfiguration { XpPerFlightHour = xp };
        Assert.Equal(xp, config.XpPerFlightHour);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public void XpPerTenNm_CanBeSet(double xp)
    {
        var config = new SkillsConfiguration { XpPerTenNm = xp };
        Assert.Equal(xp, config.XpPerTenNm);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    public void XpPerCargoJob_CanBeSet(int xp)
    {
        var config = new SkillsConfiguration { XpPerCargoJob = xp };
        Assert.Equal(xp, config.XpPerCargoJob);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    public void XpPerPassengerJob_CanBeSet(int xp)
    {
        var config = new SkillsConfiguration { XpPerPassengerJob = xp };
        Assert.Equal(xp, config.XpPerPassengerJob);
    }

    [Fact]
    public void LevelThresholds_CanBeCustomized()
    {
        var customThresholds = new[] { 0, 50, 150, 300, 500, 750, 1000, 1500 };
        var config = new SkillsConfiguration { LevelThresholds = customThresholds };

        Assert.Equal(customThresholds, config.LevelThresholds);
        Assert.Equal(8, config.MaxLevel);
    }

    [Fact]
    public void CustomLevelThresholds_AffectsGetXpForLevel()
    {
        var customThresholds = new[] { 0, 50, 150, 300, 500, 750, 1000, 1500 };
        var config = new SkillsConfiguration { LevelThresholds = customThresholds };

        Assert.Equal(0, config.GetXpForLevel(1));
        Assert.Equal(50, config.GetXpForLevel(2));
        Assert.Equal(150, config.GetXpForLevel(3));
    }

    [Fact]
    public void CustomLevelThresholds_AffectsGetLevelForXp()
    {
        var customThresholds = new[] { 0, 50, 150, 300, 500, 750, 1000, 1500 };
        var config = new SkillsConfiguration { LevelThresholds = customThresholds };

        Assert.Equal(1, config.GetLevelForXp(25));
        Assert.Equal(2, config.GetLevelForXp(50));
        Assert.Equal(2, config.GetLevelForXp(100));
        Assert.Equal(3, config.GetLevelForXp(150));
    }

    [Fact]
    public void AllXpSettings_CanBeCustomized()
    {
        var config = new SkillsConfiguration
        {
            XpPerFlightHour = 15,
            XpPerTenNm = 2.0,
            XpSmoothLanding = 10,
            XpGoodLanding = 5,
            XpPerCargoJob = 30,
            XpPerPassengerJob = 35,
            XpHighRiskJobBonus = 20,
            XpVipJobBonus = 25,
            XpNightFlightHourBonus = 8,
            XpIfrFlightHourBonus = 10,
            XpMountainLandingBonus = 15,
            XpNewAircraftTypeBonus = 30
        };

        Assert.Equal(15, config.XpPerFlightHour);
        Assert.Equal(2.0, config.XpPerTenNm);
        Assert.Equal(10, config.XpSmoothLanding);
        Assert.Equal(5, config.XpGoodLanding);
        Assert.Equal(30, config.XpPerCargoJob);
        Assert.Equal(35, config.XpPerPassengerJob);
        Assert.Equal(20, config.XpHighRiskJobBonus);
        Assert.Equal(25, config.XpVipJobBonus);
        Assert.Equal(8, config.XpNightFlightHourBonus);
        Assert.Equal(10, config.XpIfrFlightHourBonus);
        Assert.Equal(15, config.XpMountainLandingBonus);
        Assert.Equal(30, config.XpNewAircraftTypeBonus);
    }
}
