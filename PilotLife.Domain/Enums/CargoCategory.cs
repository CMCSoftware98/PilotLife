namespace PilotLife.Domain.Enums;

/// <summary>
/// Top-level categories of cargo with different rate multipliers.
/// </summary>
public enum CargoCategory
{
    /// <summary>
    /// Standard freight - boxes, pallets, general goods.
    /// Base rate: $0.50/lb
    /// </summary>
    GeneralCargo,

    /// <summary>
    /// Time-sensitive items - food, flowers, seafood, produce.
    /// Base rate: $0.80/lb
    /// </summary>
    Perishable,

    /// <summary>
    /// Dangerous goods - chemicals, flammables, radioactive.
    /// Base rate: $1.20/lb. Requires DG certification.
    /// </summary>
    Hazardous,

    /// <summary>
    /// Live animals - livestock, pets, zoo transfers.
    /// Base rate: $1.50/lb. Requires animal transport cert.
    /// </summary>
    LiveAnimals,

    /// <summary>
    /// Large/awkward items - machinery, vehicles, equipment.
    /// Base rate: $0.70/lb
    /// </summary>
    Oversized,

    /// <summary>
    /// Medical supplies, organs, blood, pharmaceuticals.
    /// Base rate: $2.00/lb. Often urgent.
    /// </summary>
    Medical,

    /// <summary>
    /// Valuable items - art, jewelry, electronics, currency.
    /// Base rate: $3.00/lb. Requires security clearance.
    /// </summary>
    HighValue,

    /// <summary>
    /// Delicate items - antiques, instruments, glassware.
    /// Base rate: $1.00/lb
    /// </summary>
    Fragile,

    /// <summary>
    /// Mail and postal service items.
    /// Base rate: $0.60/lb
    /// </summary>
    Mail,

    /// <summary>
    /// E-commerce packages and parcels.
    /// Base rate: $0.70/lb
    /// </summary>
    Parcels
}
