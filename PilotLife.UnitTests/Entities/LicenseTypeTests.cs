using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class LicenseTypeTests
{
    [Fact]
    public void NewLicenseType_HasValidId()
    {
        var licenseType = new LicenseType();

        Assert.NotEqual(Guid.Empty, licenseType.Id);
    }

    [Fact]
    public void NewLicenseType_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var licenseType = new LicenseType();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(licenseType.CreatedAt, before, after);
    }

    [Fact]
    public void NewLicenseType_HasDefaultValues()
    {
        var licenseType = new LicenseType();

        Assert.Equal(string.Empty, licenseType.Code);
        Assert.Equal(string.Empty, licenseType.Name);
        Assert.Null(licenseType.Description);
        Assert.Equal(70, licenseType.PassingScore);
        Assert.True(licenseType.IsActive);
    }

    [Fact]
    public void NewLicenseType_HasEmptyCollections()
    {
        var licenseType = new LicenseType();

        Assert.NotNull(licenseType.UserLicenses);
        Assert.Empty(licenseType.UserLicenses);
        Assert.NotNull(licenseType.Exams);
        Assert.Empty(licenseType.Exams);
    }

    [Fact]
    public void LicenseType_CanSetCategory()
    {
        var licenseType = new LicenseType
        {
            Category = LicenseCategory.Core
        };

        Assert.Equal(LicenseCategory.Core, licenseType.Category);
    }

    [Fact]
    public void LicenseType_CanSetEndorsementCategory()
    {
        var licenseType = new LicenseType
        {
            Category = LicenseCategory.Endorsement
        };

        Assert.Equal(LicenseCategory.Endorsement, licenseType.Category);
    }

    [Fact]
    public void LicenseType_CanSetTypeRatingCategory()
    {
        var licenseType = new LicenseType
        {
            Category = LicenseCategory.TypeRating
        };

        Assert.Equal(LicenseCategory.TypeRating, licenseType.Category);
    }

    [Fact]
    public void LicenseType_CanSetRequiredAircraftCategory()
    {
        var licenseType = new LicenseType
        {
            RequiredAircraftCategory = AircraftCategory.SEP
        };

        Assert.Equal(AircraftCategory.SEP, licenseType.RequiredAircraftCategory);
    }

    [Fact]
    public void LicenseType_RequiredAircraftCategory_CanBeNull()
    {
        var licenseType = new LicenseType
        {
            RequiredAircraftCategory = null
        };

        Assert.Null(licenseType.RequiredAircraftCategory);
    }

    [Fact]
    public void LicenseType_ValidityGameDays_CanBeNull_ForPermanentLicense()
    {
        var licenseType = new LicenseType
        {
            ValidityGameDays = null
        };

        Assert.Null(licenseType.ValidityGameDays);
    }

    [Fact]
    public void LicenseType_ValidityGameDays_CanBeSet_ForExpiringLicense()
    {
        var licenseType = new LicenseType
        {
            ValidityGameDays = 180
        };

        Assert.Equal(180, licenseType.ValidityGameDays);
    }

    [Fact]
    public void LicenseType_BaseRenewalCost_CanBeNull_ForPermanentLicense()
    {
        var licenseType = new LicenseType
        {
            BaseRenewalCost = null
        };

        Assert.Null(licenseType.BaseRenewalCost);
    }

    [Fact]
    public void LicenseType_CanSetPrerequisiteLicensesJson()
    {
        var licenseType = new LicenseType
        {
            PrerequisiteLicensesJson = "[\"PPL\", \"IR\"]"
        };

        Assert.Equal("[\"PPL\", \"IR\"]", licenseType.PrerequisiteLicensesJson);
    }

    [Fact]
    public void LicenseType_CanSetExamConfiguration()
    {
        var licenseType = new LicenseType
        {
            BaseExamCost = 5000m,
            ExamDurationMinutes = 45,
            PassingScore = 70
        };

        Assert.Equal(5000m, licenseType.BaseExamCost);
        Assert.Equal(45, licenseType.ExamDurationMinutes);
        Assert.Equal(70, licenseType.PassingScore);
    }

    [Fact]
    public void LicenseType_DisplayOrder_AffectsSorting()
    {
        var ppl = new LicenseType { Code = "PPL", DisplayOrder = 2 };
        var cpl = new LicenseType { Code = "CPL", DisplayOrder = 3 };
        var discovery = new LicenseType { Code = "DISCOVERY", DisplayOrder = 1 };

        var licenses = new[] { cpl, ppl, discovery }.OrderBy(l => l.DisplayOrder).ToList();

        Assert.Equal("DISCOVERY", licenses[0].Code);
        Assert.Equal("PPL", licenses[1].Code);
        Assert.Equal("CPL", licenses[2].Code);
    }
}
