namespace PilotLife.Database.Entities;

public class Job
{
    public Guid Id { get; set; }
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
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsCompleted { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
}
