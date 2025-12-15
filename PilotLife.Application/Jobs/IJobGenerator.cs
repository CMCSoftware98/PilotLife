namespace PilotLife.Application.Jobs;

/// <summary>
/// Interface for generating and populating jobs across airports.
/// </summary>
public interface IJobGenerator
{
    /// <summary>
    /// Populates jobs for all airports in a specific world.
    /// Creates jobs based on airport size and cargo type distribution.
    /// </summary>
    /// <param name="worldId">The world to populate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PopulateWorldJobsAsync(Guid worldId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes jobs at airports that have fallen below the minimum threshold.
    /// Also cleans up expired jobs.
    /// </summary>
    /// <param name="worldId">The world to refresh.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RefreshStaleJobsAsync(Guid worldId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up expired jobs across all worlds or a specific world.
    /// </summary>
    /// <param name="worldId">Optional world to clean up (null for all worlds).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of jobs cleaned up.</returns>
    Task<int> CleanupExpiredJobsAsync(Guid? worldId = null, CancellationToken cancellationToken = default);
}
