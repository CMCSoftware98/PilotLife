using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a player's progress in a specific skill.
/// </summary>
public class PlayerSkill : BaseEntity
{
    /// <summary>
    /// The player world this skill belongs to.
    /// </summary>
    public Guid PlayerWorldId { get; set; }
    public PlayerWorld PlayerWorld { get; set; } = null!;

    /// <summary>
    /// The type of skill.
    /// </summary>
    public SkillType SkillType { get; set; }

    /// <summary>
    /// Current experience points in this skill.
    /// </summary>
    public int CurrentXp { get; set; }

    /// <summary>
    /// Current level (1-8).
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    /// When this skill was last updated.
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// XP events history for this skill.
    /// </summary>
    public ICollection<SkillXpEvent> XpEvents { get; set; } = new List<SkillXpEvent>();

    // Level thresholds
    private static readonly int[] LevelThresholds = [0, 100, 300, 600, 1000, 1500, 2500, 4000];

    /// <summary>
    /// Gets the XP required for the next level.
    /// </summary>
    public int XpForNextLevel => Level < 8 ? LevelThresholds[Level] : int.MaxValue;

    /// <summary>
    /// Gets the XP required for the current level.
    /// </summary>
    public int XpForCurrentLevel => Level > 1 ? LevelThresholds[Level - 1] : 0;

    /// <summary>
    /// Gets progress towards the next level as a percentage (0-100).
    /// </summary>
    public double ProgressToNextLevel
    {
        get
        {
            if (Level >= 8) return 100;
            var xpInLevel = CurrentXp - XpForCurrentLevel;
            var xpNeeded = XpForNextLevel - XpForCurrentLevel;
            return xpNeeded > 0 ? Math.Min(100, (double)xpInLevel / xpNeeded * 100) : 100;
        }
    }

    /// <summary>
    /// Gets the name of the current skill level.
    /// </summary>
    public string LevelName => Level switch
    {
        1 => "Beginner",
        2 => "Novice",
        3 => "Apprentice",
        4 => "Journeyman",
        5 => "Expert",
        6 => "Master",
        7 => "Grandmaster",
        8 => "Legend",
        _ => "Unknown"
    };

    /// <summary>
    /// Adds XP to this skill and handles level ups.
    /// </summary>
    /// <returns>Number of levels gained.</returns>
    public int AddXp(int xp)
    {
        if (xp <= 0) return 0;

        CurrentXp += xp;
        LastUpdatedAt = DateTimeOffset.UtcNow;

        var levelsGained = 0;
        while (Level < 8 && CurrentXp >= XpForNextLevel)
        {
            Level++;
            levelsGained++;
        }

        return levelsGained;
    }

    /// <summary>
    /// Gets the level for a given XP amount.
    /// </summary>
    public static int GetLevelForXp(int xp)
    {
        for (int i = LevelThresholds.Length - 1; i >= 0; i--)
        {
            if (xp >= LevelThresholds[i])
                return i + 1;
        }
        return 1;
    }

    /// <summary>
    /// Creates a new skill for a player.
    /// </summary>
    public static PlayerSkill Create(Guid playerWorldId, SkillType skillType)
    {
        return new PlayerSkill
        {
            PlayerWorldId = playerWorldId,
            SkillType = skillType,
            CurrentXp = 0,
            Level = 1
        };
    }
}
