using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class AircraftComponentTests
{
    [Fact]
    public void NewAircraftComponent_HasValidId()
    {
        var component = new AircraftComponent();

        Assert.NotEqual(Guid.Empty, component.Id);
    }

    [Fact]
    public void NewAircraftComponent_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var component = new AircraftComponent();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(component.CreatedAt, before, after);
    }

    [Fact]
    public void NewAircraftComponent_HasDefaultValues()
    {
        var component = new AircraftComponent();

        Assert.Equal(100, component.Condition);
        Assert.Equal(0, component.OperatingMinutes);
        Assert.Equal(0, component.Cycles);
        Assert.Equal(0, component.TimeSinceOverhaul);
        Assert.Null(component.TimeBetweenOverhaul);
        Assert.False(component.IsLifeLimited);
        Assert.Null(component.LifeLimitMinutes);
        Assert.Null(component.LifeLimitCycles);
        Assert.True(component.IsServiceable);
    }

    [Fact]
    public void OperatingHours_ConvertsMinutesToHours()
    {
        var component = new AircraftComponent { OperatingMinutes = 180 };

        Assert.Equal(3.0, component.OperatingHours);
    }

    [Fact]
    public void TimeSinceOverhaulHours_ConvertsMinutesToHours()
    {
        var component = new AircraftComponent { TimeSinceOverhaul = 90 };

        Assert.Equal(1.5, component.TimeSinceOverhaulHours);
    }

    [Fact]
    public void TboPercentUsed_WhenNoTbo_ReturnsNull()
    {
        var component = new AircraftComponent
        {
            TimeSinceOverhaul = 6000,
            TimeBetweenOverhaul = null
        };

        Assert.Null(component.TboPercentUsed);
    }

    [Fact]
    public void TboPercentUsed_CalculatesCorrectPercentage()
    {
        var component = new AircraftComponent
        {
            TimeSinceOverhaul = 60000, // 1000 hours
            TimeBetweenOverhaul = 120000 // 2000 hours TBO
        };

        Assert.Equal(50.0, component.TboPercentUsed);
    }

    [Fact]
    public void IsTboApproaching_WhenAt90Percent_ReturnsTrue()
    {
        var component = new AircraftComponent
        {
            TimeSinceOverhaul = 108000, // 1800 hours
            TimeBetweenOverhaul = 120000 // 2000 hours TBO
        };

        Assert.True(component.IsTboApproaching);
    }

    [Fact]
    public void IsTboApproaching_WhenBelow90Percent_ReturnsFalse()
    {
        var component = new AircraftComponent
        {
            TimeSinceOverhaul = 60000, // 1000 hours (50%)
            TimeBetweenOverhaul = 120000 // 2000 hours TBO
        };

        Assert.False(component.IsTboApproaching);
    }

    [Fact]
    public void IsTboExceeded_WhenAt100Percent_ReturnsTrue()
    {
        var component = new AircraftComponent
        {
            TimeSinceOverhaul = 120000, // 2000 hours
            TimeBetweenOverhaul = 120000 // 2000 hours TBO
        };

        Assert.True(component.IsTboExceeded);
    }

    [Fact]
    public void IsTboExceeded_WhenBelow100Percent_ReturnsFalse()
    {
        var component = new AircraftComponent
        {
            TimeSinceOverhaul = 119000,
            TimeBetweenOverhaul = 120000
        };

        Assert.False(component.IsTboExceeded);
    }

    [Fact]
    public void LifePercentUsed_WhenNotLifeLimited_ReturnsNull()
    {
        var component = new AircraftComponent
        {
            IsLifeLimited = false,
            OperatingMinutes = 6000,
            Cycles = 100
        };

        Assert.Null(component.LifePercentUsed);
    }

    [Fact]
    public void LifePercentUsed_WhenLifeLimited_ReturnsHigherOfTwoLimits()
    {
        var component = new AircraftComponent
        {
            IsLifeLimited = true,
            OperatingMinutes = 30000, // 500 hours
            LifeLimitMinutes = 60000, // 1000 hour limit = 50%
            Cycles = 600,
            LifeLimitCycles = 1000 // 60%
        };

        Assert.Equal(60.0, component.LifePercentUsed); // Returns higher percentage
    }

    [Fact]
    public void NeedsAttention_WhenConditionBelow70_ReturnsTrue()
    {
        var component = new AircraftComponent { Condition = 69 };

        Assert.True(component.NeedsAttention);
    }

    [Fact]
    public void NeedsAttention_WhenTboApproaching_ReturnsTrue()
    {
        var component = new AircraftComponent
        {
            Condition = 100,
            TimeSinceOverhaul = 108000,
            TimeBetweenOverhaul = 120000
        };

        Assert.True(component.NeedsAttention);
    }

    [Fact]
    public void NeedsAttention_WhenLifePercentHigh_ReturnsTrue()
    {
        var component = new AircraftComponent
        {
            Condition = 100,
            IsLifeLimited = true,
            OperatingMinutes = 54000, // 900 hours = 90%
            LifeLimitMinutes = 60000
        };

        Assert.True(component.NeedsAttention);
    }

    [Fact]
    public void NeedsAttention_WhenAllGood_ReturnsFalse()
    {
        var component = new AircraftComponent
        {
            Condition = 80,
            TimeSinceOverhaul = 60000,
            TimeBetweenOverhaul = 120000
        };

        Assert.False(component.NeedsAttention);
    }

    [Fact]
    public void CreateDefaultComponents_CreatesFourBaseComponents()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var components = AircraftComponent.CreateDefaultComponents(worldId, aircraftId, 0);

        Assert.Equal(4, components.Count);
        Assert.Contains(components, c => c.ComponentType == ComponentType.Wings);
        Assert.Contains(components, c => c.ComponentType == ComponentType.LandingGear);
        Assert.Contains(components, c => c.ComponentType == ComponentType.Fuselage);
        Assert.Contains(components, c => c.ComponentType == ComponentType.Avionics);
    }

    [Fact]
    public void CreateDefaultComponents_WithSingleEngine_CreatesFiveComponents()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var components = AircraftComponent.CreateDefaultComponents(worldId, aircraftId, 1);

        Assert.Equal(5, components.Count);
        Assert.Contains(components, c => c.ComponentType == ComponentType.Engine1);
    }

    [Fact]
    public void CreateDefaultComponents_WithTwinEngines_CreatesSixComponents()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var components = AircraftComponent.CreateDefaultComponents(worldId, aircraftId, 2);

        Assert.Equal(6, components.Count);
        Assert.Contains(components, c => c.ComponentType == ComponentType.Engine1);
        Assert.Contains(components, c => c.ComponentType == ComponentType.Engine2);
    }

    [Fact]
    public void CreateDefaultComponents_WithFourEngines_CreatesEightComponents()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var components = AircraftComponent.CreateDefaultComponents(worldId, aircraftId, 4);

        Assert.Equal(8, components.Count);
        Assert.Contains(components, c => c.ComponentType == ComponentType.Engine1);
        Assert.Contains(components, c => c.ComponentType == ComponentType.Engine2);
        Assert.Contains(components, c => c.ComponentType == ComponentType.Engine3);
        Assert.Contains(components, c => c.ComponentType == ComponentType.Engine4);
    }

    [Fact]
    public void CreateDefaultComponents_SetsCorrectWorldAndAircraftIds()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var components = AircraftComponent.CreateDefaultComponents(worldId, aircraftId, 1);

        Assert.All(components, c =>
        {
            Assert.Equal(worldId, c.WorldId);
            Assert.Equal(aircraftId, c.OwnedAircraftId);
        });
    }

    [Fact]
    public void CreateDefaultComponents_EnginesHaveDefaultTbo()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var components = AircraftComponent.CreateDefaultComponents(worldId, aircraftId, 2);
        var engines = components.Where(c =>
            c.ComponentType == ComponentType.Engine1 ||
            c.ComponentType == ComponentType.Engine2);

        Assert.All(engines, e => Assert.Equal(120000, e.TimeBetweenOverhaul));
    }
}
