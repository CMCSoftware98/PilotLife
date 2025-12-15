using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Defines a specific type of cargo with its characteristics and pricing.
/// CargoTypes are global (not world-specific) master data.
/// </summary>
public class CargoType : BaseEntity
{
    /// <summary>
    /// Top-level category of this cargo type.
    /// </summary>
    public CargoCategory Category { get; set; }

    /// <summary>
    /// Subcategory name for organization.
    /// </summary>
    public string Subcategory { get; set; } = string.Empty;

    /// <summary>
    /// Display name for this cargo type.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of this cargo type.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Base rate per pound for this cargo type.
    /// </summary>
    public decimal BaseRatePerLb { get; set; }

    /// <summary>
    /// Typical minimum weight for this cargo (lbs).
    /// </summary>
    public int MinWeightLbs { get; set; } = 100;

    /// <summary>
    /// Typical maximum weight for this cargo (lbs).
    /// </summary>
    public int MaxWeightLbs { get; set; } = 5000;

    /// <summary>
    /// Volume per pound (cubic feet/lb) for density calculations.
    /// </summary>
    public decimal DensityFactor { get; set; } = 0.1m;

    /// <summary>
    /// Whether this cargo requires special handling certification.
    /// </summary>
    public bool RequiresSpecialHandling { get; set; }

    /// <summary>
    /// Type of special handling required (DG, Live Animals, Medical, etc.).
    /// </summary>
    public string? SpecialHandlingType { get; set; }

    /// <summary>
    /// Whether this cargo is temperature sensitive.
    /// </summary>
    public bool IsTemperatureSensitive { get; set; }

    /// <summary>
    /// Whether this cargo is time-critical (affects urgency generation).
    /// </summary>
    public bool IsTimeCritical { get; set; }

    /// <summary>
    /// Whether this cargo is illegal (risk of inspection/fines).
    /// </summary>
    public bool IsIllegal { get; set; }

    /// <summary>
    /// Risk level for illegal cargo (1-5, affects detection chance).
    /// </summary>
    public int? IllegalRiskLevel { get; set; }

    /// <summary>
    /// Payout multiplier for this specific cargo type.
    /// </summary>
    public decimal PayoutMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Whether this cargo type is active and can be generated.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Calculates volume from weight using density factor.
    /// </summary>
    public decimal CalculateVolume(int weightLbs) => weightLbs * DensityFactor;

    /// <summary>
    /// Gets the effective rate per pound including multiplier.
    /// </summary>
    public decimal EffectiveRatePerLb => BaseRatePerLb * PayoutMultiplier;

    /// <summary>
    /// Creates a general cargo type.
    /// </summary>
    public static CargoType CreateGeneralCargo(string name, string subcategory)
    {
        return new CargoType
        {
            Category = CargoCategory.GeneralCargo,
            Subcategory = subcategory,
            Name = name,
            BaseRatePerLb = 0.50m,
            MinWeightLbs = 500,
            MaxWeightLbs = 20000
        };
    }

    /// <summary>
    /// Creates a perishable cargo type.
    /// </summary>
    public static CargoType CreatePerishable(string name, string subcategory)
    {
        return new CargoType
        {
            Category = CargoCategory.Perishable,
            Subcategory = subcategory,
            Name = name,
            BaseRatePerLb = 0.80m,
            MinWeightLbs = 200,
            MaxWeightLbs = 10000,
            IsTemperatureSensitive = true,
            IsTimeCritical = true
        };
    }

    /// <summary>
    /// Creates a hazardous cargo type.
    /// </summary>
    public static CargoType CreateHazardous(string name, string subcategory, string dgClass)
    {
        return new CargoType
        {
            Category = CargoCategory.Hazardous,
            Subcategory = subcategory,
            Name = name,
            BaseRatePerLb = 1.20m,
            MinWeightLbs = 100,
            MaxWeightLbs = 5000,
            RequiresSpecialHandling = true,
            SpecialHandlingType = $"DG-{dgClass}",
            PayoutMultiplier = 1.2m
        };
    }

    /// <summary>
    /// Creates a medical cargo type.
    /// </summary>
    public static CargoType CreateMedical(string name, string subcategory, bool isUrgent = false)
    {
        return new CargoType
        {
            Category = CargoCategory.Medical,
            Subcategory = subcategory,
            Name = name,
            BaseRatePerLb = 2.00m,
            MinWeightLbs = 10,
            MaxWeightLbs = 2000,
            RequiresSpecialHandling = true,
            SpecialHandlingType = "Medical",
            IsTemperatureSensitive = true,
            IsTimeCritical = isUrgent,
            PayoutMultiplier = isUrgent ? 1.5m : 1.0m
        };
    }

    /// <summary>
    /// Creates a high-value cargo type.
    /// </summary>
    public static CargoType CreateHighValue(string name, string subcategory)
    {
        return new CargoType
        {
            Category = CargoCategory.HighValue,
            Subcategory = subcategory,
            Name = name,
            BaseRatePerLb = 3.00m,
            MinWeightLbs = 10,
            MaxWeightLbs = 1000,
            RequiresSpecialHandling = true,
            SpecialHandlingType = "Security",
            DensityFactor = 0.05m
        };
    }

    /// <summary>
    /// Creates a live animal cargo type.
    /// </summary>
    public static CargoType CreateLiveAnimals(string name, string subcategory)
    {
        return new CargoType
        {
            Category = CargoCategory.LiveAnimals,
            Subcategory = subcategory,
            Name = name,
            BaseRatePerLb = 1.50m,
            MinWeightLbs = 50,
            MaxWeightLbs = 5000,
            RequiresSpecialHandling = true,
            SpecialHandlingType = "LiveAnimals",
            IsTemperatureSensitive = true,
            DensityFactor = 0.3m
        };
    }

    /// <summary>
    /// Creates an illegal cargo type.
    /// </summary>
    public static CargoType CreateIllegal(string name, string subcategory, int riskLevel)
    {
        return new CargoType
        {
            Category = CargoCategory.GeneralCargo,
            Subcategory = subcategory,
            Name = name,
            BaseRatePerLb = 5.00m,
            MinWeightLbs = 50,
            MaxWeightLbs = 2000,
            IsIllegal = true,
            IllegalRiskLevel = riskLevel,
            PayoutMultiplier = 2.0m + (riskLevel * 0.5m)
        };
    }
}
