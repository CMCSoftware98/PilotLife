namespace PilotLife.Application.Marketplace;

/// <summary>
/// Interface for generating and populating marketplace dealers and inventory.
/// </summary>
public interface IMarketplaceGenerator
{
    /// <summary>
    /// Populates the marketplace for a specific world.
    /// Creates dealers at airports and generates inventory based on airport size and dealer type.
    /// </summary>
    /// <param name="worldId">The world to populate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PopulateWorldMarketplaceAsync(Guid worldId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes inventory for dealers that need it (based on their refresh interval).
    /// </summary>
    /// <param name="worldId">The world to refresh.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RefreshStaleInventoryAsync(Guid worldId, CancellationToken cancellationToken = default);
}
