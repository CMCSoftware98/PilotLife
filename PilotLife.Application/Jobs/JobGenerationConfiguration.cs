namespace PilotLife.Application.Jobs;

/// <summary>
/// Configuration options for job generation and population.
/// </summary>
public class JobGenerationConfiguration
{
    public const string SectionName = "JobGeneration";

    /// <summary>
    /// Interval in hours between job generation runs.
    /// </summary>
    public double IntervalHours { get; set; } = 1.0;

    /// <summary>
    /// Number of jobs to generate at large airports.
    /// </summary>
    public int JobsPerLargeAirport { get; set; } = 25;

    /// <summary>
    /// Number of jobs to generate at medium airports.
    /// </summary>
    public int JobsPerMediumAirport { get; set; } = 15;

    /// <summary>
    /// Number of jobs to generate at small airports.
    /// </summary>
    public int JobsPerSmallAirport { get; set; } = 8;

    /// <summary>
    /// Minimum jobs to maintain at any airport (triggers regeneration).
    /// </summary>
    public int MinJobsPerAirport { get; set; } = 3;

    /// <summary>
    /// Base expiry hours for standard urgency jobs.
    /// </summary>
    public int BaseExpiryHours { get; set; } = 48;

    /// <summary>
    /// Percentage of cargo jobs vs passenger jobs (0.0 to 1.0).
    /// </summary>
    public double CargoJobPercentage { get; set; } = 0.70;

    /// <summary>
    /// Batch size for processing airports.
    /// </summary>
    public int AirportBatchSize { get; set; } = 500;

    /// <summary>
    /// How often to log progress (number of airports).
    /// </summary>
    public int ProgressLogInterval { get; set; } = 1000;

    /// <summary>
    /// Maximum distance in nautical miles for job routes.
    /// </summary>
    public int MaxRouteDistanceNm { get; set; } = 3000;

    /// <summary>
    /// Minimum distance in nautical miles for job routes.
    /// </summary>
    public int MinRouteDistanceNm { get; set; } = 20;

    /// <summary>
    /// Whether to only generate jobs at airports with runways (exclude heliports, etc.).
    /// </summary>
    public bool OnlyAirportsWithRunways { get; set; } = true;

    /// <summary>
    /// Maximum degree of parallelism for job generation.
    /// Defaults to half the processor count for conservative resource usage.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = Math.Max(1, Environment.ProcessorCount / 2);

    /// <summary>
    /// Number of airports to process before saving to database in parallel mode.
    /// </summary>
    public int ParallelSaveBatchSize { get; set; } = 50;

    /// <summary>
    /// If true, only generate jobs for airports within DevCenterRadiusNm of DevCenterAirportIcao.
    /// Useful for development to limit the scope of job generation.
    /// </summary>
    public bool DevModeEnabled { get; set; } = false;

    /// <summary>
    /// The ICAO code of the center airport for development mode (e.g., "EGLL").
    /// Only used when DevModeEnabled is true.
    /// </summary>
    public string DevCenterAirportIcao { get; set; } = "EGLL";

    /// <summary>
    /// The radius in nautical miles around the center airport for development mode.
    /// Only airports within this radius will have jobs generated.
    /// </summary>
    public int DevCenterRadiusNm { get; set; } = 200;
}
