using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Maps a permission to a role.
/// This is a junction table with additional grant/deny flag.
/// </summary>
public class RolePermission
{
    /// <summary>
    /// The role this permission belongs to.
    /// </summary>
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    /// <summary>
    /// The permission being assigned.
    /// </summary>
    public PermissionCategory Permission { get; set; }

    /// <summary>
    /// Whether the permission is granted (true) or explicitly denied (false).
    /// </summary>
    public bool IsGranted { get; set; } = true;
}
