using PilotLife.Application.Jobs;

namespace PilotLife.UnitTests.Jobs;

public class JobGenerationConfigurationTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var config = new JobGenerationConfiguration();

        Assert.Equal(1.0, config.IntervalHours);
        Assert.Equal(25, config.JobsPerLargeAirport);
        Assert.Equal(15, config.JobsPerMediumAirport);
        Assert.Equal(8, config.JobsPerSmallAirport);
        Assert.Equal(3, config.MinJobsPerAirport);
        Assert.Equal(48, config.BaseExpiryHours);
        Assert.Equal(0.70, config.CargoJobPercentage);
        Assert.Equal(500, config.AirportBatchSize);
        Assert.Equal(1000, config.ProgressLogInterval);
        Assert.Equal(3000, config.MaxRouteDistanceNm);
        Assert.Equal(20, config.MinRouteDistanceNm);
        Assert.True(config.OnlyAirportsWithRunways);
    }

    [Fact]
    public void SectionName_IsCorrect()
    {
        Assert.Equal("JobGeneration", JobGenerationConfiguration.SectionName);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(6.0)]
    public void IntervalHours_CanBeSet(double hours)
    {
        var config = new JobGenerationConfiguration { IntervalHours = hours };
        Assert.Equal(hours, config.IntervalHours);
    }

    [Theory]
    [InlineData(10, 8, 5)]
    [InlineData(25, 15, 8)]
    [InlineData(50, 30, 15)]
    public void JobsPerAirport_CanBeSet(int large, int medium, int small)
    {
        var config = new JobGenerationConfiguration
        {
            JobsPerLargeAirport = large,
            JobsPerMediumAirport = medium,
            JobsPerSmallAirport = small
        };

        Assert.Equal(large, config.JobsPerLargeAirport);
        Assert.Equal(medium, config.JobsPerMediumAirport);
        Assert.Equal(small, config.JobsPerSmallAirport);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void MinJobsPerAirport_CanBeSet(int min)
    {
        var config = new JobGenerationConfiguration { MinJobsPerAirport = min };
        Assert.Equal(min, config.MinJobsPerAirport);
    }

    [Theory]
    [InlineData(24)]
    [InlineData(48)]
    [InlineData(72)]
    [InlineData(168)]
    public void BaseExpiryHours_CanBeSet(int hours)
    {
        var config = new JobGenerationConfiguration { BaseExpiryHours = hours };
        Assert.Equal(hours, config.BaseExpiryHours);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.50)]
    [InlineData(0.70)]
    [InlineData(1.0)]
    public void CargoJobPercentage_CanBeSet(double percentage)
    {
        var config = new JobGenerationConfiguration { CargoJobPercentage = percentage };
        Assert.Equal(percentage, config.CargoJobPercentage);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(5000)]
    public void AirportBatchSize_CanBeSet(int size)
    {
        var config = new JobGenerationConfiguration { AirportBatchSize = size };
        Assert.Equal(size, config.AirportBatchSize);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(5000)]
    public void ProgressLogInterval_CanBeSet(int interval)
    {
        var config = new JobGenerationConfiguration { ProgressLogInterval = interval };
        Assert.Equal(interval, config.ProgressLogInterval);
    }

    [Theory]
    [InlineData(500, 50)]
    [InlineData(1500, 100)]
    [InlineData(3000, 20)]
    [InlineData(5000, 50)]
    public void RouteDistanceLimits_CanBeSet(int max, int min)
    {
        var config = new JobGenerationConfiguration
        {
            MaxRouteDistanceNm = max,
            MinRouteDistanceNm = min
        };

        Assert.Equal(max, config.MaxRouteDistanceNm);
        Assert.Equal(min, config.MinRouteDistanceNm);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OnlyAirportsWithRunways_CanBeSet(bool value)
    {
        var config = new JobGenerationConfiguration { OnlyAirportsWithRunways = value };
        Assert.Equal(value, config.OnlyAirportsWithRunways);
    }

    [Fact]
    public void MaxRouteDistance_ShouldBeGreaterThanMinRouteDistance()
    {
        var config = new JobGenerationConfiguration();
        Assert.True(config.MaxRouteDistanceNm > config.MinRouteDistanceNm);
    }

    [Fact]
    public void LargeAirportJobs_ShouldBeGreaterThanMediumAirportJobs()
    {
        var config = new JobGenerationConfiguration();
        Assert.True(config.JobsPerLargeAirport > config.JobsPerMediumAirport);
    }

    [Fact]
    public void MediumAirportJobs_ShouldBeGreaterThanSmallAirportJobs()
    {
        var config = new JobGenerationConfiguration();
        Assert.True(config.JobsPerMediumAirport > config.JobsPerSmallAirport);
    }

    [Fact]
    public void MinJobsPerAirport_ShouldBeLessThanSmallAirportJobs()
    {
        var config = new JobGenerationConfiguration();
        Assert.True(config.MinJobsPerAirport < config.JobsPerSmallAirport);
    }

    [Fact]
    public void CargoJobPercentage_DefaultIsMajority()
    {
        var config = new JobGenerationConfiguration();
        Assert.True(config.CargoJobPercentage > 0.5);
    }
}
