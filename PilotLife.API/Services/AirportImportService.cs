using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;

namespace PilotLife.API.Services;

public class AirportImportService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AirportImportService> _logger;
    private readonly IWebHostEnvironment _environment;

    public AirportImportService(
        IServiceProvider serviceProvider,
        ILogger<AirportImportService> logger,
        IWebHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _environment = environment;
    }

    public async Task ImportAirportsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();

        var airportCount = await context.Airports.CountAsync();
        if (airportCount > 0)
        {
            _logger.LogInformation("Airports table already has {Count} records, skipping import", airportCount);
            return;
        }

        var csvPath = Path.Combine(_environment.ContentRootPath, "..", "pilotlife-app", "data", "airports.csv");

        if (!File.Exists(csvPath))
        {
            _logger.LogWarning("Airports CSV file not found at {Path}", csvPath);
            return;
        }

        _logger.LogInformation("Starting airport import from {Path}", csvPath);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null
        };

        // In development, only import large airports for faster startup
        var validTypes = _environment.IsDevelopment()
            ? new HashSet<string> { "large_airport" }
            : new HashSet<string> { "large_airport", "medium_airport", "small_airport" };

        if (_environment.IsDevelopment())
        {
            _logger.LogInformation("Development mode: importing only large airports for faster startup");
        }
        var airports = new List<Airport>();

        using (var reader = new StreamReader(csvPath))
        using (var csv = new CsvReader(reader, config))
        {
            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var type = csv.GetField("type");
                if (type == null || !validTypes.Contains(type))
                    continue;

                var ident = csv.GetField("ident");
                var name = csv.GetField("name");

                if (string.IsNullOrWhiteSpace(ident) || string.IsNullOrWhiteSpace(name))
                    continue;

                var latStr = csv.GetField("latitude_deg");
                var lonStr = csv.GetField("longitude_deg");

                if (!double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude) ||
                    !double.TryParse(lonStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude))
                    continue;

                int? elevationFt = null;
                var elevStr = csv.GetField("elevation_ft");
                if (!string.IsNullOrWhiteSpace(elevStr) && int.TryParse(elevStr, out var elev))
                    elevationFt = elev;

                var iataCode = csv.GetField("iata_code");
                if (string.IsNullOrWhiteSpace(iataCode))
                    iataCode = null;

                var country = csv.GetField("iso_country");
                if (string.IsNullOrWhiteSpace(country))
                    country = null;

                var municipality = csv.GetField("municipality");
                if (string.IsNullOrWhiteSpace(municipality))
                    municipality = null;

                airports.Add(new Airport
                {
                    Ident = ident,
                    Name = name,
                    IataCode = iataCode,
                    Type = type,
                    Latitude = latitude,
                    Longitude = longitude,
                    ElevationFt = elevationFt,
                    Country = country,
                    Municipality = municipality
                });
            }
        }

        _logger.LogInformation("Parsed {Count} airports from CSV, importing to database...", airports.Count);

        // Batch insert for performance
        const int batchSize = 1000;
        for (var i = 0; i < airports.Count; i += batchSize)
        {
            var batch = airports.Skip(i).Take(batchSize);
            context.Airports.AddRange(batch);
            await context.SaveChangesAsync();

            if ((i + batchSize) % 10000 == 0 || i + batchSize >= airports.Count)
            {
                _logger.LogInformation("Imported {Count}/{Total} airports",
                    Math.Min(i + batchSize, airports.Count), airports.Count);
            }
        }

        _logger.LogInformation("Airport import completed. Total airports: {Count}", airports.Count);
    }
}
