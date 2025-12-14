namespace PilotLife.Domain.Enums;

/// <summary>
/// Difficulty levels for worlds, affecting economy multipliers.
/// </summary>
public enum WorldDifficulty
{
    /// <summary>
    /// Casual difficulty: Higher payouts, cheaper prices, forgiving penalties.
    /// </summary>
    Easy,

    /// <summary>
    /// Standard difficulty: Balanced baseline experience.
    /// </summary>
    Medium,

    /// <summary>
    /// Realistic difficulty: Lower payouts, expensive, harsh consequences.
    /// </summary>
    Hard,

    /// <summary>
    /// Server-defined modifiers (future custom worlds).
    /// </summary>
    Custom
}
