using PilotLife.Domain.Entities;

namespace PilotLife.UnitTests.Entities;

public class UserRoleTests
{
    [Fact]
    public void NewUserRole_HasValidId()
    {
        var userRole = new UserRole();

        Assert.NotEqual(Guid.Empty, userRole.Id);
    }

    [Fact]
    public void NewUserRole_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var userRole = new UserRole();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(userRole.CreatedAt, before, after);
    }

    [Fact]
    public void NewUserRole_HasGrantedAtSet()
    {
        var before = DateTimeOffset.UtcNow;
        var userRole = new UserRole();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(userRole.GrantedAt, before, after);
    }

    [Fact]
    public void NewUserRole_HasNoExpiresAt()
    {
        var userRole = new UserRole();

        Assert.Null(userRole.ExpiresAt);
    }

    [Fact]
    public void NewUserRole_HasNoWorldId()
    {
        var userRole = new UserRole();

        Assert.Null(userRole.WorldId);
    }

    [Fact]
    public void IsActive_ReturnsTrue_WhenNoExpiration()
    {
        var userRole = new UserRole
        {
            ExpiresAt = null
        };

        Assert.True(userRole.IsActive);
    }

    [Fact]
    public void IsActive_ReturnsTrue_WhenNotExpired()
    {
        var userRole = new UserRole
        {
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        Assert.True(userRole.IsActive);
    }

    [Fact]
    public void IsActive_ReturnsFalse_WhenExpired()
    {
        var userRole = new UserRole
        {
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        Assert.False(userRole.IsActive);
    }

    [Fact]
    public void IsActive_ReturnsFalse_WhenJustExpired()
    {
        var userRole = new UserRole
        {
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1)
        };

        Assert.False(userRole.IsActive);
    }

    [Fact]
    public void UserRole_CanBeGlobal_WhenWorldIdIsNull()
    {
        var userRole = new UserRole
        {
            WorldId = null
        };

        Assert.Null(userRole.WorldId);
    }

    [Fact]
    public void UserRole_CanBeScopedToWorld()
    {
        var worldId = Guid.CreateVersion7();
        var userRole = new UserRole
        {
            WorldId = worldId
        };

        Assert.Equal(worldId, userRole.WorldId);
    }

    [Fact]
    public void UserRole_TracksGrantingUser()
    {
        var grantedByUserId = Guid.CreateVersion7();
        var userRole = new UserRole
        {
            GrantedByUserId = grantedByUserId
        };

        Assert.Equal(grantedByUserId, userRole.GrantedByUserId);
    }
}
