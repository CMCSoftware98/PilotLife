using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a community-submitted aircraft request awaiting approval.
/// </summary>
public class AircraftRequest : BaseEntity
{
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

    // Raw file contents
    public string? ManifestJsonRaw { get; set; }
    public string? AircraftCfgRaw { get; set; }

    // Parsed manifest.json fields
    public string? ManifestContentType { get; set; }
    public string? ManifestTitle { get; set; }
    public string? ManifestManufacturer { get; set; }
    public string? ManifestCreator { get; set; }
    public string? ManifestPackageVersion { get; set; }
    public string? ManifestMinimumGameVersion { get; set; }
    public string? ManifestTotalPackageSize { get; set; }
    public string? ManifestContentId { get; set; }

    // Parsed Aircraft.cfg [FLTSIM.0] fields
    public string? CfgTitle { get; set; }
    public string? CfgModel { get; set; }
    public string? CfgPanel { get; set; }
    public string? CfgSound { get; set; }
    public string? CfgTexture { get; set; }
    public string? CfgAtcType { get; set; }
    public string? CfgAtcModel { get; set; }
    public string? CfgAtcId { get; set; }
    public string? CfgAtcAirline { get; set; }
    public string? CfgUiManufacturer { get; set; }
    public string? CfgUiType { get; set; }
    public string? CfgUiVariation { get; set; }
    public string? CfgIcaoAirline { get; set; }

    // Parsed Aircraft.cfg [GENERAL] fields
    public string? CfgGeneralAtcType { get; set; }
    public string? CfgGeneralAtcModel { get; set; }
    public string? CfgEditable { get; set; }
    public string? CfgPerformance { get; set; }
    public string? CfgCategory { get; set; }

    public Guid RequestedByUserId { get; set; }
    public User? RequestedByUser { get; set; }

    public AircraftRequestStatus Status { get; set; } = AircraftRequestStatus.Pending;
    public string? ReviewNotes { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
}
