namespace PilotLife.Database.Entities;

public enum AircraftRequestStatus
{
    Pending,
    Approved,
    Rejected
}

public class AircraftRequest
{
    public Guid Id { get; set; }
    public required string AircraftTitle { get; set; }
    public string? AtcType { get; set; }
    public string? AtcModel { get; set; }
    public string? Category { get; set; }
    public int EngineType { get; set; }
    public string? EngineTypeStr { get; set; }
    public int NumberOfEngines { get; set; }
    public double MaxGrossWeightLbs { get; set; }
    public double EmptyWeightLbs { get; set; }
    public double CruiseSpeedKts { get; set; }
    public string? SimulatorVersion { get; set; }

    public Guid RequestedByUserId { get; set; }
    public User? RequestedByUser { get; set; }

    public AircraftRequestStatus Status { get; set; }
    public string? ReviewNotes { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
