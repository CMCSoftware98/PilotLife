namespace PilotLife.Domain.Enums;

/// <summary>
/// Urgency level of a job affecting time window and payout.
/// </summary>
public enum JobUrgency
{
    /// <summary>
    /// Standard delivery window (24-48 hours).
    /// Payout multiplier: 1.0x
    /// </summary>
    Standard,

    /// <summary>
    /// Priority delivery (12-24 hours).
    /// Payout multiplier: 1.2x
    /// </summary>
    Priority,

    /// <summary>
    /// Express delivery (6-12 hours).
    /// Payout multiplier: 1.5x
    /// </summary>
    Express,

    /// <summary>
    /// Urgent delivery (2-6 hours).
    /// Payout multiplier: 2.0x
    /// </summary>
    Urgent,

    /// <summary>
    /// Critical/emergency delivery (1-2 hours).
    /// Payout multiplier: 3.0x
    /// </summary>
    Critical
}
