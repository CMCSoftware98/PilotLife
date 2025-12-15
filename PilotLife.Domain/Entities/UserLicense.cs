using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a license held by a player in a specific world.
/// Tracks validity, renewal history, and exam attempts.
/// </summary>
public class UserLicense : BaseEntity
{
    /// <summary>
    /// The player world this license belongs to.
    /// </summary>
    public Guid PlayerWorldId { get; set; }
    public PlayerWorld PlayerWorld { get; set; } = null!;

    /// <summary>
    /// The type of license.
    /// </summary>
    public Guid LicenseTypeId { get; set; }
    public LicenseType LicenseType { get; set; } = null!;

    /// <summary>
    /// When this license was first earned.
    /// </summary>
    public DateTimeOffset EarnedAt { get; set; }

    /// <summary>
    /// When this license expires. Null = permanent license.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// When this license was last renewed.
    /// </summary>
    public DateTimeOffset? LastRenewedAt { get; set; }

    /// <summary>
    /// Number of times this license has been renewed.
    /// </summary>
    public int RenewalCount { get; set; }

    /// <summary>
    /// Whether this license is currently valid.
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Whether this license has been revoked (e.g., due to violations).
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Reason for revocation if applicable.
    /// </summary>
    public string? RevocationReason { get; set; }

    /// <summary>
    /// When the license was revoked.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Score achieved on the exam (0-100).
    /// </summary>
    public int ExamScore { get; set; }

    /// <summary>
    /// Number of exam attempts before passing.
    /// </summary>
    public int ExamAttempts { get; set; }

    /// <summary>
    /// Total amount paid for this license (exam + renewals).
    /// </summary>
    public decimal TotalPaid { get; set; }

    /// <summary>
    /// The exam that was passed to earn this license.
    /// </summary>
    public Guid? PassedExamId { get; set; }
    public LicenseExam? PassedExam { get; set; }
}
