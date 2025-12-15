using Microsoft.Extensions.Options;
using PilotLife.Application.Marketplace;
using PilotLife.Database.Data;
using Microsoft.EntityFrameworkCore;

namespace PilotLife.API.Services;

/// <summary>
/// Background service that periodically populates and refreshes the marketplace.
/// </summary>
public class MarketplacePopulationService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MarketplacePopulationService> _logger;
    private readonly MarketplaceConfiguration _config;
    private Timer? _timer;
    private bool _isRunning;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public MarketplacePopulationService(
        IServiceProvider serviceProvider,
        ILogger<MarketplacePopulationService> logger,
        IOptions<MarketplaceConfiguration> config)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _config = config.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marketplace Population Service starting. Interval: {Hours} hours",
            _config.IntervalHours);

        // Run immediately on startup, then on interval
        _timer = new Timer(
            DoWork,
            null,
            TimeSpan.FromMinutes(1), // Small delay to let app start up
            TimeSpan.FromHours(_config.IntervalHours));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marketplace Population Service stopping");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        // Prevent concurrent runs
        if (!await _semaphore.WaitAsync(0))
        {
            _logger.LogInformation("Marketplace population already running, skipping this interval");
            return;
        }

        try
        {
            _isRunning = true;
            await DoWorkAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during marketplace population");
        }
        finally
        {
            _isRunning = false;
            _semaphore.Release();
        }
    }

    private async Task DoWorkAsync()
    {
        _logger.LogInformation("Starting marketplace population run at {Time}", DateTimeOffset.UtcNow);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();
        var generator = scope.ServiceProvider.GetRequiredService<IMarketplaceGenerator>();

        // Get all active worlds
        var worlds = await context.Worlds
            .Where(w => w.IsActive)
            .Select(w => w.Id)
            .ToListAsync();

        _logger.LogInformation("Found {Count} active worlds to populate", worlds.Count);

        foreach (var worldId in worlds)
        {
            try
            {
                // Check if this world needs initial population or just refresh
                var dealerCount = await context.AircraftDealers
                    .CountAsync(d => d.WorldId == worldId);

                if (dealerCount == 0)
                {
                    _logger.LogInformation("World {WorldId} has no dealers, performing full population", worldId);
                    await generator.PopulateWorldMarketplaceAsync(worldId);
                }
                else
                {
                    _logger.LogInformation("World {WorldId} has {Count} dealers, refreshing stale inventory", worldId, dealerCount);
                    await generator.RefreshStaleInventoryAsync(worldId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating marketplace for world {WorldId}", worldId);
            }
        }

        _logger.LogInformation("Marketplace population run completed at {Time}", DateTimeOffset.UtcNow);
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _semaphore.Dispose();
    }
}
