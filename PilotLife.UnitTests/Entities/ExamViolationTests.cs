using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class ExamViolationTests
{
    [Fact]
    public void NewExamViolation_HasValidId()
    {
        var violation = new ExamViolation();

        Assert.NotEqual(Guid.Empty, violation.Id);
    }

    [Fact]
    public void NewExamViolation_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var violation = new ExamViolation();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(violation.CreatedAt, before, after);
    }

    [Fact]
    public void NewExamViolation_HasDefaultValues()
    {
        var violation = new ExamViolation();

        Assert.False(violation.CausedFailure);
        Assert.Equal(0, violation.PointsDeducted);
    }

    [Fact]
    public void ExamViolation_CanSetSpeedExcessType()
    {
        var violation = new ExamViolation
        {
            Type = ViolationType.SpeedExcess
        };

        Assert.Equal(ViolationType.SpeedExcess, violation.Type);
    }

    [Fact]
    public void ExamViolation_CanSetAltitudeDeviationType()
    {
        var violation = new ExamViolation
        {
            Type = ViolationType.AltitudeDeviation
        };

        Assert.Equal(ViolationType.AltitudeDeviation, violation.Type);
    }

    [Fact]
    public void ExamViolation_CanSetGForceExcessType()
    {
        var violation = new ExamViolation
        {
            Type = ViolationType.GForceExcess
        };

        Assert.Equal(ViolationType.GForceExcess, violation.Type);
    }

    [Fact]
    public void ExamViolation_CanSetCrashType()
    {
        var violation = new ExamViolation
        {
            Type = ViolationType.Crash,
            CausedFailure = true
        };

        Assert.Equal(ViolationType.Crash, violation.Type);
        Assert.True(violation.CausedFailure);
    }

    [Fact]
    public void ExamViolation_TracksValueAndThreshold()
    {
        var violation = new ExamViolation
        {
            Type = ViolationType.SpeedExcess,
            Value = 280f,
            Threshold = 250f,
            PointsDeducted = 5
        };

        Assert.Equal(280f, violation.Value);
        Assert.Equal(250f, violation.Threshold);
        Assert.Equal(5, violation.PointsDeducted);
    }

    [Fact]
    public void ExamViolation_TracksLocation()
    {
        var violation = new ExamViolation
        {
            LatitudeAtViolation = 37.6213,
            LongitudeAtViolation = -122.3790,
            AltitudeAtViolation = 9500
        };

        Assert.Equal(37.6213, violation.LatitudeAtViolation);
        Assert.Equal(-122.3790, violation.LongitudeAtViolation);
        Assert.Equal(9500, violation.AltitudeAtViolation);
    }

    [Fact]
    public void ExamViolation_AltitudeAtViolation_CanBeNull()
    {
        var violation = new ExamViolation
        {
            AltitudeAtViolation = null
        };

        Assert.Null(violation.AltitudeAtViolation);
    }

    [Fact]
    public void ExamViolation_TracksOccurrence()
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var violation = new ExamViolation
        {
            OccurredAt = occurredAt,
            Description = "Exceeded 250kts below 10,000ft"
        };

        Assert.Equal(occurredAt, violation.OccurredAt);
        Assert.Equal("Exceeded 250kts below 10,000ft", violation.Description);
    }

    [Fact]
    public void ExamViolation_CanLinkToExam()
    {
        var examId = Guid.NewGuid();
        var violation = new ExamViolation
        {
            ExamId = examId
        };

        Assert.Equal(examId, violation.ExamId);
    }

    [Fact]
    public void ExamViolation_CausedFailure_ForSevereViolations()
    {
        var violation = new ExamViolation
        {
            Type = ViolationType.GearUpLanding,
            CausedFailure = true,
            PointsDeducted = 100
        };

        Assert.True(violation.CausedFailure);
        Assert.Equal(100, violation.PointsDeducted);
    }
}
