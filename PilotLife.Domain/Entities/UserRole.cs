using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Assigns a role to a user, optionally scoped to a specific world.
/// </summary>
public class UserRole : BaseEntity
{
    /// <summary>
    /// The user being assigned the role.
    /// </summary>
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// The role being assigned.
    /// </summary>
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    /// <summary>
    /// The world this role applies to.
    /// Null = global (applies to all worlds).
    /// </summary>
    public Guid? WorldId { get; set; }
    public World? World { get; set; }

    /// <summary>
    /// When the role was granted.
    /// </summary>
    public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// User who granted this role.
    /// </summary>
    public Guid GrantedByUserId { get; set; }
    public User GrantedByUser { get; set; } = null!;

    /// <summary>
    /// When the role expires (null = permanent).
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Whether the role assignment is currently active (not expired).
    /// </summary>
    public bool IsActive => ExpiresAt == null || ExpiresAt > DateTimeOffset.UtcNow;
}
