using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class LicenseExamTests
{
    [Fact]
    public void NewLicenseExam_HasValidId()
    {
        var exam = new LicenseExam();

        Assert.NotEqual(Guid.Empty, exam.Id);
    }

    [Fact]
    public void NewLicenseExam_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var exam = new LicenseExam();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(exam.CreatedAt, before, after);
    }

    [Fact]
    public void NewLicenseExam_HasDefaultValues()
    {
        var exam = new LicenseExam();

        Assert.Equal(ExamStatus.Scheduled, exam.Status);
        Assert.Equal(70, exam.PassingScore);
        Assert.Equal(1, exam.AttemptNumber);
        Assert.Equal(0, exam.Score);
        Assert.Equal(string.Empty, exam.DepartureIcao);
    }

    [Fact]
    public void NewLicenseExam_HasEmptyCollections()
    {
        var exam = new LicenseExam();

        Assert.NotNull(exam.Maneuvers);
        Assert.Empty(exam.Maneuvers);
        Assert.NotNull(exam.Checkpoints);
        Assert.Empty(exam.Checkpoints);
        Assert.NotNull(exam.Landings);
        Assert.Empty(exam.Landings);
        Assert.NotNull(exam.Violations);
        Assert.Empty(exam.Violations);
    }

    [Fact]
    public void LicenseExam_CanSetStatus_Scheduled()
    {
        var exam = new LicenseExam
        {
            Status = ExamStatus.Scheduled
        };

        Assert.Equal(ExamStatus.Scheduled, exam.Status);
    }

    [Fact]
    public void LicenseExam_CanSetStatus_InProgress()
    {
        var exam = new LicenseExam
        {
            Status = ExamStatus.InProgress
        };

        Assert.Equal(ExamStatus.InProgress, exam.Status);
    }

    [Fact]
    public void LicenseExam_CanSetStatus_Passed()
    {
        var exam = new LicenseExam
        {
            Status = ExamStatus.Passed
        };

        Assert.Equal(ExamStatus.Passed, exam.Status);
    }

    [Fact]
    public void LicenseExam_CanSetStatus_Failed()
    {
        var exam = new LicenseExam
        {
            Status = ExamStatus.Failed
        };

        Assert.Equal(ExamStatus.Failed, exam.Status);
    }

    [Fact]
    public void LicenseExam_CanSetStatus_Abandoned()
    {
        var exam = new LicenseExam
        {
            Status = ExamStatus.Abandoned
        };

        Assert.Equal(ExamStatus.Abandoned, exam.Status);
    }

    [Fact]
    public void LicenseExam_CanSetStatus_Expired()
    {
        var exam = new LicenseExam
        {
            Status = ExamStatus.Expired
        };

        Assert.Equal(ExamStatus.Expired, exam.Status);
    }

    [Fact]
    public void LicenseExam_CanSetRequiredAircraftCategory()
    {
        var exam = new LicenseExam
        {
            RequiredAircraftCategory = AircraftCategory.MEP
        };

        Assert.Equal(AircraftCategory.MEP, exam.RequiredAircraftCategory);
    }

    [Fact]
    public void LicenseExam_RequiredAircraftCategory_CanBeNull()
    {
        var exam = new LicenseExam
        {
            RequiredAircraftCategory = null
        };

        Assert.Null(exam.RequiredAircraftCategory);
    }

    [Fact]
    public void LicenseExam_TracksTiming()
    {
        var scheduledAt = DateTimeOffset.UtcNow;
        var startedAt = scheduledAt.AddMinutes(5);
        var completedAt = startedAt.AddMinutes(30);

        var exam = new LicenseExam
        {
            ScheduledAt = scheduledAt,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            TimeLimitMinutes = 45
        };

        Assert.Equal(scheduledAt, exam.ScheduledAt);
        Assert.Equal(startedAt, exam.StartedAt);
        Assert.Equal(completedAt, exam.CompletedAt);
        Assert.Equal(45, exam.TimeLimitMinutes);
    }

    [Fact]
    public void LicenseExam_TracksScore()
    {
        var exam = new LicenseExam
        {
            Score = 85,
            PassingScore = 70
        };

        Assert.Equal(85, exam.Score);
        Assert.Equal(70, exam.PassingScore);
    }

    [Fact]
    public void LicenseExam_TracksFailureReason()
    {
        var exam = new LicenseExam
        {
            Status = ExamStatus.Failed,
            FailureReason = "Exceeded time limit"
        };

        Assert.Equal(ExamStatus.Failed, exam.Status);
        Assert.Equal("Exceeded time limit", exam.FailureReason);
    }

    [Fact]
    public void LicenseExam_TracksExaminerNotes()
    {
        var exam = new LicenseExam
        {
            ExaminerNotes = "Good handling, needs to work on altitude management."
        };

        Assert.Equal("Good handling, needs to work on altitude management.", exam.ExaminerNotes);
    }

    [Fact]
    public void LicenseExam_TracksAttemptNumber()
    {
        var exam = new LicenseExam
        {
            AttemptNumber = 3
        };

        Assert.Equal(3, exam.AttemptNumber);
    }

    [Fact]
    public void LicenseExam_TracksFeePaid()
    {
        var exam = new LicenseExam
        {
            FeePaid = 5000m
        };

        Assert.Equal(5000m, exam.FeePaid);
    }

    [Fact]
    public void LicenseExam_TracksCooldownPeriod()
    {
        var retakeDate = DateTimeOffset.UtcNow.AddHours(6);
        var exam = new LicenseExam
        {
            EligibleForRetakeAt = retakeDate
        };

        Assert.Equal(retakeDate, exam.EligibleForRetakeAt);
    }

    [Fact]
    public void LicenseExam_TracksFlightData()
    {
        var exam = new LicenseExam
        {
            FlightTimeMinutes = 25,
            DistanceFlownNm = 45.5,
            AircraftUsed = "C172"
        };

        Assert.Equal(25, exam.FlightTimeMinutes);
        Assert.Equal(45.5, exam.DistanceFlownNm);
        Assert.Equal("C172", exam.AircraftUsed);
    }

    [Fact]
    public void LicenseExam_TracksRoute()
    {
        var routeJson = "{\"waypoints\": [{\"icao\": \"KSFO\"}]}";
        var exam = new LicenseExam
        {
            DepartureIcao = "KSFO",
            RouteJson = routeJson,
            AssignedAltitudeFt = 3000
        };

        Assert.Equal("KSFO", exam.DepartureIcao);
        Assert.Equal(routeJson, exam.RouteJson);
        Assert.Equal(3000, exam.AssignedAltitudeFt);
    }

    [Fact]
    public void LicenseExam_CanLinkToPlayerWorld()
    {
        var playerWorldId = Guid.NewGuid();
        var exam = new LicenseExam
        {
            PlayerWorldId = playerWorldId
        };

        Assert.Equal(playerWorldId, exam.PlayerWorldId);
    }

    [Fact]
    public void LicenseExam_CanLinkToWorld()
    {
        var worldId = Guid.NewGuid();
        var exam = new LicenseExam
        {
            WorldId = worldId
        };

        Assert.Equal(worldId, exam.WorldId);
    }

    [Fact]
    public void LicenseExam_CanLinkToLicenseType()
    {
        var licenseTypeId = Guid.NewGuid();
        var exam = new LicenseExam
        {
            LicenseTypeId = licenseTypeId
        };

        Assert.Equal(licenseTypeId, exam.LicenseTypeId);
    }
}
