namespace PilotLife.Domain.Enums;

/// <summary>
/// Category of aircraft for exam and license requirements.
/// </summary>
public enum AircraftCategory
{
    /// <summary>
    /// Single Engine Piston aircraft.
    /// </summary>
    SEP,

    /// <summary>
    /// Multi Engine Piston aircraft.
    /// </summary>
    MEP,

    /// <summary>
    /// Turboprop aircraft.
    /// </summary>
    Turboprop,

    /// <summary>
    /// Regional jet aircraft.
    /// </summary>
    RegionalJet,

    /// <summary>
    /// Narrow body jet aircraft.
    /// </summary>
    NarrowBody,

    /// <summary>
    /// Wide body jet aircraft.
    /// </summary>
    WideBody,

    /// <summary>
    /// Helicopter/rotorcraft.
    /// </summary>
    Helicopter
}
