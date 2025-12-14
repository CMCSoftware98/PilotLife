using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class AircraftModificationTests
{
    [Fact]
    public void NewAircraftModification_HasValidId()
    {
        var modification = new AircraftModification();

        Assert.NotEqual(Guid.Empty, modification.Id);
    }

    [Fact]
    public void NewAircraftModification_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var modification = new AircraftModification();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(modification.CreatedAt, before, after);
    }

    [Fact]
    public void NewAircraftModification_HasDefaultValues()
    {
        var modification = new AircraftModification();

        Assert.True(modification.IsActive);
        Assert.True(modification.IsRemovable);
        Assert.Equal(1.0m, modification.FuelConsumptionMultiplier);
        Assert.Equal(1.0m, modification.TakeoffDistanceMultiplier);
        Assert.Equal(1.0m, modification.LandingDistanceMultiplier);
        Assert.False(modification.EnablesWaterOperations);
        Assert.False(modification.EnablesSnowOperations);
        Assert.False(modification.EnablesStolOperations);
        Assert.False(modification.EnablesIfrOperations);
        Assert.False(modification.EnablesNightOperations);
        Assert.False(modification.EnablesKnownIcing);
    }

    [Fact]
    public void CreateCargoConversion_25Percent_HasCorrectType()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var mod = AircraftModification.CreateCargoConversion(worldId, aircraftId, 25, 1000, 6);

        Assert.Equal(ModificationType.CargoConversion25, mod.ModificationType);
    }

    [Fact]
    public void CreateCargoConversion_50Percent_HasCorrectType()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var mod = AircraftModification.CreateCargoConversion(worldId, aircraftId, 50, 1000, 6);

        Assert.Equal(ModificationType.CargoConversion50, mod.ModificationType);
    }

    [Fact]
    public void CreateCargoConversion_75Percent_HasCorrectType()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var mod = AircraftModification.CreateCargoConversion(worldId, aircraftId, 75, 1000, 6);

        Assert.Equal(ModificationType.CargoConversion75, mod.ModificationType);
    }

    [Fact]
    public void CreateCargoConversion_100Percent_HasCorrectType()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var mod = AircraftModification.CreateCargoConversion(worldId, aircraftId, 100, 1000, 6);

        Assert.Equal(ModificationType.CargoConversion100, mod.ModificationType);
    }

    [Fact]
    public void CreateCargoConversion_CalculatesCargoCapacityChange()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var mod = AircraftModification.CreateCargoConversion(worldId, aircraftId, 50, 1000, 6);

        Assert.Equal(500, mod.CargoCapacityChangeLbs); // 50% of 1000 lbs
    }

    [Fact]
    public void CreateCargoConversion_CalculatesPassengerCapacityReduction()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var mod = AircraftModification.CreateCargoConversion(worldId, aircraftId, 50, 1000, 6);

        Assert.Equal(-3, mod.PassengerCapacityChange); // 50% of 6 seats removed
    }

    [Fact]
    public void CreateCargoConversion_SetsCorrectIds()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var mod = AircraftModification.CreateCargoConversion(worldId, aircraftId, 50, 1000, 6);

        Assert.Equal(worldId, mod.WorldId);
        Assert.Equal(aircraftId, mod.OwnedAircraftId);
    }

    [Fact]
    public void CreateCargoConversion_SetsNameAndDescription()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var mod = AircraftModification.CreateCargoConversion(worldId, aircraftId, 50, 1000, 6);

        Assert.Equal("50% Cargo Conversion", mod.Name);
        Assert.Contains("50%", mod.Description);
    }

    [Fact]
    public void CreateCargoConversion_IsRemovable()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var mod = AircraftModification.CreateCargoConversion(worldId, aircraftId, 50, 1000, 6);

        Assert.True(mod.IsRemovable);
    }

    [Fact]
    public void CreateCargoConversion_WithInvalidPercent_DefaultsTo50()
    {
        var worldId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var mod = AircraftModification.CreateCargoConversion(worldId, aircraftId, 33, 1000, 6);

        Assert.Equal(ModificationType.CargoConversion50, mod.ModificationType);
    }
}
