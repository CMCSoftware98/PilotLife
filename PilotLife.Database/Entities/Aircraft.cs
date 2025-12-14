namespace PilotLife.Database.Entities;

public class Aircraft
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
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
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
