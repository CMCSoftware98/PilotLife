namespace PilotLife.Database.Entities;

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
