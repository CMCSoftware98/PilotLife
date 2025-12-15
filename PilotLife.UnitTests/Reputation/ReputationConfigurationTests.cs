using PilotLife.Application.Reputation;

namespace PilotLife.UnitTests.Reputation;

public class ReputationConfigurationTests
{
    [Fact]
    public void SectionName_IsCorrect()
    {
        Assert.Equal("Reputation", ReputationConfiguration.SectionName);
    }

    [Fact]
    public void DefaultValues_BaseValues_AreCorrect()
    {
        var config = new ReputationConfiguration();

        Assert.Equal(3.0m, config.BaseScore);
        Assert.Equal(0.0m, config.MinScore);
        Assert.Equal(5.0m, config.MaxScore);
    }

    [Fact]
    public void DefaultValues_JobBonusesAndPenalties_AreCorrect()
    {
        var config = new ReputationConfiguration();

        Assert.Equal(0.1m, config.JobOnTimeBonus);
        Assert.Equal(0.15m, config.JobEarlyBonus);
        Assert.Equal(-0.1m, config.JobLatePenalty);
        Assert.Equal(-0.3m, config.JobFailedPenalty);
        Assert.Equal(-0.15m, config.JobCancelledPenalty);
    }

    [Fact]
    public void DefaultValues_LandingBonusesAndPenalties_AreCorrect()
    {
        var config = new ReputationConfiguration();

        Assert.Equal(0.02m, config.SmoothLandingBonus);
        Assert.Equal(0.01m, config.GoodLandingBonus);
        Assert.Equal(-0.05m, config.HardLandingPenalty);
    }

    [Fact]
    public void DefaultValues_SafetyPenalties_AreCorrect()
    {
        var config = new ReputationConfiguration();

        Assert.Equal(-0.03m, config.OverspeedPenalty);
        Assert.Equal(-0.02m, config.StallWarningPenalty);
        Assert.Equal(-0.5m, config.AccidentPenalty);
    }

    [Fact]
    public void DefaultValues_SpecialBonuses_AreCorrect()
    {
        var config = new ReputationConfiguration();

        Assert.Equal(0.05m, config.HighRiskJobBonus);
        Assert.Equal(0.1m, config.VipJobBonus);
    }

    [Fact]
    public void DefaultValues_Decay_AreCorrect()
    {
        var config = new ReputationConfiguration();

        Assert.Equal(0.01m, config.DecayRatePerDay);
        Assert.Equal(7, config.DecayGracePeriodDays);
    }

    [Fact]
    public void DefaultValues_LevelThresholds_AreCorrect()
    {
        var config = new ReputationConfiguration();

        Assert.Equal(1.0m, config.Level2Threshold);
        Assert.Equal(2.0m, config.Level3Threshold);
        Assert.Equal(3.0m, config.Level4Threshold);
        Assert.Equal(4.0m, config.Level5Threshold);
    }

    [Fact]
    public void DefaultValues_PayoutBonuses_AreCorrect()
    {
        var config = new ReputationConfiguration();

        Assert.Equal(10m, config.TrustedPayoutBonus);
        Assert.Equal(20m, config.ElitePayoutBonus);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(2.5)]
    [InlineData(5.0)]
    public void BaseScore_CanBeSet(decimal score)
    {
        var config = new ReputationConfiguration { BaseScore = score };
        Assert.Equal(score, config.BaseScore);
    }

    [Theory]
    [InlineData(0.05)]
    [InlineData(0.1)]
    [InlineData(0.2)]
    public void JobOnTimeBonus_CanBeSet(decimal bonus)
    {
        var config = new ReputationConfiguration { JobOnTimeBonus = bonus };
        Assert.Equal(bonus, config.JobOnTimeBonus);
    }

    [Theory]
    [InlineData(-0.05)]
    [InlineData(-0.1)]
    [InlineData(-0.5)]
    public void JobFailedPenalty_CanBeSet(decimal penalty)
    {
        var config = new ReputationConfiguration { JobFailedPenalty = penalty };
        Assert.Equal(penalty, config.JobFailedPenalty);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(0.02)]
    [InlineData(0.05)]
    public void SmoothLandingBonus_CanBeSet(decimal bonus)
    {
        var config = new ReputationConfiguration { SmoothLandingBonus = bonus };
        Assert.Equal(bonus, config.SmoothLandingBonus);
    }

    [Theory]
    [InlineData(-0.3)]
    [InlineData(-0.5)]
    [InlineData(-1.0)]
    public void AccidentPenalty_CanBeSet(decimal penalty)
    {
        var config = new ReputationConfiguration { AccidentPenalty = penalty };
        Assert.Equal(penalty, config.AccidentPenalty);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(14)]
    public void DecayGracePeriodDays_CanBeSet(int days)
    {
        var config = new ReputationConfiguration { DecayGracePeriodDays = days };
        Assert.Equal(days, config.DecayGracePeriodDays);
    }

    [Theory]
    [InlineData(0.5, 1.0, 2.0, 3.0)]
    [InlineData(1.0, 2.0, 3.0, 4.0)]
    [InlineData(1.5, 2.5, 3.5, 4.5)]
    public void LevelThresholds_CanBeCustomized(decimal l2, decimal l3, decimal l4, decimal l5)
    {
        var config = new ReputationConfiguration
        {
            Level2Threshold = l2,
            Level3Threshold = l3,
            Level4Threshold = l4,
            Level5Threshold = l5
        };

        Assert.Equal(l2, config.Level2Threshold);
        Assert.Equal(l3, config.Level3Threshold);
        Assert.Equal(l4, config.Level4Threshold);
        Assert.Equal(l5, config.Level5Threshold);
    }

    [Theory]
    [InlineData(5, 10)]
    [InlineData(10, 20)]
    [InlineData(15, 30)]
    public void PayoutBonuses_CanBeCustomized(decimal trusted, decimal elite)
    {
        var config = new ReputationConfiguration
        {
            TrustedPayoutBonus = trusted,
            ElitePayoutBonus = elite
        };

        Assert.Equal(trusted, config.TrustedPayoutBonus);
        Assert.Equal(elite, config.ElitePayoutBonus);
    }
}
