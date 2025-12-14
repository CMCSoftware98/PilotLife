using PilotLife.Domain.Entities;

namespace PilotLife.UnitTests.Entities;

public class RoleTests
{
    [Fact]
    public void NewRole_HasValidId()
    {
        var role = new Role();

        Assert.NotEqual(Guid.Empty, role.Id);
    }

    [Fact]
    public void NewRole_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var role = new Role();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(role.CreatedAt, before, after);
    }

    [Fact]
    public void NewRole_HasDefaultValues()
    {
        var role = new Role();

        Assert.Equal(string.Empty, role.Name);
        Assert.Null(role.Description);
        Assert.Equal(0, role.Priority);
        Assert.False(role.IsSystemRole);
        Assert.False(role.IsGlobal);
    }

    [Fact]
    public void Permissions_DefaultsToEmptyCollection()
    {
        var role = new Role();

        Assert.NotNull(role.Permissions);
        Assert.Empty(role.Permissions);
    }

    [Fact]
    public void UserRoles_DefaultsToEmptyCollection()
    {
        var role = new Role();

        Assert.NotNull(role.UserRoles);
        Assert.Empty(role.UserRoles);
    }

    [Fact]
    public void Role_CanBeCreatedWithProperties()
    {
        var role = new Role
        {
            Name = "Admin",
            Description = "Administrator role",
            Priority = 100,
            IsSystemRole = true,
            IsGlobal = true
        };

        Assert.Equal("Admin", role.Name);
        Assert.Equal("Administrator role", role.Description);
        Assert.Equal(100, role.Priority);
        Assert.True(role.IsSystemRole);
        Assert.True(role.IsGlobal);
    }

    [Fact]
    public void Priority_DeterminesRoleHierarchy()
    {
        var superAdmin = new Role { Name = "SuperAdmin", Priority = 1000 };
        var admin = new Role { Name = "Admin", Priority = 100 };
        var moderator = new Role { Name = "Moderator", Priority = 50 };
        var player = new Role { Name = "Player", Priority = 0 };

        Assert.True(superAdmin.Priority > admin.Priority);
        Assert.True(admin.Priority > moderator.Priority);
        Assert.True(moderator.Priority > player.Priority);
    }
}
