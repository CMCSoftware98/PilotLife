using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class ExamLandingTests
{
    [Fact]
    public void NewExamLanding_HasValidId()
    {
        var landing = new ExamLanding();

        Assert.NotEqual(Guid.Empty, landing.Id);
    }

    [Fact]
    public void NewExamLanding_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var landing = new ExamLanding();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(landing.CreatedAt, before, after);
    }

    [Fact]
    public void NewExamLanding_HasDefaultValues()
    {
        var landing = new ExamLanding();

        Assert.Equal(string.Empty, landing.AirportIcao);
        Assert.True(landing.GearDown);
    }

    [Fact]
    public void ExamLanding_CanSetTouchAndGoType()
    {
        var landing = new ExamLanding
        {
            Type = LandingType.TouchAndGo
        };

        Assert.Equal(LandingType.TouchAndGo, landing.Type);
    }

    [Fact]
    public void ExamLanding_CanSetFullStopType()
    {
        var landing = new ExamLanding
        {
            Type = LandingType.FullStop
        };

        Assert.Equal(LandingType.FullStop, landing.Type);
    }

    [Fact]
    public void ExamLanding_TracksLandingMetrics()
    {
        var landing = new ExamLanding
        {
            VerticalSpeedFpm = -300f,
            CenterlineDeviationFt = 5f,
            TouchdownZoneDistanceFt = 500f,
            GroundSpeedKts = 65f
        };

        Assert.Equal(-300f, landing.VerticalSpeedFpm);
        Assert.Equal(5f, landing.CenterlineDeviationFt);
        Assert.Equal(500f, landing.TouchdownZoneDistanceFt);
        Assert.Equal(65f, landing.GroundSpeedKts);
    }

    [Fact]
    public void ExamLanding_TracksAttitudeAtTouchdown()
    {
        var landing = new ExamLanding
        {
            PitchDeg = 5.5f,
            BankDeg = 0.5f
        };

        Assert.Equal(5.5f, landing.PitchDeg);
        Assert.Equal(0.5f, landing.BankDeg);
    }

    [Fact]
    public void ExamLanding_TracksGearState()
    {
        var goodLanding = new ExamLanding
        {
            GearDown = true
        };

        var badLanding = new ExamLanding
        {
            GearDown = false
        };

        Assert.True(goodLanding.GearDown);
        Assert.False(badLanding.GearDown);
    }

    [Fact]
    public void ExamLanding_TracksAirportAndRunway()
    {
        var landing = new ExamLanding
        {
            AirportIcao = "KSFO",
            RunwayUsed = "28R"
        };

        Assert.Equal("KSFO", landing.AirportIcao);
        Assert.Equal("28R", landing.RunwayUsed);
    }

    [Fact]
    public void ExamLanding_TracksScoring()
    {
        var landing = new ExamLanding
        {
            PointsAwarded = 25,
            MaxPoints = 30
        };

        Assert.Equal(25, landing.PointsAwarded);
        Assert.Equal(30, landing.MaxPoints);
    }

    [Fact]
    public void ExamLanding_TracksOrder()
    {
        var landing = new ExamLanding
        {
            Order = 2
        };

        Assert.Equal(2, landing.Order);
    }

    [Fact]
    public void ExamLanding_TracksTimingAndNotes()
    {
        var landedAt = DateTimeOffset.UtcNow;
        var landing = new ExamLanding
        {
            LandedAt = landedAt,
            Notes = "Smooth touchdown, good centerline tracking"
        };

        Assert.Equal(landedAt, landing.LandedAt);
        Assert.Equal("Smooth touchdown, good centerline tracking", landing.Notes);
    }

    [Fact]
    public void ExamLanding_CanLinkToExam()
    {
        var examId = Guid.NewGuid();
        var landing = new ExamLanding
        {
            ExamId = examId
        };

        Assert.Equal(examId, landing.ExamId);
    }

    [Fact]
    public void ExamLanding_OptionalFields_CanBeNull()
    {
        var landing = new ExamLanding
        {
            GroundSpeedKts = null,
            PitchDeg = null,
            BankDeg = null,
            RunwayUsed = null,
            Notes = null
        };

        Assert.Null(landing.GroundSpeedKts);
        Assert.Null(landing.PitchDeg);
        Assert.Null(landing.BankDeg);
        Assert.Null(landing.RunwayUsed);
        Assert.Null(landing.Notes);
    }
}
