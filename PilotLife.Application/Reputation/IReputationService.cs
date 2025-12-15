using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.Application.Reputation;

/// <summary>
/// Service interface for managing player reputation.
/// </summary>
public interface IReputationService
{
    /// <summary>
    /// Gets the current reputation status for a player.
    /// </summary>
    Task<ReputationStatus> GetReputationStatusAsync(Guid playerWorldId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a reputation event and updates the player's score.
    /// </summary>
    Task<ReputationResult> AddReputationEventAsync(
        Guid playerWorldId,
        ReputationEventType eventType,
        string? description = null,
        Guid? relatedJobId = null,
        Guid? relatedFlightId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the reputation history for a player.
    /// </summary>
    Task<IEnumerable<ReputationEvent>> GetReputationHistoryAsync(
        Guid playerWorldId,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a completed flight and updates reputation accordingly.
    /// </summary>
    Task ProcessFlightCompletedAsync(
        Guid playerWorldId,
        TrackedFlight flight,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a completed job and updates reputation accordingly.
    /// </summary>
    Task ProcessJobCompletedAsync(
        Guid playerWorldId,
        Job job,
        bool wasOnTime,
        bool wasEarly,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the payout bonus percentage based on reputation level.
    /// </summary>
    Task<decimal> GetPayoutBonusAsync(Guid playerWorldId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Current reputation status for a player.
/// </summary>
public class ReputationStatus
{
    public required Guid PlayerWorldId { get; set; }
    public required decimal Score { get; set; }
    public required int Level { get; set; }
    public required string LevelName { get; set; }
    public required decimal ProgressToNextLevel { get; set; }
    public required int OnTimeDeliveries { get; set; }
    public required int LateDeliveries { get; set; }
    public required int FailedDeliveries { get; set; }
    public required double JobCompletionRate { get; set; }
    public required double OnTimeRate { get; set; }
    public required decimal PayoutBonus { get; set; }
    public required List<ReputationBenefit> Benefits { get; set; }
}

/// <summary>
/// A benefit unlocked at a reputation level.
/// </summary>
public class ReputationBenefit
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required bool IsUnlocked { get; set; }
    public required int RequiredLevel { get; set; }
}

/// <summary>
/// Result of a reputation change operation.
/// </summary>
public class ReputationResult
{
    public required bool Success { get; set; }
    public required decimal NewScore { get; set; }
    public required decimal PointChange { get; set; }
    public required int NewLevel { get; set; }
    public required bool LevelChanged { get; set; }
    public string? Message { get; set; }
}
