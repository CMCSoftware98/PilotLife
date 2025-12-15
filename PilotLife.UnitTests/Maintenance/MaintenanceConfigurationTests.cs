using PilotLife.Application.Maintenance;

namespace PilotLife.UnitTests.Maintenance;

public class MaintenanceConfigurationTests
{
    [Fact]
    public void SectionName_IsCorrect()
    {
        Assert.Equal("Maintenance", MaintenanceConfiguration.SectionName);
    }

    [Fact]
    public void DefaultValues_ConditionDegradation_AreCorrect()
    {
        var config = new MaintenanceConfiguration();

        Assert.Equal(0.1, config.BaseConditionDegradationPerHour);
        Assert.Equal(2.0, config.HardLandingConditionPenalty);
        Assert.Equal(1.0, config.OverspeedConditionPenalty);
        Assert.Equal(0.15, config.ComponentDegradationPerHour);
    }

    [Fact]
    public void DefaultValues_InspectionIntervals_AreCorrect()
    {
        var config = new MaintenanceConfiguration();

        Assert.Equal(365, config.AnnualInspectionIntervalDays);
        Assert.Equal(6000, config.HundredHourInspectionMinutes);
    }

    [Fact]
    public void DefaultValues_Costs_AreCorrect()
    {
        var config = new MaintenanceConfiguration();

        Assert.Equal(85m, config.LaborCostPerHour);
        Assert.Equal(1.2m, config.PartsCostMultiplier);
        Assert.Equal(1200m, config.AnnualInspectionBaseCost);
        Assert.Equal(600m, config.HundredHourInspectionBaseCost);
        Assert.Equal(250m, config.MinorRepairBaseCost);
        Assert.Equal(2500m, config.MajorRepairBaseCost);
    }

    [Fact]
    public void DefaultValues_Durations_AreCorrect()
    {
        var config = new MaintenanceConfiguration();

        Assert.Equal(12, config.AnnualInspectionDurationHours);
        Assert.Equal(6, config.HundredHourInspectionDurationHours);
        Assert.Equal(2, config.MinorRepairDurationHours);
        Assert.Equal(24, config.MajorRepairDurationHours);
        Assert.Equal(60, config.EngineOverhaulDurationHours);
    }

    [Fact]
    public void DefaultValues_ConditionImprovements_AreCorrect()
    {
        var config = new MaintenanceConfiguration();

        Assert.Equal(8, config.AnnualInspectionConditionImprovement);
        Assert.Equal(4, config.HundredHourInspectionConditionImprovement);
        Assert.Equal(5, config.MinorRepairConditionImprovement);
        Assert.Equal(15, config.MajorRepairConditionImprovement);
    }

    [Fact]
    public void DefaultValues_WarrantyAndInsurance_AreCorrect()
    {
        var config = new MaintenanceConfiguration();

        Assert.Equal(24, config.DefaultWarrantyMonths);
        Assert.Equal(80m, config.InsuranceCoveragePercent);
        Assert.Equal(100m, config.WarrantyCoveragePercent);
    }

    [Fact]
    public void DefaultValues_Airworthiness_AreCorrect()
    {
        var config = new MaintenanceConfiguration();

        Assert.Equal(60, config.MinAirworthyCondition);
        Assert.Equal(40, config.MinServiceableComponentCondition);
    }

    [Theory]
    [InlineData(0.05)]
    [InlineData(0.1)]
    [InlineData(0.2)]
    [InlineData(0.5)]
    public void BaseConditionDegradationPerHour_CanBeSet(double rate)
    {
        var config = new MaintenanceConfiguration { BaseConditionDegradationPerHour = rate };
        Assert.Equal(rate, config.BaseConditionDegradationPerHour);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(5.0)]
    public void HardLandingConditionPenalty_CanBeSet(double penalty)
    {
        var config = new MaintenanceConfiguration { HardLandingConditionPenalty = penalty };
        Assert.Equal(penalty, config.HardLandingConditionPenalty);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(85)]
    [InlineData(120)]
    public void LaborCostPerHour_CanBeSet(decimal cost)
    {
        var config = new MaintenanceConfiguration { LaborCostPerHour = cost };
        Assert.Equal(cost, config.LaborCostPerHour);
    }

    [Theory]
    [InlineData(12)]
    [InlineData(24)]
    [InlineData(36)]
    public void DefaultWarrantyMonths_CanBeSet(int months)
    {
        var config = new MaintenanceConfiguration { DefaultWarrantyMonths = months };
        Assert.Equal(months, config.DefaultWarrantyMonths);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(60)]
    [InlineData(70)]
    public void MinAirworthyCondition_CanBeSet(int condition)
    {
        var config = new MaintenanceConfiguration { MinAirworthyCondition = condition };
        Assert.Equal(condition, config.MinAirworthyCondition);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(40)]
    [InlineData(50)]
    public void MinServiceableComponentCondition_CanBeSet(int condition)
    {
        var config = new MaintenanceConfiguration { MinServiceableComponentCondition = condition };
        Assert.Equal(condition, config.MinServiceableComponentCondition);
    }

    [Theory]
    [InlineData(180, 3000)] // 180 days, 50 hours
    [InlineData(365, 6000)] // 365 days, 100 hours
    [InlineData(730, 12000)] // 2 years, 200 hours
    public void InspectionIntervals_CanBeSet(int days, int minutes)
    {
        var config = new MaintenanceConfiguration
        {
            AnnualInspectionIntervalDays = days,
            HundredHourInspectionMinutes = minutes
        };

        Assert.Equal(days, config.AnnualInspectionIntervalDays);
        Assert.Equal(minutes, config.HundredHourInspectionMinutes);
    }

    [Fact]
    public void AllCostSettings_CanBeCustomized()
    {
        var config = new MaintenanceConfiguration
        {
            AnnualInspectionBaseCost = 1500m,
            HundredHourInspectionBaseCost = 750m,
            MinorRepairBaseCost = 300m,
            MajorRepairBaseCost = 3000m,
            LaborCostPerHour = 100m,
            PartsCostMultiplier = 1.5m
        };

        Assert.Equal(1500m, config.AnnualInspectionBaseCost);
        Assert.Equal(750m, config.HundredHourInspectionBaseCost);
        Assert.Equal(300m, config.MinorRepairBaseCost);
        Assert.Equal(3000m, config.MajorRepairBaseCost);
        Assert.Equal(100m, config.LaborCostPerHour);
        Assert.Equal(1.5m, config.PartsCostMultiplier);
    }

    [Fact]
    public void AllDurationSettings_CanBeCustomized()
    {
        var config = new MaintenanceConfiguration
        {
            AnnualInspectionDurationHours = 16,
            HundredHourInspectionDurationHours = 8,
            MinorRepairDurationHours = 4,
            MajorRepairDurationHours = 32,
            EngineOverhaulDurationHours = 80
        };

        Assert.Equal(16, config.AnnualInspectionDurationHours);
        Assert.Equal(8, config.HundredHourInspectionDurationHours);
        Assert.Equal(4, config.MinorRepairDurationHours);
        Assert.Equal(32, config.MajorRepairDurationHours);
        Assert.Equal(80, config.EngineOverhaulDurationHours);
    }

    [Fact]
    public void AllConditionImprovementSettings_CanBeCustomized()
    {
        var config = new MaintenanceConfiguration
        {
            AnnualInspectionConditionImprovement = 10,
            HundredHourInspectionConditionImprovement = 5,
            MinorRepairConditionImprovement = 8,
            MajorRepairConditionImprovement = 20
        };

        Assert.Equal(10, config.AnnualInspectionConditionImprovement);
        Assert.Equal(5, config.HundredHourInspectionConditionImprovement);
        Assert.Equal(8, config.MinorRepairConditionImprovement);
        Assert.Equal(20, config.MajorRepairConditionImprovement);
    }
}
