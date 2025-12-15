namespace PilotLife.Domain.Enums;

/// <summary>
/// Distance categories for job classification.
/// </summary>
public enum DistanceCategory
{
    /// <summary>
    /// Very short flights (0-50nm).
    /// Training, island hops, urgent medical, local courier.
    /// Base rate: $20/nm
    /// </summary>
    VeryShort,

    /// <summary>
    /// Short flights (50-150nm).
    /// Regional cargo, business passengers, mail.
    /// Base rate: $16/nm
    /// </summary>
    Short,

    /// <summary>
    /// Medium flights (150-500nm).
    /// General cargo, charter flights, livestock.
    /// Base rate: $14/nm
    /// </summary>
    Medium,

    /// <summary>
    /// Long flights (500-1500nm).
    /// High-value cargo, international, time-sensitive.
    /// Base rate: $11/nm
    /// </summary>
    Long,

    /// <summary>
    /// Ultra long flights (1500nm+).
    /// Intercontinental, airline contracts, specialty.
    /// Base rate: $8/nm
    /// </summary>
    UltraLong
}
