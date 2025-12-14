namespace PilotLife.Domain.Enums;

/// <summary>
/// Types of modifications that can be applied to aircraft.
/// </summary>
public enum ModificationType
{
    /// <summary>
    /// 25% cargo conversion - removes some passenger seats for cargo space.
    /// </summary>
    CargoConversion25 = 1,

    /// <summary>
    /// 50% cargo conversion - half passenger, half cargo configuration.
    /// </summary>
    CargoConversion50 = 2,

    /// <summary>
    /// 75% cargo conversion - mostly cargo with minimal passenger space.
    /// </summary>
    CargoConversion75 = 3,

    /// <summary>
    /// 100% cargo conversion - full freighter configuration.
    /// </summary>
    CargoConversion100 = 4,

    /// <summary>
    /// VIP/Executive interior upgrade.
    /// </summary>
    VipInterior = 10,

    /// <summary>
    /// Extended range fuel tanks.
    /// </summary>
    ExtendedRangeTanks = 11,

    /// <summary>
    /// STOL (Short Takeoff and Landing) kit.
    /// </summary>
    StolKit = 12,

    /// <summary>
    /// Float conversion for water operations.
    /// </summary>
    FloatConversion = 13,

    /// <summary>
    /// Ski conversion for snow/ice operations.
    /// </summary>
    SkiConversion = 14,

    /// <summary>
    /// Avionics upgrade package.
    /// </summary>
    AvionicsUpgrade = 20,

    /// <summary>
    /// Engine performance upgrade.
    /// </summary>
    EngineUpgrade = 21,

    /// <summary>
    /// Winglets for improved fuel efficiency.
    /// </summary>
    Winglets = 22,

    /// <summary>
    /// Noise reduction modifications.
    /// </summary>
    NoiseReduction = 23,

    /// <summary>
    /// Weather radar installation.
    /// </summary>
    WeatherRadar = 24,

    /// <summary>
    /// De-icing system installation.
    /// </summary>
    DeIcingSystem = 25
}
