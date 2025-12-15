using PilotLife.Application.Marketplace;

namespace PilotLife.UnitTests.Marketplace;

public class MarketplaceConfigurationTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var config = new MarketplaceConfiguration();

        Assert.Equal(3.5, config.IntervalHours);
        Assert.Equal(3, config.DealersPerLargeAirport);
        Assert.Equal(2, config.DealersPerMediumAirport);
        Assert.Equal(1, config.DealersPerSmallAirport);
        Assert.Equal(3, config.MinInventoryPerDealer);
        Assert.Equal(15, config.MaxInventoryPerDealer);
        Assert.Equal(14, config.InventoryExpirationDays);
        Assert.Equal(500, config.AirportBatchSize);
        Assert.Equal(1000, config.ProgressLogInterval);
    }

    [Fact]
    public void SectionName_IsCorrect()
    {
        Assert.Equal("MarketplacePopulation", MarketplaceConfiguration.SectionName);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(3.5)]
    [InlineData(6.0)]
    [InlineData(12.0)]
    public void IntervalHours_CanBeSet(double hours)
    {
        var config = new MarketplaceConfiguration { IntervalHours = hours };
        Assert.Equal(hours, config.IntervalHours);
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(5, 3, 2)]
    [InlineData(10, 5, 3)]
    public void DealersPerAirport_CanBeSet(int large, int medium, int small)
    {
        var config = new MarketplaceConfiguration
        {
            DealersPerLargeAirport = large,
            DealersPerMediumAirport = medium,
            DealersPerSmallAirport = small
        };

        Assert.Equal(large, config.DealersPerLargeAirport);
        Assert.Equal(medium, config.DealersPerMediumAirport);
        Assert.Equal(small, config.DealersPerSmallAirport);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(5, 20)]
    [InlineData(10, 50)]
    public void InventoryLimits_CanBeSet(int min, int max)
    {
        var config = new MarketplaceConfiguration
        {
            MinInventoryPerDealer = min,
            MaxInventoryPerDealer = max
        };

        Assert.Equal(min, config.MinInventoryPerDealer);
        Assert.Equal(max, config.MaxInventoryPerDealer);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(14)]
    [InlineData(30)]
    public void InventoryExpirationDays_CanBeSet(int days)
    {
        var config = new MarketplaceConfiguration { InventoryExpirationDays = days };
        Assert.Equal(days, config.InventoryExpirationDays);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    public void AirportBatchSize_CanBeSet(int size)
    {
        var config = new MarketplaceConfiguration { AirportBatchSize = size };
        Assert.Equal(size, config.AirportBatchSize);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(5000)]
    public void ProgressLogInterval_CanBeSet(int interval)
    {
        var config = new MarketplaceConfiguration { ProgressLogInterval = interval };
        Assert.Equal(interval, config.ProgressLogInterval);
    }
}
