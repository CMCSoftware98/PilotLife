namespace PilotLife.Domain.Enums;

/// <summary>
/// Route difficulty affecting payout multipliers.
/// </summary>
public enum RouteDifficulty
{
    /// <summary>
    /// Easy route - good weather, simple terrain.
    /// Payout multiplier: 1.0x
    /// </summary>
    Easy,

    /// <summary>
    /// Moderate route - some challenges.
    /// Payout multiplier: 1.15x
    /// </summary>
    Moderate,

    /// <summary>
    /// Challenging route - mountain/ocean crossings.
    /// Payout multiplier: 1.3x
    /// </summary>
    Challenging,

    /// <summary>
    /// Difficult route - remote, harsh conditions.
    /// Payout multiplier: 1.5x
    /// </summary>
    Difficult,

    /// <summary>
    /// Extreme route - bush strips, extreme terrain.
    /// Payout multiplier: 2.0x
    /// </summary>
    Extreme
}
