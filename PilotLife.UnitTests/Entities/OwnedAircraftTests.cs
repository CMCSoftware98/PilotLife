using PilotLife.Domain.Entities;

namespace PilotLife.UnitTests.Entities;

public class OwnedAircraftTests
{
    [Fact]
    public void NewOwnedAircraft_HasValidId()
    {
        var aircraft = new OwnedAircraft();

        Assert.NotEqual(Guid.Empty, aircraft.Id);
    }

    [Fact]
    public void NewOwnedAircraft_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var aircraft = new OwnedAircraft();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(aircraft.CreatedAt, before, after);
    }

    [Fact]
    public void NewOwnedAircraft_HasDefaultValues()
    {
        var aircraft = new OwnedAircraft();

        Assert.Equal(100, aircraft.Condition);
        Assert.Equal(0, aircraft.TotalFlightMinutes);
        Assert.Equal(0, aircraft.TotalCycles);
        Assert.Equal(0, aircraft.HoursSinceLastInspection);
        Assert.Equal(string.Empty, aircraft.CurrentLocationIcao);
        Assert.False(aircraft.IsInMaintenance);
        Assert.False(aircraft.IsInUse);
        Assert.False(aircraft.IsListedForRental);
        Assert.False(aircraft.IsListedForSale);
        Assert.False(aircraft.WasPurchasedNew);
        Assert.False(aircraft.HasWarranty);
        Assert.False(aircraft.HasInsurance);
    }

    [Fact]
    public void IsAirworthy_WhenConditionAbove60AndNotInMaintenance_ReturnsTrue()
    {
        var aircraft = new OwnedAircraft
        {
            Condition = 70,
            IsInMaintenance = false
        };

        Assert.True(aircraft.IsAirworthy);
    }

    [Fact]
    public void IsAirworthy_WhenConditionIs60_ReturnsTrue()
    {
        var aircraft = new OwnedAircraft
        {
            Condition = 60,
            IsInMaintenance = false
        };

        Assert.True(aircraft.IsAirworthy);
    }

    [Fact]
    public void IsAirworthy_WhenConditionBelow60_ReturnsFalse()
    {
        var aircraft = new OwnedAircraft
        {
            Condition = 59,
            IsInMaintenance = false
        };

        Assert.False(aircraft.IsAirworthy);
    }

    [Fact]
    public void IsAirworthy_WhenInMaintenance_ReturnsFalse()
    {
        var aircraft = new OwnedAircraft
        {
            Condition = 100,
            IsInMaintenance = true
        };

        Assert.False(aircraft.IsAirworthy);
    }

    [Fact]
    public void TotalFlightHours_ConvertsMinutesToHours()
    {
        var aircraft = new OwnedAircraft { TotalFlightMinutes = 150 };

        Assert.Equal(2.5, aircraft.TotalFlightHours);
    }

    [Fact]
    public void TotalFlightHours_WhenZeroMinutes_ReturnsZero()
    {
        var aircraft = new OwnedAircraft { TotalFlightMinutes = 0 };

        Assert.Equal(0, aircraft.TotalFlightHours);
    }

    [Fact]
    public void InspectionDue_WhenAt100Hours_ReturnsTrue()
    {
        var aircraft = new OwnedAircraft { HoursSinceLastInspection = 6000 }; // 100 hours in minutes

        Assert.True(aircraft.InspectionDue);
    }

    [Fact]
    public void InspectionDue_WhenBelow100Hours_ReturnsFalse()
    {
        var aircraft = new OwnedAircraft { HoursSinceLastInspection = 5999 };

        Assert.False(aircraft.InspectionDue);
    }

    [Fact]
    public void EstimatedValue_WhenNewCondition_ReturnsBasePrice()
    {
        var aircraft = new OwnedAircraft
        {
            Condition = 100,
            TotalFlightMinutes = 0
        };

        var value = aircraft.EstimatedValue(100000m);

        Assert.Equal(100000m, value);
    }

    [Fact]
    public void EstimatedValue_WhenDegraded_ReturnsReducedValue()
    {
        var aircraft = new OwnedAircraft
        {
            Condition = 80,
            TotalFlightMinutes = 0
        };

        var value = aircraft.EstimatedValue(100000m);

        Assert.Equal(80000m, value); // 80% condition = 80% of base
    }

    [Fact]
    public void EstimatedValue_WhenHighHours_ReturnsReducedValue()
    {
        var aircraft = new OwnedAircraft
        {
            Condition = 100,
            TotalFlightMinutes = 60000 // 1000 hours
        };

        var value = aircraft.EstimatedValue(100000m);

        Assert.Equal(95000m, value); // 5% reduction for 1000 hours
    }

    [Fact]
    public void EstimatedValue_WhenVeryHighHours_CapsDepreciationAt50Percent()
    {
        var aircraft = new OwnedAircraft
        {
            Condition = 100,
            TotalFlightMinutes = 1200000 // 20000 hours
        };

        var value = aircraft.EstimatedValue(100000m);

        Assert.Equal(50000m, value); // Max 50% depreciation for hours
    }

    [Fact]
    public void Collections_DefaultToEmpty()
    {
        var aircraft = new OwnedAircraft();

        Assert.NotNull(aircraft.Components);
        Assert.Empty(aircraft.Components);
        Assert.NotNull(aircraft.Modifications);
        Assert.Empty(aircraft.Modifications);
        Assert.NotNull(aircraft.MaintenanceLogs);
        Assert.Empty(aircraft.MaintenanceLogs);
    }
}
