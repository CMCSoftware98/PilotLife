using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a single event that affected a player's reputation score.
/// </summary>
public class ReputationEvent : BaseEntity
{
    /// <summary>
    /// The player world this event belongs to.
    /// </summary>
    public Guid PlayerWorldId { get; set; }
    public PlayerWorld PlayerWorld { get; set; } = null!;

    /// <summary>
    /// Type of reputation event.
    /// </summary>
    public ReputationEventType EventType { get; set; }

    /// <summary>
    /// The change in reputation points (can be positive or negative).
    /// </summary>
    public decimal PointChange { get; set; }

    /// <summary>
    /// The player's reputation score after this event.
    /// </summary>
    public decimal ResultingScore { get; set; }

    /// <summary>
    /// Human-readable description of the event.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Related job ID if this event was job-related.
    /// </summary>
    public Guid? RelatedJobId { get; set; }
    public Job? RelatedJob { get; set; }

    /// <summary>
    /// Related flight ID if this event was flight-related.
    /// </summary>
    public Guid? RelatedFlightId { get; set; }
    public TrackedFlight? RelatedFlight { get; set; }

    /// <summary>
    /// When the event occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates a reputation event for a job completion.
    /// </summary>
    public static ReputationEvent CreateJobEvent(
        Guid playerWorldId,
        ReputationEventType eventType,
        decimal pointChange,
        decimal resultingScore,
        Guid jobId,
        string description)
    {
        return new ReputationEvent
        {
            PlayerWorldId = playerWorldId,
            EventType = eventType,
            PointChange = pointChange,
            ResultingScore = resultingScore,
            RelatedJobId = jobId,
            Description = description
        };
    }

    /// <summary>
    /// Creates a reputation event for a flight.
    /// </summary>
    public static ReputationEvent CreateFlightEvent(
        Guid playerWorldId,
        ReputationEventType eventType,
        decimal pointChange,
        decimal resultingScore,
        Guid flightId,
        string description)
    {
        return new ReputationEvent
        {
            PlayerWorldId = playerWorldId,
            EventType = eventType,
            PointChange = pointChange,
            ResultingScore = resultingScore,
            RelatedFlightId = flightId,
            Description = description
        };
    }
}
