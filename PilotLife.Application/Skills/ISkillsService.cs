using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.Application.Skills;

/// <summary>
/// Service interface for managing player skills.
/// </summary>
public interface ISkillsService
{
    /// <summary>
    /// Gets all skills for a player.
    /// </summary>
    Task<IEnumerable<PlayerSkillStatus>> GetAllSkillsAsync(Guid playerWorldId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific skill for a player.
    /// </summary>
    Task<PlayerSkillStatus?> GetSkillAsync(Guid playerWorldId, SkillType skillType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds XP to a skill.
    /// </summary>
    Task<SkillXpResult> AddXpAsync(
        Guid playerWorldId,
        SkillType skillType,
        int xp,
        string source,
        string? description = null,
        Guid? relatedFlightId = null,
        Guid? relatedJobId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets XP history for a player's skills.
    /// </summary>
    Task<IEnumerable<SkillXpEvent>> GetXpHistoryAsync(
        Guid playerWorldId,
        SkillType? skillType = null,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes all skills for a new player.
    /// </summary>
    Task InitializeSkillsAsync(Guid playerWorldId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a completed flight and awards XP accordingly.
    /// </summary>
    Task ProcessFlightCompletedAsync(
        Guid playerWorldId,
        TrackedFlight flight,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a completed job and awards XP accordingly.
    /// </summary>
    Task ProcessJobCompletedAsync(
        Guid playerWorldId,
        Job job,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total skill level (sum of all skill levels).
    /// </summary>
    Task<int> GetTotalSkillLevelAsync(Guid playerWorldId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Status of a player's skill.
/// </summary>
public class PlayerSkillStatus
{
    public required Guid SkillId { get; set; }
    public required SkillType SkillType { get; set; }
    public required string SkillName { get; set; }
    public required string Description { get; set; }
    public required int CurrentXp { get; set; }
    public required int Level { get; set; }
    public required string LevelName { get; set; }
    public required int XpForNextLevel { get; set; }
    public required int XpForCurrentLevel { get; set; }
    public required double ProgressToNextLevel { get; set; }
    public required bool IsMaxLevel { get; set; }
}

/// <summary>
/// Result of adding XP to a skill.
/// </summary>
public class SkillXpResult
{
    public required bool Success { get; set; }
    public required SkillType SkillType { get; set; }
    public required int XpGained { get; set; }
    public required int TotalXp { get; set; }
    public required int Level { get; set; }
    public required int LevelsGained { get; set; }
    public string? Message { get; set; }
}
