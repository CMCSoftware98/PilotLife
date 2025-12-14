namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents an airport. Uses int Id as this is reference data seeded from external sources.
/// </summary>
public class Airport
{
    public int Id { get; set; }
    public required string Ident { get; set; }
    public required string Name { get; set; }
    public string? IataCode { get; set; }
    public required string Type { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int? ElevationFt { get; set; }
    public string? Country { get; set; }
    public string? Municipality { get; set; }
}
