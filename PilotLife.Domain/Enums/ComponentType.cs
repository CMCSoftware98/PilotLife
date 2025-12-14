namespace PilotLife.Domain.Enums;

/// <summary>
/// Types of aircraft components that can be tracked for condition and maintenance.
/// </summary>
public enum ComponentType
{
    /// <summary>
    /// Engine 1 (typically left or single engine).
    /// </summary>
    Engine1 = 1,

    /// <summary>
    /// Engine 2 (typically right engine on twins).
    /// </summary>
    Engine2 = 2,

    /// <summary>
    /// Engine 3 (for 3+ engine aircraft).
    /// </summary>
    Engine3 = 3,

    /// <summary>
    /// Engine 4 (for 4+ engine aircraft).
    /// </summary>
    Engine4 = 4,

    /// <summary>
    /// Engine 5 (rare, for large aircraft).
    /// </summary>
    Engine5 = 5,

    /// <summary>
    /// Engine 6 (rare, for large aircraft).
    /// </summary>
    Engine6 = 6,

    /// <summary>
    /// Wing structure including control surfaces.
    /// </summary>
    Wings = 10,

    /// <summary>
    /// Landing gear system.
    /// </summary>
    LandingGear = 11,

    /// <summary>
    /// Fuselage and airframe structure.
    /// </summary>
    Fuselage = 12,

    /// <summary>
    /// Avionics and electrical systems.
    /// </summary>
    Avionics = 13,

    /// <summary>
    /// Propeller(s) - for prop aircraft.
    /// </summary>
    Propeller = 14,

    /// <summary>
    /// Interior cabin and seats.
    /// </summary>
    Interior = 15,

    /// <summary>
    /// Hydraulic systems.
    /// </summary>
    Hydraulics = 16,

    /// <summary>
    /// Fuel system including tanks.
    /// </summary>
    FuelSystem = 17
}
