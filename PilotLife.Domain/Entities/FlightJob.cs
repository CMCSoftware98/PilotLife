using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Junction entity linking a tracked flight to a job.
/// A flight can complete multiple jobs, and tracks outcome per job.
/// </summary>
public class FlightJob : BaseEntity
{
    /// <summary>
    /// The tracked flight this job assignment belongs to.
    /// </summary>
    public Guid TrackedFlightId { get; set; }
    public TrackedFlight TrackedFlight { get; set; } = null!;

    /// <summary>
    /// The job being completed on this flight.
    /// </summary>
    public Guid JobId { get; set; }
    public Job Job { get; set; } = null!;

    /// <summary>
    /// Whether this job was successfully completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Whether this job failed (e.g., wrong destination, cargo damaged).
    /// </summary>
    public bool IsFailed { get; set; }

    /// <summary>
    /// Reason for failure, if applicable.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Actual payout for this job (may differ from base due to bonuses/penalties).
    /// </summary>
    public decimal? ActualPayout { get; set; }

    /// <summary>
    /// XP earned from this job.
    /// </summary>
    public int XpEarned { get; set; }

    /// <summary>
    /// Reputation change from this job (positive or negative).
    /// </summary>
    public int ReputationChange { get; set; }
}
