namespace PilotLife.Domain.Enums;

/// <summary>
/// Status of a job through its lifecycle.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// Job is available for acceptance.
    /// </summary>
    Available,

    /// <summary>
    /// Job has been accepted by a player.
    /// </summary>
    Accepted,

    /// <summary>
    /// Flight is in progress for this job.
    /// </summary>
    InProgress,

    /// <summary>
    /// Job was completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Job failed (wrong destination, cargo damaged, etc.).
    /// </summary>
    Failed,

    /// <summary>
    /// Job was cancelled by the player.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Job expired before being accepted.
    /// </summary>
    Expired
}
