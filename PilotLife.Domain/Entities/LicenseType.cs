using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Defines a type of license that players can earn.
/// License types are global and apply across all worlds (with world-specific cost multipliers).
/// </summary>
public class LicenseType : BaseEntity
{
    /// <summary>
    /// Unique code for the license (e.g., "PPL", "CPL", "IR", "TYPE_C172").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the license (e.g., "Private Pilot License").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the license and what it unlocks.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category of this license for grouping purposes.
    /// </summary>
    public LicenseCategory Category { get; set; }

    // Exam Configuration

    /// <summary>
    /// Base cost to take the exam (multiplied by world settings).
    /// </summary>
    public decimal BaseExamCost { get; set; }

    /// <summary>
    /// Time limit for the exam in minutes.
    /// </summary>
    public int ExamDurationMinutes { get; set; }

    /// <summary>
    /// Minimum score required to pass (0-100).
    /// </summary>
    public int PassingScore { get; set; } = 70;

    /// <summary>
    /// Required aircraft category for the exam (null = any).
    /// </summary>
    public AircraftCategory? RequiredAircraftCategory { get; set; }

    /// <summary>
    /// Specific aircraft type required (for type ratings).
    /// References Aircraft.TypeCode.
    /// </summary>
    public string? RequiredAircraftType { get; set; }

    // License Validity

    /// <summary>
    /// Validity period in game days. Null = permanent.
    /// </summary>
    public int? ValidityGameDays { get; set; }

    /// <summary>
    /// Base cost for renewal (multiplied by world settings).
    /// </summary>
    public decimal? BaseRenewalCost { get; set; }

    // Prerequisites

    /// <summary>
    /// JSON array of license codes that are prerequisites.
    /// </summary>
    public string? PrerequisiteLicensesJson { get; set; }

    /// <summary>
    /// Order for display in the license shop.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this license type is currently active and available.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// User licenses of this type.
    /// </summary>
    public ICollection<UserLicense> UserLicenses { get; set; } = new List<UserLicense>();

    /// <summary>
    /// Exams for this license type.
    /// </summary>
    public ICollection<LicenseExam> Exams { get; set; } = new List<LicenseExam>();
}
