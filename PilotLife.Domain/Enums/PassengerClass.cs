namespace PilotLife.Domain.Enums;

/// <summary>
/// Class of passenger service affecting payout rates.
/// </summary>
public enum PassengerClass
{
    /// <summary>
    /// Standard economy passengers.
    /// Base rate: $150/pax
    /// </summary>
    Economy,

    /// <summary>
    /// Business class passengers.
    /// Base rate: $400/pax
    /// </summary>
    Business,

    /// <summary>
    /// First class passengers.
    /// Base rate: $800/pax
    /// </summary>
    First,

    /// <summary>
    /// Charter flight passengers.
    /// Base rate: $600/pax
    /// </summary>
    Charter,

    /// <summary>
    /// Medical transport (patients, medical staff).
    /// Base rate: $1500/pax
    /// </summary>
    Medical,

    /// <summary>
    /// VIP/executive passengers.
    /// Base rate: $2000/pax
    /// </summary>
    Vip
}
