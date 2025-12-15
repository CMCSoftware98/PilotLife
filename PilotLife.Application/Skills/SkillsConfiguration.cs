namespace PilotLife.Application.Skills;

/// <summary>
/// Configuration options for the skills system.
/// </summary>
public class SkillsConfiguration
{
    public const string SectionName = "Skills";

    // XP Sources - Flight
    /// <summary>
    /// XP gained per flight hour for Piloting skill.
    /// </summary>
    public int XpPerFlightHour { get; set; } = 10;

    /// <summary>
    /// XP gained per 10 nautical miles flown for Navigation skill.
    /// </summary>
    public double XpPerTenNm { get; set; } = 1;

    /// <summary>
    /// XP bonus for a smooth landing (< 100 fpm).
    /// </summary>
    public int XpSmoothLanding { get; set; } = 5;

    /// <summary>
    /// XP bonus for a good landing (100-200 fpm).
    /// </summary>
    public int XpGoodLanding { get; set; } = 2;

    // XP Sources - Jobs
    /// <summary>
    /// XP gained for completing a cargo job.
    /// </summary>
    public int XpPerCargoJob { get; set; } = 25;

    /// <summary>
    /// XP gained for completing a passenger job.
    /// </summary>
    public int XpPerPassengerJob { get; set; } = 25;

    /// <summary>
    /// Bonus XP for completing a high-risk job.
    /// </summary>
    public int XpHighRiskJobBonus { get; set; } = 15;

    /// <summary>
    /// Bonus XP for completing a VIP job.
    /// </summary>
    public int XpVipJobBonus { get; set; } = 20;

    // XP Sources - Special Flight Conditions
    /// <summary>
    /// Bonus XP per hour for night flight.
    /// </summary>
    public int XpNightFlightHourBonus { get; set; } = 5;

    /// <summary>
    /// Bonus XP per hour for IFR flight.
    /// </summary>
    public int XpIfrFlightHourBonus { get; set; } = 5;

    /// <summary>
    /// Bonus XP for landing at a mountain airport.
    /// </summary>
    public int XpMountainLandingBonus { get; set; } = 10;

    // XP Sources - Aircraft
    /// <summary>
    /// XP for first flight in a new aircraft type.
    /// </summary>
    public int XpNewAircraftTypeBonus { get; set; } = 20;

    // Level Thresholds
    /// <summary>
    /// XP thresholds for each level (index = level - 1).
    /// Level 1: 0 XP, Level 2: 100 XP, etc.
    /// </summary>
    public int[] LevelThresholds { get; set; } = [0, 100, 300, 600, 1000, 1500, 2500, 4000];

    /// <summary>
    /// Gets the XP required for a specific level.
    /// </summary>
    public int GetXpForLevel(int level)
    {
        if (level < 1) return 0;
        if (level > LevelThresholds.Length) return int.MaxValue;
        return LevelThresholds[level - 1];
    }

    /// <summary>
    /// Gets the level for a given XP amount.
    /// </summary>
    public int GetLevelForXp(int xp)
    {
        for (int i = LevelThresholds.Length - 1; i >= 0; i--)
        {
            if (xp >= LevelThresholds[i])
                return i + 1;
        }
        return 1;
    }

    /// <summary>
    /// Maximum skill level.
    /// </summary>
    public int MaxLevel => LevelThresholds.Length;
}
