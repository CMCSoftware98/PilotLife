using Microsoft.EntityFrameworkCore;
using PilotLife.Application.Maintenance;
using PilotLife.Database.Data;

namespace PilotLife.API.Services.Maintenance;

/// <summary>
/// Background service that monitors and completes scheduled maintenance jobs
/// when their estimated duration has elapsed.
/// </summary>
public class MaintenanceCompletionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MaintenanceCompletionService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public MaintenanceCompletionService(
        IServiceProvider serviceProvider,
        ILogger<MaintenanceCompletionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Maintenance Completion Service starting");

        // Initial delay to let the application start up
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndCompleteMaintenanceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Maintenance Completion Service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Maintenance Completion Service stopping");
    }

    private async Task CheckAndCompleteMaintenanceAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();
        var maintenanceService = scope.ServiceProvider.GetRequiredService<IMaintenanceService>();

        // Find all incomplete maintenance jobs that should be done by now
        var now = DateTimeOffset.UtcNow;
        var completedJobs = await context.MaintenanceLogs
            .Where(m => !m.IsCompleted)
            .ToListAsync(cancellationToken);

        var jobsToComplete = completedJobs
            .Where(m => m.StartedAt.AddHours(m.EstimatedDurationHours) <= now)
            .ToList();

        if (jobsToComplete.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Found {Count} maintenance jobs ready for completion", jobsToComplete.Count);

        foreach (var job in jobsToComplete)
        {
            try
            {
                var result = await maintenanceService.CompleteMaintenanceAsync(job.Id, cancellationToken);

                if (result.Success)
                {
                    _logger.LogInformation("Completed maintenance {MaintenanceId} ({Type}) for aircraft {AircraftId}",
                        job.Id, job.MaintenanceType, job.OwnedAircraftId);
                }
                else
                {
                    _logger.LogWarning("Failed to complete maintenance {MaintenanceId}: {Message}",
                        job.Id, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing maintenance {MaintenanceId}", job.Id);
            }
        }
    }
}
