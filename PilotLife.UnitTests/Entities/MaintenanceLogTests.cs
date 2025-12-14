using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class MaintenanceLogTests
{
    [Fact]
    public void NewMaintenanceLog_HasValidId()
    {
        var log = new MaintenanceLog();

        Assert.NotEqual(Guid.Empty, log.Id);
    }

    [Fact]
    public void NewMaintenanceLog_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var log = new MaintenanceLog();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(log.CreatedAt, before, after);
    }

    [Fact]
    public void NewMaintenanceLog_HasDefaultValues()
    {
        var log = new MaintenanceLog();

        Assert.Equal(string.Empty, log.Title);
        Assert.Equal(string.Empty, log.PerformedAtAirport);
        Assert.False(log.CoveredByWarranty);
        Assert.False(log.CoveredByInsurance);
        Assert.False(log.IsCompleted);
    }

    [Fact]
    public void TotalCost_SumsLaborAndParts()
    {
        var log = new MaintenanceLog
        {
            LaborCost = 500m,
            PartsCost = 300m
        };

        Assert.Equal(800m, log.TotalCost);
    }

    [Fact]
    public void TotalCost_WhenBothZero_ReturnsZero()
    {
        var log = new MaintenanceLog
        {
            LaborCost = 0m,
            PartsCost = 0m
        };

        Assert.Equal(0m, log.TotalCost);
    }

    [Fact]
    public void CreateInspection_AnnualInspection_HasCorrectTitle()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var log = MaintenanceLog.CreateInspection(
            worldId, aircraftId,
            MaintenanceType.AnnualInspection,
            "EGLL", 6000, 100, 1500m);

        Assert.Equal("Annual Inspection", log.Title);
    }

    [Fact]
    public void CreateInspection_HundredHourInspection_HasCorrectTitle()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var log = MaintenanceLog.CreateInspection(
            worldId, aircraftId,
            MaintenanceType.HundredHourInspection,
            "EGLL", 6000, 100, 800m);

        Assert.Equal("100-Hour Inspection", log.Title);
    }

    [Fact]
    public void CreateInspection_PreFlight_HasCorrectTitle()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var log = MaintenanceLog.CreateInspection(
            worldId, aircraftId,
            MaintenanceType.PreFlight,
            "EGLL", 6000, 100, 0m);

        Assert.Equal("Pre-Flight Inspection", log.Title);
    }

    [Fact]
    public void CreateInspection_SetsCorrectIds()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var log = MaintenanceLog.CreateInspection(
            worldId, aircraftId,
            MaintenanceType.AnnualInspection,
            "EGLL", 6000, 100, 1500m);

        Assert.Equal(worldId, log.WorldId);
        Assert.Equal(aircraftId, log.OwnedAircraftId);
    }

    [Fact]
    public void CreateInspection_SetsAirportAndServiceData()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var log = MaintenanceLog.CreateInspection(
            worldId, aircraftId,
            MaintenanceType.AnnualInspection,
            "EGLL", 6000, 100, 1500m);

        Assert.Equal("EGLL", log.PerformedAtAirport);
        Assert.Equal(6000, log.AircraftFlightMinutesAtService);
        Assert.Equal(100, log.AircraftCyclesAtService);
    }

    [Fact]
    public void CreateInspection_SetsCostAsLabor()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var log = MaintenanceLog.CreateInspection(
            worldId, aircraftId,
            MaintenanceType.AnnualInspection,
            "EGLL", 6000, 100, 1500m);

        Assert.Equal(1500m, log.LaborCost);
        Assert.Equal(0m, log.PartsCost);
    }

    [Fact]
    public void CreateInspection_MarksAsCompleted()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var log = MaintenanceLog.CreateInspection(
            worldId, aircraftId,
            MaintenanceType.AnnualInspection,
            "EGLL", 6000, 100, 1500m);

        Assert.True(log.IsCompleted);
        Assert.NotNull(log.CompletedAt);
    }

    [Fact]
    public void CreateRepair_SetsCorrectValues()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var log = MaintenanceLog.CreateRepair(
            worldId, aircraftId,
            "Engine Repair",
            "Replaced spark plugs",
            "KJFK", 12000, 200,
            400m, 150m, 10);

        Assert.Equal(worldId, log.WorldId);
        Assert.Equal(aircraftId, log.OwnedAircraftId);
        Assert.Equal("Engine Repair", log.Title);
        Assert.Equal("Replaced spark plugs", log.Description);
        Assert.Equal("KJFK", log.PerformedAtAirport);
        Assert.Equal(12000, log.AircraftFlightMinutesAtService);
        Assert.Equal(200, log.AircraftCyclesAtService);
        Assert.Equal(400m, log.LaborCost);
        Assert.Equal(150m, log.PartsCost);
        Assert.Equal(10, log.ConditionImprovement);
        Assert.Equal(MaintenanceType.MinorRepair, log.MaintenanceType);
        Assert.True(log.IsCompleted);
    }

    [Fact]
    public void CreateRepair_WithComponentId_SetsComponentId()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();
        var componentId = Guid.NewGuid();

        var log = MaintenanceLog.CreateRepair(
            worldId, aircraftId,
            "Engine Repair",
            "Replaced spark plugs",
            "KJFK", 12000, 200,
            400m, 150m, 10, componentId);

        Assert.Equal(componentId, log.AircraftComponentId);
    }

    [Fact]
    public void CreateRepair_WithoutComponentId_HasNullComponentId()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var log = MaintenanceLog.CreateRepair(
            worldId, aircraftId,
            "General Repair",
            "Fixed panel",
            "KJFK", 12000, 200,
            100m, 50m, 5);

        Assert.Null(log.AircraftComponentId);
    }
}
