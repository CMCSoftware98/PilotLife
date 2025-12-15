namespace PilotLife.Domain.Enums;

/// <summary>
/// Category of license for grouping and display purposes.
/// </summary>
public enum LicenseCategory
{
    /// <summary>
    /// Core pilot licenses (Discovery, PPL, CPL, ATPL).
    /// </summary>
    Core,

    /// <summary>
    /// Endorsements and ratings (Night, IR, MEP).
    /// </summary>
    Endorsement,

    /// <summary>
    /// Aircraft type ratings for specific aircraft.
    /// </summary>
    TypeRating,

    /// <summary>
    /// Special certifications (Dangerous Goods, etc.).
    /// </summary>
    Certification
}
