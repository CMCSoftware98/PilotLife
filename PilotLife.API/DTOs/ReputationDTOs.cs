namespace PilotLife.API.DTOs;

public class ReputationStatusResponse
{
    public string PlayerWorldId { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public int Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public decimal ProgressToNextLevel { get; set; }
    public int OnTimeDeliveries { get; set; }
    public int LateDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
    public double JobCompletionRate { get; set; }
    public double OnTimeRate { get; set; }
    public decimal PayoutBonus { get; set; }
    public List<ReputationBenefitResponse> Benefits { get; set; } = new();
}

public class ReputationBenefitResponse
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; }
    public int RequiredLevel { get; set; }
}

public class ReputationEventResponse
{
    public string Id { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public decimal PointChange { get; set; }
    public decimal ResultingScore { get; set; }
    public string Description { get; set; } = string.Empty;
    public string OccurredAt { get; set; } = string.Empty;
    public string? RelatedJobId { get; set; }
    public string? RelatedFlightId { get; set; }
}
