using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Defines a role with associated permissions.
/// Roles can be global (apply to all worlds) or per-world.
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Display name of the role (e.g., "Admin", "Moderator").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the role's purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Priority/hierarchy level. Higher values = more authority.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Whether this is a system role that cannot be deleted.
    /// </summary>
    public bool IsSystemRole { get; set; }

    /// <summary>
    /// Whether this role applies globally to all worlds.
    /// </summary>
    public bool IsGlobal { get; set; }

    /// <summary>
    /// Permissions assigned to this role.
    /// </summary>
    public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();

    /// <summary>
    /// Users assigned to this role.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
