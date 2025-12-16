namespace PilotLife.Application.Marketplace;

/// <summary>
/// Configuration options for marketplace population.
/// </summary>
public class MarketplaceConfiguration
{
    public const string SectionName = "MarketplacePopulation";

    /// <summary>
    /// Interval in hours between marketplace population runs.
    /// </summary>
    public double IntervalHours { get; set; } = 3.5;

    /// <summary>
    /// Number of dealers to create at large airports.
    /// </summary>
    public int DealersPerLargeAirport { get; set; } = 3;

    /// <summary>
    /// Number of dealers to create at medium airports.
    /// </summary>
    public int DealersPerMediumAirport { get; set; } = 2;

    /// <summary>
    /// Number of dealers to create at small airports.
    /// </summary>
    public int DealersPerSmallAirport { get; set; } = 1;

    /// <summary>
    /// Minimum inventory items per dealer.
    /// </summary>
    public int MinInventoryPerDealer { get; set; } = 3;

    /// <summary>
    /// Maximum inventory items per dealer.
    /// </summary>
    public int MaxInventoryPerDealer { get; set; } = 15;

    /// <summary>
    /// Number of days before inventory listings expire.
    /// </summary>
    public int InventoryExpirationDays { get; set; } = 14;

    /// <summary>
    /// Batch size for processing airports.
    /// </summary>
    public int AirportBatchSize { get; set; } = 500;

    /// <summary>
    /// How often to log progress (number of airports).
    /// </summary>
    public int ProgressLogInterval { get; set; } = 1000;

    /// <summary>
    /// Number of parallel batch tasks for marketplace population.
    /// Defaults to half the processor count for conservative resource usage.
    /// </summary>
    public int ParallelBatchCount { get; set; } = Math.Max(1, Environment.ProcessorCount / 2);

    /// <summary>
    /// If true, only generate marketplace listings for airports within DevCenterRadiusNm of DevCenterAirportIcao.
    /// Useful for development to limit the scope of marketplace generation.
    /// </summary>
    public bool DevModeEnabled { get; set; } = false;

    /// <summary>
    /// The ICAO code of the center airport for development mode (e.g., "EGLL").
    /// Only used when DevModeEnabled is true.
    /// </summary>
    public string DevCenterAirportIcao { get; set; } = "EGLL";

    /// <summary>
    /// The radius in nautical miles around the center airport for development mode.
    /// Only airports within this radius will have marketplace listings generated.
    /// </summary>
    public int DevCenterRadiusNm { get; set; } = 200;
}
