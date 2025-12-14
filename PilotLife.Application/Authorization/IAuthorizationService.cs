using PilotLife.Domain.Enums;

namespace PilotLife.Application.Authorization;

/// <summary>
/// Service for checking user permissions and authorization.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Checks if a user has a specific permission.
    /// Considers role hierarchy and world scope.
    /// </summary>
    /// <param name="userId">The user to check.</param>
    /// <param name="permission">The permission to check for.</param>
    /// <param name="worldId">Optional world scope (null = global check).</param>
    /// <returns>True if the user has the permission.</returns>
    Task<bool> HasPermissionAsync(Guid userId, PermissionCategory permission, Guid? worldId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all effective permissions for a user in a world context.
    /// </summary>
    /// <param name="userId">The user to check.</param>
    /// <param name="worldId">Optional world scope (null = global permissions only).</param>
    /// <returns>List of granted permissions.</returns>
    Task<IEnumerable<PermissionCategory>> GetPermissionsAsync(Guid userId, Guid? worldId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    /// <param name="userId">The user to check.</param>
    /// <param name="roleName">The role name to check for.</param>
    /// <param name="worldId">Optional world scope.</param>
    /// <returns>True if the user has the role.</returns>
    Task<bool> HasRoleAsync(Guid userId, string roleName, Guid? worldId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles assigned to a user.
    /// </summary>
    /// <param name="userId">The user to check.</param>
    /// <param name="worldId">Optional world scope (null = all roles including global).</param>
    /// <returns>List of role names.</returns>
    Task<IEnumerable<string>> GetRolesAsync(Guid userId, Guid? worldId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is a SuperAdmin (has all permissions globally).
    /// </summary>
    Task<bool> IsSuperAdminAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is an Admin for a specific world.
    /// </summary>
    Task<bool> IsWorldAdminAsync(Guid userId, Guid worldId, CancellationToken cancellationToken = default);
}
