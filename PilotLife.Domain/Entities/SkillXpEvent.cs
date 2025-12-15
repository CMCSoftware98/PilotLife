using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents an XP gain event for a player skill.
/// </summary>
public class SkillXpEvent : BaseEntity
{
    /// <summary>
    /// The player skill this event belongs to.
    /// </summary>
    public Guid PlayerSkillId { get; set; }
    public PlayerSkill PlayerSkill { get; set; } = null!;

    /// <summary>
    /// Amount of XP gained.
    /// </summary>
    public int XpGained { get; set; }

    /// <summary>
    /// Total XP after this event.
    /// </summary>
    public int ResultingXp { get; set; }

    /// <summary>
    /// Level after this event.
    /// </summary>
    public int ResultingLevel { get; set; }

    /// <summary>
    /// Whether this event caused a level up.
    /// </summary>
    public bool CausedLevelUp { get; set; }

    /// <summary>
    /// Source of the XP (e.g., "Flight completed", "Smooth landing", "Cargo job").
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Optional detailed description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Related flight ID if this XP came from a flight.
    /// </summary>
    public Guid? RelatedFlightId { get; set; }
    public TrackedFlight? RelatedFlight { get; set; }

    /// <summary>
    /// Related job ID if this XP came from a job.
    /// </summary>
    public Guid? RelatedJobId { get; set; }
    public Job? RelatedJob { get; set; }

    /// <summary>
    /// When the XP was gained.
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates an XP event for a flight.
    /// </summary>
    public static SkillXpEvent CreateFromFlight(
        Guid playerSkillId,
        int xpGained,
        int resultingXp,
        int resultingLevel,
        bool causedLevelUp,
        Guid flightId,
        string source,
        string? description = null)
    {
        return new SkillXpEvent
        {
            PlayerSkillId = playerSkillId,
            XpGained = xpGained,
            ResultingXp = resultingXp,
            ResultingLevel = resultingLevel,
            CausedLevelUp = causedLevelUp,
            RelatedFlightId = flightId,
            Source = source,
            Description = description
        };
    }

    /// <summary>
    /// Creates an XP event for a job.
    /// </summary>
    public static SkillXpEvent CreateFromJob(
        Guid playerSkillId,
        int xpGained,
        int resultingXp,
        int resultingLevel,
        bool causedLevelUp,
        Guid jobId,
        string source,
        string? description = null)
    {
        return new SkillXpEvent
        {
            PlayerSkillId = playerSkillId,
            XpGained = xpGained,
            ResultingXp = resultingXp,
            ResultingLevel = resultingLevel,
            CausedLevelUp = causedLevelUp,
            RelatedJobId = jobId,
            Source = source,
            Description = description
        };
    }
}
