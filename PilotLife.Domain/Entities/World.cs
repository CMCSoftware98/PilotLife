using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a game world with isolated player progress.
/// Players can join multiple worlds, each with separate economy and progression.
/// </summary>
public class World : BaseEntity
{
    /// <summary>
    /// Display name of the world (e.g., "Easy", "Medium", "Hard").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly identifier (e.g., "easy", "medium", "hard").
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Description of the world and its rules.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Base difficulty level affecting economy multipliers.
    /// </summary>
    public WorldDifficulty Difficulty { get; set; } = WorldDifficulty.Medium;

    // Economy Multipliers
    /// <summary>
    /// Starting capital for new players joining this world.
    /// </summary>
    public decimal StartingCapital { get; set; } = 50000m;

    /// <summary>
    /// Multiplier for job payouts (1.0 = 100%).
    /// </summary>
    public decimal JobPayoutMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Multiplier for aircraft purchase prices.
    /// </summary>
    public decimal AircraftPriceMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Multiplier for maintenance costs.
    /// </summary>
    public decimal MaintenanceCostMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Multiplier for license costs.
    /// </summary>
    public decimal LicenseCostMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Multiplier for loan interest rates.
    /// </summary>
    public decimal LoanInterestMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Multiplier for illegal cargo detection risk.
    /// </summary>
    public decimal DetectionRiskMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Multiplier for fines and penalties.
    /// </summary>
    public decimal FineMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Multiplier for job expiry time (higher = more time).
    /// </summary>
    public decimal JobExpiryMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Multiplier for credit score recovery rate.
    /// </summary>
    public decimal CreditRecoveryMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Multiplier for worker/crew salaries.
    /// </summary>
    public decimal WorkerSalaryMultiplier { get; set; } = 1.0m;

    // Status
    /// <summary>
    /// Whether the world is active and accepting players.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is the default world for new players.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Maximum number of players allowed (0 = unlimited).
    /// </summary>
    public int MaxPlayers { get; set; }

    // Navigation properties
    /// <summary>
    /// Configurable settings for this world (admin-adjustable).
    /// </summary>
    public WorldSettings? Settings { get; set; }

    /// <summary>
    /// Players participating in this world.
    /// </summary>
    public ICollection<PlayerWorld> Players { get; set; } = new List<PlayerWorld>();

    /// <summary>
    /// Creates a world with default Easy difficulty settings.
    /// </summary>
    public static World CreateEasy(string name = "Easy", string slug = "easy") => new()
    {
        Name = name,
        Slug = slug,
        Description = "Higher payouts, cheaper prices, forgiving penalties.",
        Difficulty = WorldDifficulty.Easy,
        StartingCapital = 100000m,
        JobPayoutMultiplier = 1.5m,
        AircraftPriceMultiplier = 0.7m,
        MaintenanceCostMultiplier = 0.5m,
        LicenseCostMultiplier = 0.5m,
        LoanInterestMultiplier = 0.5m,
        DetectionRiskMultiplier = 0.5m,
        FineMultiplier = 0.5m,
        JobExpiryMultiplier = 2.0m,
        CreditRecoveryMultiplier = 2.0m,
        WorkerSalaryMultiplier = 0.7m
    };

    /// <summary>
    /// Creates a world with default Medium difficulty settings.
    /// </summary>
    public static World CreateMedium(string name = "Medium", string slug = "medium") => new()
    {
        Name = name,
        Slug = slug,
        Description = "Balanced baseline experience.",
        Difficulty = WorldDifficulty.Medium,
        StartingCapital = 50000m,
        JobPayoutMultiplier = 1.0m,
        AircraftPriceMultiplier = 1.0m,
        MaintenanceCostMultiplier = 1.0m,
        LicenseCostMultiplier = 1.0m,
        LoanInterestMultiplier = 1.0m,
        DetectionRiskMultiplier = 1.0m,
        FineMultiplier = 1.0m,
        JobExpiryMultiplier = 1.0m,
        CreditRecoveryMultiplier = 1.0m,
        WorkerSalaryMultiplier = 1.0m,
        IsDefault = true
    };

    /// <summary>
    /// Creates a world with default Hard difficulty settings.
    /// </summary>
    public static World CreateHard(string name = "Hard", string slug = "hard") => new()
    {
        Name = name,
        Slug = slug,
        Description = "Lower payouts, expensive, harsh consequences.",
        Difficulty = WorldDifficulty.Hard,
        StartingCapital = 25000m,
        JobPayoutMultiplier = 0.7m,
        AircraftPriceMultiplier = 1.3m,
        MaintenanceCostMultiplier = 1.5m,
        LicenseCostMultiplier = 1.5m,
        LoanInterestMultiplier = 1.5m,
        DetectionRiskMultiplier = 1.5m,
        FineMultiplier = 2.0m,
        JobExpiryMultiplier = 0.5m,
        CreditRecoveryMultiplier = 0.5m,
        WorkerSalaryMultiplier = 1.3m
    };
}
