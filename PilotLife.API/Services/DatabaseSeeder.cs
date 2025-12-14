using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services;

/// <summary>
/// Seeds the database with initial data including default worlds and system roles.
/// </summary>
public class DatabaseSeeder
{
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(PilotLifeDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds all initial data if not already present.
    /// </summary>
    public async Task SeedAsync()
    {
        await SeedWorldsAsync();
        await SeedRolesAsync();
        await SeedAdminUserAsync();
    }

    /// <summary>
    /// Seeds the default worlds if they don't exist.
    /// </summary>
    private async Task SeedWorldsAsync()
    {
        if (await _context.Worlds.AnyAsync())
        {
            _logger.LogInformation("Worlds already seeded, skipping.");
            return;
        }

        _logger.LogInformation("Seeding default worlds...");

        var worlds = new List<World>
        {
            CreateWorldWithSettings(World.CreateEasy()),
            CreateWorldWithSettings(World.CreateMedium()),
            CreateWorldWithSettings(World.CreateHard())
        };

        _context.Worlds.AddRange(worlds);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} default worlds.", worlds.Count);
    }

    /// <summary>
    /// Creates a world with associated default settings.
    /// </summary>
    private static World CreateWorldWithSettings(World world)
    {
        world.Settings = new WorldSettings
        {
            WorldId = world.Id,
            AllowNewPlayers = true,
            AllowIllegalCargo = true,
            EnableAuctions = true,
            EnableAICrews = true,
            EnableAircraftRental = true,
            MaxAircraftPerPlayer = 0, // unlimited
            MaxLoansPerPlayer = 3,
            MaxWorkersPerPlayer = 10,
            MaxActiveJobsPerPlayer = 5,
            RequireApprovalToJoin = false,
            EnableChat = true,
            EnableReporting = true
        };
        return world;
    }

    /// <summary>
    /// Seeds the system roles if they don't exist.
    /// </summary>
    private async Task SeedRolesAsync()
    {
        if (await _context.Roles.AnyAsync())
        {
            _logger.LogInformation("Roles already seeded, skipping.");
            return;
        }

        _logger.LogInformation("Seeding system roles...");

        var roles = new List<Role>
        {
            CreateSuperAdminRole(),
            CreateAdminRole(),
            CreateModeratorRole(),
            CreatePlayerRole()
        };

        _context.Roles.AddRange(roles);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} system roles.", roles.Count);
    }

    /// <summary>
    /// Seeds the default admin user if it doesn't exist.
    /// </summary>
    private async Task SeedAdminUserAsync()
    {
        var adminEmail = "admin@pilotlife.com";

        if (await _context.Users.AnyAsync(u => u.Email == adminEmail))
        {
            _logger.LogInformation("Admin user already exists, skipping.");
            return;
        }

        _logger.LogInformation("Seeding admin user...");

        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (superAdminRole == null)
        {
            _logger.LogWarning("SuperAdmin role not found, cannot create admin user.");
            return;
        }

        var adminUser = new User
        {
            FirstName = "Admin",
            LastName = "User",
            Email = adminEmail,
            PasswordHash = HashPassword("123456"),
            EmailVerified = true,
            Balance = 1000000m
        };

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        var userRole = new UserRole
        {
            UserId = adminUser.Id,
            RoleId = superAdminRole.Id,
            WorldId = null, // Global
            GrantedAt = DateTimeOffset.UtcNow,
            GrantedByUserId = adminUser.Id // Self-granted for system user
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded admin user with SuperAdmin role.");
    }

    /// <summary>
    /// Hashes a password using SHA256.
    /// </summary>
    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Creates the SuperAdmin role with all permissions.
    /// </summary>
    private static Role CreateSuperAdminRole()
    {
        var role = new Role
        {
            Name = "SuperAdmin",
            Description = "Full system access - all permissions in all worlds.",
            Priority = 1000,
            IsSystemRole = true,
            IsGlobal = true
        };

        // SuperAdmin gets all permissions
        foreach (var permission in Enum.GetValues<PermissionCategory>())
        {
            role.Permissions.Add(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission,
                IsGranted = true
            });
        }

        return role;
    }

    /// <summary>
    /// Creates the Admin role with administrative permissions.
    /// </summary>
    private static Role CreateAdminRole()
    {
        var role = new Role
        {
            Name = "Admin",
            Description = "World administrator - can manage users, economy, and moderation.",
            Priority = 100,
            IsSystemRole = true,
            IsGlobal = false // Can be assigned per-world
        };

        // Admin permissions (excludes system-level permissions)
        var adminPermissions = new[]
        {
            PermissionCategory.Users_View,
            PermissionCategory.Users_Edit,
            PermissionCategory.Users_Ban,
            PermissionCategory.Worlds_View,
            PermissionCategory.Worlds_Edit,
            PermissionCategory.Worlds_Settings,
            PermissionCategory.Economy_View,
            PermissionCategory.Economy_Adjust,
            PermissionCategory.Economy_Audit,
            PermissionCategory.Jobs_View,
            PermissionCategory.Jobs_Create,
            PermissionCategory.Jobs_Edit,
            PermissionCategory.Jobs_Delete,
            PermissionCategory.Jobs_ForceComplete,
            PermissionCategory.Aircraft_View,
            PermissionCategory.Aircraft_Spawn,
            PermissionCategory.Marketplace_Manage,
            PermissionCategory.Auctions_Moderate,
            PermissionCategory.Licenses_View,
            PermissionCategory.Licenses_Grant,
            PermissionCategory.Licenses_Revoke,
            PermissionCategory.Exams_Override,
            PermissionCategory.Chat_Moderate,
            PermissionCategory.Reports_View,
            PermissionCategory.Reports_Resolve,
            PermissionCategory.Violations_Issue,
            PermissionCategory.Violations_Clear,
            PermissionCategory.Logs_View,
            PermissionCategory.Analytics_View
        };

        foreach (var permission in adminPermissions)
        {
            role.Permissions.Add(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission,
                IsGranted = true
            });
        }

        return role;
    }

    /// <summary>
    /// Creates the Moderator role with moderation permissions.
    /// </summary>
    private static Role CreateModeratorRole()
    {
        var role = new Role
        {
            Name = "Moderator",
            Description = "Community moderator - can moderate users and view reports.",
            Priority = 50,
            IsSystemRole = true,
            IsGlobal = false // Can be assigned per-world
        };

        var moderatorPermissions = new[]
        {
            PermissionCategory.Users_View,
            PermissionCategory.Jobs_View,
            PermissionCategory.Aircraft_View,
            PermissionCategory.Licenses_View,
            PermissionCategory.Chat_Moderate,
            PermissionCategory.Reports_View,
            PermissionCategory.Reports_Resolve,
            PermissionCategory.Violations_Issue
        };

        foreach (var permission in moderatorPermissions)
        {
            role.Permissions.Add(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission,
                IsGranted = true
            });
        }

        return role;
    }

    /// <summary>
    /// Creates the default Player role with basic permissions.
    /// </summary>
    private static Role CreatePlayerRole()
    {
        var role = new Role
        {
            Name = "Player",
            Description = "Default role for all players.",
            Priority = 0,
            IsSystemRole = true,
            IsGlobal = true
        };

        var playerPermissions = new[]
        {
            PermissionCategory.Jobs_View,
            PermissionCategory.Aircraft_View,
            PermissionCategory.Licenses_View
        };

        foreach (var permission in playerPermissions)
        {
            role.Permissions.Add(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission,
                IsGranted = true
            });
        }

        return role;
    }
}
