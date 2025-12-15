using PilotLife.Domain.Entities;

namespace PilotLife.UnitTests.Entities;

public class UserLicenseTests
{
    [Fact]
    public void NewUserLicense_HasValidId()
    {
        var userLicense = new UserLicense();

        Assert.NotEqual(Guid.Empty, userLicense.Id);
    }

    [Fact]
    public void NewUserLicense_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var userLicense = new UserLicense();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(userLicense.CreatedAt, before, after);
    }

    [Fact]
    public void NewUserLicense_HasDefaultValues()
    {
        var userLicense = new UserLicense();

        Assert.True(userLicense.IsValid);
        Assert.False(userLicense.IsRevoked);
        Assert.Null(userLicense.RevocationReason);
        Assert.Null(userLicense.RevokedAt);
        Assert.Equal(0, userLicense.RenewalCount);
        Assert.Equal(0, userLicense.ExamScore);
        Assert.Equal(0, userLicense.ExamAttempts);
        Assert.Equal(0m, userLicense.TotalPaid);
    }

    [Fact]
    public void UserLicense_ExpiresAt_CanBeNull_ForPermanentLicense()
    {
        var userLicense = new UserLicense
        {
            ExpiresAt = null
        };

        Assert.Null(userLicense.ExpiresAt);
    }

    [Fact]
    public void UserLicense_ExpiresAt_CanBeSet_ForExpiringLicense()
    {
        var expiryDate = DateTimeOffset.UtcNow.AddDays(30);
        var userLicense = new UserLicense
        {
            ExpiresAt = expiryDate
        };

        Assert.Equal(expiryDate, userLicense.ExpiresAt);
    }

    [Fact]
    public void UserLicense_CanBeRevoked()
    {
        var userLicense = new UserLicense
        {
            IsValid = false,
            IsRevoked = true,
            RevocationReason = "Multiple violations",
            RevokedAt = DateTimeOffset.UtcNow
        };

        Assert.False(userLicense.IsValid);
        Assert.True(userLicense.IsRevoked);
        Assert.Equal("Multiple violations", userLicense.RevocationReason);
        Assert.NotNull(userLicense.RevokedAt);
    }

    [Fact]
    public void UserLicense_TracksRenewals()
    {
        var userLicense = new UserLicense
        {
            RenewalCount = 3,
            LastRenewedAt = DateTimeOffset.UtcNow
        };

        Assert.Equal(3, userLicense.RenewalCount);
        Assert.NotNull(userLicense.LastRenewedAt);
    }

    [Fact]
    public void UserLicense_TracksExamPerformance()
    {
        var userLicense = new UserLicense
        {
            ExamScore = 85,
            ExamAttempts = 2
        };

        Assert.Equal(85, userLicense.ExamScore);
        Assert.Equal(2, userLicense.ExamAttempts);
    }

    [Fact]
    public void UserLicense_TracksTotalPaid()
    {
        var userLicense = new UserLicense
        {
            TotalPaid = 7500m
        };

        Assert.Equal(7500m, userLicense.TotalPaid);
    }

    [Fact]
    public void UserLicense_CanLinkToPassedExam()
    {
        var examId = Guid.NewGuid();
        var userLicense = new UserLicense
        {
            PassedExamId = examId
        };

        Assert.Equal(examId, userLicense.PassedExamId);
    }

    [Fact]
    public void UserLicense_PassedExamId_CanBeNull()
    {
        var userLicense = new UserLicense
        {
            PassedExamId = null
        };

        Assert.Null(userLicense.PassedExamId);
    }

    [Fact]
    public void UserLicense_CanLinkToPlayerWorld()
    {
        var playerWorldId = Guid.NewGuid();
        var userLicense = new UserLicense
        {
            PlayerWorldId = playerWorldId
        };

        Assert.Equal(playerWorldId, userLicense.PlayerWorldId);
    }

    [Fact]
    public void UserLicense_CanLinkToLicenseType()
    {
        var licenseTypeId = Guid.NewGuid();
        var userLicense = new UserLicense
        {
            LicenseTypeId = licenseTypeId
        };

        Assert.Equal(licenseTypeId, userLicense.LicenseTypeId);
    }

    [Fact]
    public void UserLicense_EarnedAt_IsRequired()
    {
        var earnedAt = DateTimeOffset.UtcNow;
        var userLicense = new UserLicense
        {
            EarnedAt = earnedAt
        };

        Assert.Equal(earnedAt, userLicense.EarnedAt);
    }
}
