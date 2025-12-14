namespace PilotLife.Domain.Enums;

/// <summary>
/// Types of aircraft dealers with different inventory characteristics.
/// </summary>
public enum DealerType
{
    /// <summary>
    /// New aircraft only, full warranty, MSRP pricing.
    /// Condition: 100%, Hours: 0
    /// </summary>
    ManufacturerShowroom,

    /// <summary>
    /// Pre-owned aircraft in good condition with limited warranty.
    /// Condition: 80-95%, Hours: 500-3000
    /// </summary>
    CertifiedPreOwned,

    /// <summary>
    /// Mix of new and used regional aircraft.
    /// Condition: 70-100%, Hours: 0-8000
    /// </summary>
    RegionalDealer,

    /// <summary>
    /// Lower condition aircraft at steep discounts, sold as-is.
    /// Condition: 60-80%, Hours: 3000-15000+
    /// </summary>
    BudgetLot,

    /// <summary>
    /// Specialty aircraft: bush planes, floatplanes, warbirds.
    /// Condition: 65-95%, Hours: varies
    /// </summary>
    SpecialtyDealer,

    /// <summary>
    /// Business jets and high-end turboprops.
    /// Condition: 85-100%, Hours: 0-5000
    /// </summary>
    ExecutiveDealer,

    /// <summary>
    /// Freighter aircraft and cargo conversions.
    /// Condition: 70-90%, Hours: 2000-12000
    /// </summary>
    CargoSpecialist,

    /// <summary>
    /// Training aircraft, affordable starters.
    /// Condition: 75-95%, Hours: 1000-8000
    /// </summary>
    FlightSchool
}
