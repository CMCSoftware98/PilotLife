using Microsoft.EntityFrameworkCore;
using PilotLife.Application.Authorization;
using PilotLife.Database.Data;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services;

/// <summary>
/// Implementation of authorization service using role-based access control.
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<AuthorizationService> _logger;

    private const string SuperAdminRole = "SuperAdmin";
    private const string AdminRole = "Admin";

    public AuthorizationService(PilotLifeDbContext context, ILogger<AuthorizationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, PermissionCategory permission, Guid? worldId = null, CancellationToken cancellationToken = default)
    {
        // SuperAdmin has all permissions
        if (await IsSuperAdminAsync(userId, cancellationToken))
        {
            return true;
        }

        // Get user's active roles (global + world-specific if worldId provided)
        var userRoles = await GetActiveUserRolesQuery(userId, worldId)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.Permissions)
            .ToListAsync(cancellationToken);

        // Check if any role grants this permission
        foreach (var userRole in userRoles)
        {
            var rolePermission = userRole.Role.Permissions
                .FirstOrDefault(p => p.Permission == permission);

            if (rolePermission != null)
            {
                // Explicit deny takes precedence
                if (!rolePermission.IsGranted)
                {
                    return false;
                }
                return true;
            }
        }

        return false;
    }

    public async Task<IEnumerable<PermissionCategory>> GetPermissionsAsync(Guid userId, Guid? worldId = null, CancellationToken cancellationToken = default)
    {
        // SuperAdmin has all permissions
        if (await IsSuperAdminAsync(userId, cancellationToken))
        {
            return Enum.GetValues<PermissionCategory>();
        }

        var userRoles = await GetActiveUserRolesQuery(userId, worldId)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.Permissions)
            .ToListAsync(cancellationToken);

        var grantedPermissions = new HashSet<PermissionCategory>();
        var deniedPermissions = new HashSet<PermissionCategory>();

        // Process roles by priority (highest first)
        foreach (var userRole in userRoles.OrderByDescending(ur => ur.Role.Priority))
        {
            foreach (var permission in userRole.Role.Permissions)
            {
                if (permission.IsGranted)
                {
                    if (!deniedPermissions.Contains(permission.Permission))
                    {
                        grantedPermissions.Add(permission.Permission);
                    }
                }
                else
                {
                    deniedPermissions.Add(permission.Permission);
                    grantedPermissions.Remove(permission.Permission);
                }
            }
        }

        return grantedPermissions;
    }

    public async Task<bool> HasRoleAsync(Guid userId, string roleName, Guid? worldId = null, CancellationToken cancellationToken = default)
    {
        return await GetActiveUserRolesQuery(userId, worldId)
            .AnyAsync(ur => ur.Role.Name == roleName, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetRolesAsync(Guid userId, Guid? worldId = null, CancellationToken cancellationToken = default)
    {
        return await GetActiveUserRolesQuery(userId, worldId)
            .Select(ur => ur.Role.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsSuperAdminAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Where(ur => ur.Role.Name == SuperAdminRole)
            .Where(ur => ur.Role.IsGlobal)
            .Where(ur => ur.ExpiresAt == null || ur.ExpiresAt > DateTimeOffset.UtcNow)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> IsWorldAdminAsync(Guid userId, Guid worldId, CancellationToken cancellationToken = default)
    {
        // SuperAdmin is admin of all worlds
        if (await IsSuperAdminAsync(userId, cancellationToken))
        {
            return true;
        }

        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Where(ur => ur.Role.Name == AdminRole)
            .Where(ur => ur.WorldId == worldId || ur.Role.IsGlobal)
            .Where(ur => ur.ExpiresAt == null || ur.ExpiresAt > DateTimeOffset.UtcNow)
            .AnyAsync(cancellationToken);
    }

    private IQueryable<Domain.Entities.UserRole> GetActiveUserRolesQuery(Guid userId, Guid? worldId)
    {
        var query = _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Where(ur => ur.ExpiresAt == null || ur.ExpiresAt > DateTimeOffset.UtcNow);

        if (worldId.HasValue)
        {
            // Include global roles AND world-specific roles
            query = query.Where(ur => ur.Role.IsGlobal || ur.WorldId == worldId);
        }
        else
        {
            // Only global roles
            query = query.Where(ur => ur.Role.IsGlobal);
        }

        return query;
    }
}
