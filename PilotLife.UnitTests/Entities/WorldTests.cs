using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class WorldTests
{
    [Fact]
    public void NewWorld_HasValidId()
    {
        var world = new World();

        Assert.NotEqual(Guid.Empty, world.Id);
    }

    [Fact]
    public void NewWorld_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var world = new World();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(world.CreatedAt, before, after);
    }

    [Fact]
    public void NewWorld_HasDefaultValues()
    {
        var world = new World();

        Assert.Equal(string.Empty, world.Name);
        Assert.Equal(string.Empty, world.Slug);
        Assert.Null(world.Description);
        Assert.Equal(WorldDifficulty.Medium, world.Difficulty);
        Assert.Equal(50000m, world.StartingCapital);
        Assert.Equal(1.0m, world.JobPayoutMultiplier);
        Assert.Equal(1.0m, world.AircraftPriceMultiplier);
        Assert.Equal(1.0m, world.MaintenanceCostMultiplier);
        Assert.Equal(1.0m, world.LicenseCostMultiplier);
        Assert.Equal(1.0m, world.LoanInterestMultiplier);
        Assert.Equal(1.0m, world.DetectionRiskMultiplier);
        Assert.Equal(1.0m, world.FineMultiplier);
        Assert.Equal(1.0m, world.JobExpiryMultiplier);
        Assert.Equal(1.0m, world.CreditRecoveryMultiplier);
        Assert.Equal(1.0m, world.WorkerSalaryMultiplier);
        Assert.True(world.IsActive);
        Assert.False(world.IsDefault);
        Assert.Equal(0, world.MaxPlayers);
    }

    [Fact]
    public void CreateEasy_HasCorrectDifficulty()
    {
        var world = World.CreateEasy();

        Assert.Equal(WorldDifficulty.Easy, world.Difficulty);
    }

    [Fact]
    public void CreateEasy_HasHigherStartingCapital()
    {
        var world = World.CreateEasy();

        Assert.Equal(100000m, world.StartingCapital);
    }

    [Fact]
    public void CreateEasy_HasHigherJobPayout()
    {
        var world = World.CreateEasy();

        Assert.Equal(1.5m, world.JobPayoutMultiplier);
    }

    [Fact]
    public void CreateEasy_HasLowerCosts()
    {
        var world = World.CreateEasy();

        Assert.Equal(0.7m, world.AircraftPriceMultiplier);
        Assert.Equal(0.5m, world.MaintenanceCostMultiplier);
        Assert.Equal(0.5m, world.LicenseCostMultiplier);
        Assert.Equal(0.5m, world.LoanInterestMultiplier);
        Assert.Equal(0.7m, world.WorkerSalaryMultiplier);
    }

    [Fact]
    public void CreateEasy_HasLowerPenalties()
    {
        var world = World.CreateEasy();

        Assert.Equal(0.5m, world.DetectionRiskMultiplier);
        Assert.Equal(0.5m, world.FineMultiplier);
    }

    [Fact]
    public void CreateEasy_HasMoreForgivingTimers()
    {
        var world = World.CreateEasy();

        Assert.Equal(2.0m, world.JobExpiryMultiplier);
        Assert.Equal(2.0m, world.CreditRecoveryMultiplier);
    }

    [Fact]
    public void CreateEasy_UsesCustomNameAndSlug()
    {
        var world = World.CreateEasy("My Easy World", "my-easy-world");

        Assert.Equal("My Easy World", world.Name);
        Assert.Equal("my-easy-world", world.Slug);
    }

    [Fact]
    public void CreateMedium_HasCorrectDifficulty()
    {
        var world = World.CreateMedium();

        Assert.Equal(WorldDifficulty.Medium, world.Difficulty);
    }

    [Fact]
    public void CreateMedium_HasBalancedMultipliers()
    {
        var world = World.CreateMedium();

        Assert.Equal(50000m, world.StartingCapital);
        Assert.Equal(1.0m, world.JobPayoutMultiplier);
        Assert.Equal(1.0m, world.AircraftPriceMultiplier);
        Assert.Equal(1.0m, world.MaintenanceCostMultiplier);
        Assert.Equal(1.0m, world.LicenseCostMultiplier);
        Assert.Equal(1.0m, world.LoanInterestMultiplier);
        Assert.Equal(1.0m, world.DetectionRiskMultiplier);
        Assert.Equal(1.0m, world.FineMultiplier);
        Assert.Equal(1.0m, world.JobExpiryMultiplier);
        Assert.Equal(1.0m, world.CreditRecoveryMultiplier);
        Assert.Equal(1.0m, world.WorkerSalaryMultiplier);
    }

    [Fact]
    public void CreateMedium_IsDefaultWorld()
    {
        var world = World.CreateMedium();

        Assert.True(world.IsDefault);
    }

    [Fact]
    public void CreateHard_HasCorrectDifficulty()
    {
        var world = World.CreateHard();

        Assert.Equal(WorldDifficulty.Hard, world.Difficulty);
    }

    [Fact]
    public void CreateHard_HasLowerStartingCapital()
    {
        var world = World.CreateHard();

        Assert.Equal(25000m, world.StartingCapital);
    }

    [Fact]
    public void CreateHard_HasLowerJobPayout()
    {
        var world = World.CreateHard();

        Assert.Equal(0.7m, world.JobPayoutMultiplier);
    }

    [Fact]
    public void CreateHard_HasHigherCosts()
    {
        var world = World.CreateHard();

        Assert.Equal(1.3m, world.AircraftPriceMultiplier);
        Assert.Equal(1.5m, world.MaintenanceCostMultiplier);
        Assert.Equal(1.5m, world.LicenseCostMultiplier);
        Assert.Equal(1.5m, world.LoanInterestMultiplier);
        Assert.Equal(1.3m, world.WorkerSalaryMultiplier);
    }

    [Fact]
    public void CreateHard_HasHigherPenalties()
    {
        var world = World.CreateHard();

        Assert.Equal(1.5m, world.DetectionRiskMultiplier);
        Assert.Equal(2.0m, world.FineMultiplier);
    }

    [Fact]
    public void CreateHard_HasStricterTimers()
    {
        var world = World.CreateHard();

        Assert.Equal(0.5m, world.JobExpiryMultiplier);
        Assert.Equal(0.5m, world.CreditRecoveryMultiplier);
    }

    [Fact]
    public void Players_DefaultsToEmptyCollection()
    {
        var world = new World();

        Assert.NotNull(world.Players);
        Assert.Empty(world.Players);
    }
}
