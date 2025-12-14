using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a cargo/passenger transport job.
/// </summary>
public class Job : BaseEntity
{
    public int DepartureAirportId { get; set; }
    public Airport DepartureAirport { get; set; } = null!;
    public int ArrivalAirportId { get; set; }
    public Airport ArrivalAirport { get; set; } = null!;
    public required string CargoType { get; set; }
    public int Weight { get; set; }
    public decimal Payout { get; set; }
    public double DistanceNm { get; set; }
    public int EstimatedFlightTimeMinutes { get; set; }
    public required string RequiredAircraftType { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsCompleted { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
}
