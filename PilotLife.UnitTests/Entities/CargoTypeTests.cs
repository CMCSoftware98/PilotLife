using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class CargoTypeTests
{
    [Fact]
    public void NewCargoType_HasValidId()
    {
        var cargoType = new CargoType();

        Assert.NotEqual(Guid.Empty, cargoType.Id);
    }

    [Fact]
    public void NewCargoType_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var cargoType = new CargoType();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(cargoType.CreatedAt, before, after);
    }

    [Fact]
    public void NewCargoType_HasDefaultValues()
    {
        var cargoType = new CargoType();

        Assert.Equal(100, cargoType.MinWeightLbs);
        Assert.Equal(5000, cargoType.MaxWeightLbs);
        Assert.Equal(0.1m, cargoType.DensityFactor);
        Assert.False(cargoType.RequiresSpecialHandling);
        Assert.False(cargoType.IsTemperatureSensitive);
        Assert.False(cargoType.IsTimeCritical);
        Assert.False(cargoType.IsIllegal);
        Assert.Equal(1.0m, cargoType.PayoutMultiplier);
        Assert.True(cargoType.IsActive);
    }

    [Fact]
    public void CalculateVolume_ReturnsCorrectValue()
    {
        var cargoType = new CargoType { DensityFactor = 0.15m };

        var volume = cargoType.CalculateVolume(1000);

        Assert.Equal(150m, volume);
    }

    [Fact]
    public void EffectiveRatePerLb_IncludesMultiplier()
    {
        var cargoType = new CargoType
        {
            BaseRatePerLb = 2.0m,
            PayoutMultiplier = 1.5m
        };

        Assert.Equal(3.0m, cargoType.EffectiveRatePerLb);
    }

    [Fact]
    public void CreateGeneralCargo_HasCorrectCategory()
    {
        var cargoType = CargoType.CreateGeneralCargo("Boxes", "Freight");

        Assert.Equal(CargoCategory.GeneralCargo, cargoType.Category);
        Assert.Equal("Freight", cargoType.Subcategory);
        Assert.Equal("Boxes", cargoType.Name);
        Assert.Equal(0.50m, cargoType.BaseRatePerLb);
    }

    [Fact]
    public void CreatePerishable_HasCorrectProperties()
    {
        var cargoType = CargoType.CreatePerishable("Fresh Seafood", "Food");

        Assert.Equal(CargoCategory.Perishable, cargoType.Category);
        Assert.Equal(0.80m, cargoType.BaseRatePerLb);
        Assert.True(cargoType.IsTemperatureSensitive);
        Assert.True(cargoType.IsTimeCritical);
    }

    [Fact]
    public void CreateHazardous_HasCorrectProperties()
    {
        var cargoType = CargoType.CreateHazardous("Flammable Liquids", "Chemicals", "3");

        Assert.Equal(CargoCategory.Hazardous, cargoType.Category);
        Assert.Equal(1.20m, cargoType.BaseRatePerLb);
        Assert.True(cargoType.RequiresSpecialHandling);
        Assert.Equal("DG-3", cargoType.SpecialHandlingType);
        Assert.Equal(1.2m, cargoType.PayoutMultiplier);
    }

    [Fact]
    public void CreateMedical_HasCorrectProperties()
    {
        var cargoType = CargoType.CreateMedical("Blood Samples", "Lab Specimens", isUrgent: false);

        Assert.Equal(CargoCategory.Medical, cargoType.Category);
        Assert.Equal(2.00m, cargoType.BaseRatePerLb);
        Assert.True(cargoType.RequiresSpecialHandling);
        Assert.Equal("Medical", cargoType.SpecialHandlingType);
        Assert.True(cargoType.IsTemperatureSensitive);
        Assert.False(cargoType.IsTimeCritical);
        Assert.Equal(1.0m, cargoType.PayoutMultiplier);
    }

    [Fact]
    public void CreateMedical_Urgent_HasHigherMultiplier()
    {
        var cargoType = CargoType.CreateMedical("Organs", "Transplant", isUrgent: true);

        Assert.True(cargoType.IsTimeCritical);
        Assert.Equal(1.5m, cargoType.PayoutMultiplier);
    }

    [Fact]
    public void CreateHighValue_HasCorrectProperties()
    {
        var cargoType = CargoType.CreateHighValue("Diamond Shipment", "Jewelry");

        Assert.Equal(CargoCategory.HighValue, cargoType.Category);
        Assert.Equal(3.00m, cargoType.BaseRatePerLb);
        Assert.True(cargoType.RequiresSpecialHandling);
        Assert.Equal("Security", cargoType.SpecialHandlingType);
        Assert.Equal(0.05m, cargoType.DensityFactor);
    }

    [Fact]
    public void CreateLiveAnimals_HasCorrectProperties()
    {
        var cargoType = CargoType.CreateLiveAnimals("Zoo Transfers", "Wildlife");

        Assert.Equal(CargoCategory.LiveAnimals, cargoType.Category);
        Assert.Equal(1.50m, cargoType.BaseRatePerLb);
        Assert.True(cargoType.RequiresSpecialHandling);
        Assert.Equal("LiveAnimals", cargoType.SpecialHandlingType);
        Assert.True(cargoType.IsTemperatureSensitive);
        Assert.Equal(0.3m, cargoType.DensityFactor);
    }

    [Fact]
    public void CreateIllegal_HasCorrectProperties()
    {
        var cargoType = CargoType.CreateIllegal("Contraband", "Smuggling", riskLevel: 3);

        Assert.True(cargoType.IsIllegal);
        Assert.Equal(3, cargoType.IllegalRiskLevel);
        Assert.Equal(5.00m, cargoType.BaseRatePerLb);
        Assert.Equal(3.5m, cargoType.PayoutMultiplier); // 2.0 + (3 * 0.5)
    }

    [Fact]
    public void CreateIllegal_HighRisk_HasHigherMultiplier()
    {
        var cargoType = CargoType.CreateIllegal("High Risk Contraband", "Smuggling", riskLevel: 5);

        Assert.Equal(5, cargoType.IllegalRiskLevel);
        Assert.Equal(4.5m, cargoType.PayoutMultiplier); // 2.0 + (5 * 0.5)
    }
}
