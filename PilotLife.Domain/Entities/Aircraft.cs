using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents an aircraft template/definition in the system.
/// This is master data for available aircraft types.
/// </summary>
public class Aircraft : BaseEntity
{
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
}
