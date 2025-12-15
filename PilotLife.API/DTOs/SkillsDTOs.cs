namespace PilotLife.API.DTOs;

public class PlayerSkillResponse
{
    public string SkillId { get; set; } = string.Empty;
    public string SkillType { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CurrentXp { get; set; }
    public int Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public int XpForNextLevel { get; set; }
    public int XpForCurrentLevel { get; set; }
    public double ProgressToNextLevel { get; set; }
    public bool IsMaxLevel { get; set; }
}

public class SkillXpEventResponse
{
    public string Id { get; set; } = string.Empty;
    public string SkillType { get; set; } = string.Empty;
    public int XpGained { get; set; }
    public int ResultingXp { get; set; }
    public int ResultingLevel { get; set; }
    public bool CausedLevelUp { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OccurredAt { get; set; } = string.Empty;
    public string? RelatedFlightId { get; set; }
    public string? RelatedJobId { get; set; }
}
