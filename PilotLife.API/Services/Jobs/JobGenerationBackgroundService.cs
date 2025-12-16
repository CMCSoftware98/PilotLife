using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PilotLife.Application.Jobs;
using PilotLife.Database.Data;

namespace PilotLife.API.Services.Jobs;

/// <summary>
/// Background service that periodically populates and refreshes jobs across all airports.
/// </summary>
public class JobGenerationBackgroundService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobGenerationBackgroundService> _logger;
    private readonly JobGenerationConfiguration _config;
    private Timer? _timer;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public JobGenerationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<JobGenerationBackgroundService> logger,
        IOptions<JobGenerationConfiguration> config)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _config = config.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Job Generation Service starting. Interval: {Hours} hours",
            _config.IntervalHours);

        // Run immediately on startup (after 1 minute delay), then on interval
        _timer = new Timer(
            DoWork,
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromHours(_config.IntervalHours));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Job Generation Service stopping");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        // Prevent concurrent runs
        if (!await _semaphore.WaitAsync(0))
        {
            _logger.LogInformation("Job generation already running, skipping this interval");
            return;
        }

        try
        {
            await DoWorkAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during job generation");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task DoWorkAsync()
    {
        _logger.LogInformation("Starting job generation run at {Time}", DateTimeOffset.UtcNow);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();
        var generator = scope.ServiceProvider.GetRequiredService<IJobGenerator>();

        // First, cleanup expired jobs
        var cleanedUp = await generator.CleanupExpiredJobsAsync();
        if (cleanedUp > 0)
        {
            _logger.LogInformation("Cleaned up {Count} expired jobs", cleanedUp);
        }

        // Get all active worlds
        var worlds = await context.Worlds
            .Where(w => w.IsActive)
            .Select(w => w.Id)
            .ToListAsync();

        _logger.LogInformation("Found {Count} active worlds to process", worlds.Count);

        foreach (var worldId in worlds)
        {
            try
            {
                // Check if this world has any jobs at all
                var jobCount = await context.Jobs
                    .CountAsync(j => j.WorldId == worldId);

                if (jobCount == 0)
                {
                    _logger.LogInformation("World {WorldId} has no jobs, performing full population", worldId);
                    await generator.PopulateWorldJobsAsync(worldId);
                    _logger.LogInformation("World {WorldId} full population finished", worldId);
                }
                else
                {
                    _logger.LogInformation("World {WorldId} has {Count} jobs, refreshing stale airports", worldId, jobCount);
                    await generator.RefreshStaleJobsAsync(worldId);
                    _logger.LogInformation("World {WorldId} refresh finished", worldId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating jobs for world {WorldId}", worldId);
            }
        }

        _logger.LogInformation("=== Job generation run COMPLETED at {Time} ===", DateTimeOffset.UtcNow);
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _semaphore.Dispose();
    }
}
