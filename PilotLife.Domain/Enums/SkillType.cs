namespace PilotLife.Domain.Enums;

/// <summary>
/// Types of skills that players can develop and level up.
/// </summary>
public enum SkillType
{
    /// <summary>
    /// Overall flying proficiency - leveled by flight hours and smooth landings.
    /// </summary>
    Piloting = 1,

    /// <summary>
    /// Route planning and efficiency - leveled by distance flown and on-time arrivals.
    /// </summary>
    Navigation = 2,

    /// <summary>
    /// Expertise in cargo transport jobs - leveled by completing cargo jobs.
    /// </summary>
    CargoHandling = 3,

    /// <summary>
    /// Expertise in passenger transport - leveled by completing passenger jobs.
    /// </summary>
    PassengerService = 4,

    /// <summary>
    /// Familiarity with different aircraft types - leveled by hours in different aircraft.
    /// </summary>
    AircraftKnowledge = 5,

    /// <summary>
    /// Competence in IFR and adverse weather conditions - leveled by IFR flights.
    /// </summary>
    WeatherFlying = 6,

    /// <summary>
    /// Competence in night operations - leveled by night flight hours.
    /// </summary>
    NightFlying = 7,

    /// <summary>
    /// High altitude and terrain operations - leveled by flights to mountain airports.
    /// </summary>
    MountainFlying = 8
}
