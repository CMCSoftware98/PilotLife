using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class JobTests
{
    [Fact]
    public void NewJob_HasValidId()
    {
        var job = new Job();

        Assert.NotEqual(Guid.Empty, job.Id);
    }

    [Fact]
    public void NewJob_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var job = new Job();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(job.CreatedAt, before, after);
    }

    [Fact]
    public void NewJob_HasDefaultValues()
    {
        var job = new Job();

        Assert.Equal(JobStatus.Available, job.Status);
        Assert.Equal(JobUrgency.Standard, job.Urgency);
        Assert.Equal(RouteDifficulty.Easy, job.RouteDifficulty);
        Assert.Equal(1, job.RiskLevel);
        Assert.False(job.IsCompleted);
        Assert.False(job.IsFailed);
        Assert.False(job.RequiresSpecialCertification);
    }

    [Fact]
    public void IsAvailable_WhenStatusAvailableAndNotExpired_ReturnsTrue()
    {
        var job = new Job
        {
            Status = JobStatus.Available,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        };

        Assert.True(job.IsAvailable);
    }

    [Fact]
    public void IsAvailable_WhenExpired_ReturnsFalse()
    {
        var job = new Job
        {
            Status = JobStatus.Available,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

        Assert.False(job.IsAvailable);
    }

    [Fact]
    public void IsAvailable_WhenAccepted_ReturnsFalse()
    {
        var job = new Job
        {
            Status = JobStatus.Accepted,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        };

        Assert.False(job.IsAvailable);
    }

    [Fact]
    public void IsExpired_WhenPastExpiry_ReturnsTrue()
    {
        var job = new Job
        {
            Status = JobStatus.Available,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

        Assert.True(job.IsExpired);
    }

    [Fact]
    public void TimeRemaining_WhenAvailable_ReturnsTimeSpan()
    {
        var job = new Job
        {
            Status = JobStatus.Available,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2)
        };

        Assert.NotNull(job.TimeRemaining);
        Assert.True(job.TimeRemaining.Value.TotalHours > 1.9);
    }

    [Fact]
    public void CreateCargoJob_SetsCorrectValues()
    {
        var worldId = Guid.NewGuid();
        var cargoTypeId = Guid.NewGuid();

        var job = Job.CreateCargoJob(
            worldId, 1, "EGLL", 2, "EGCC",
            120, cargoTypeId, "General Freight",
            2000, 5000m, JobUrgency.Priority,
            DateTimeOffset.UtcNow.AddHours(24));

        Assert.Equal(worldId, job.WorldId);
        Assert.Equal(1, job.DepartureAirportId);
        Assert.Equal("EGLL", job.DepartureIcao);
        Assert.Equal(2, job.ArrivalAirportId);
        Assert.Equal("EGCC", job.ArrivalIcao);
        Assert.Equal(120, job.DistanceNm);
        Assert.Equal(DistanceCategory.Short, job.DistanceCategory);
        Assert.Equal(JobType.Cargo, job.Type);
        Assert.Equal(cargoTypeId, job.CargoTypeId);
        Assert.Equal("General Freight", job.CargoType);
        Assert.Equal(2000, job.WeightLbs);
        Assert.Equal(5000m, job.Payout);
        Assert.Equal(JobUrgency.Priority, job.Urgency);
        Assert.Contains("General Freight", job.Title);
    }

    [Fact]
    public void CreateCargoJob_SetsCorrectDistanceCategory_VeryShort()
    {
        var job = Job.CreateCargoJob(
            Guid.NewGuid(), 1, "EGLL", 2, "EGCC",
            25, Guid.NewGuid(), "Test", 1000, 1000m,
            JobUrgency.Standard, DateTimeOffset.UtcNow.AddHours(24));

        Assert.Equal(DistanceCategory.VeryShort, job.DistanceCategory);
    }

    [Fact]
    public void CreateCargoJob_SetsCorrectDistanceCategory_Medium()
    {
        var job = Job.CreateCargoJob(
            Guid.NewGuid(), 1, "EGLL", 2, "EGCC",
            300, Guid.NewGuid(), "Test", 1000, 1000m,
            JobUrgency.Standard, DateTimeOffset.UtcNow.AddHours(24));

        Assert.Equal(DistanceCategory.Medium, job.DistanceCategory);
    }

    [Fact]
    public void CreateCargoJob_SetsCorrectDistanceCategory_Long()
    {
        var job = Job.CreateCargoJob(
            Guid.NewGuid(), 1, "EGLL", 2, "EGCC",
            1000, Guid.NewGuid(), "Test", 1000, 1000m,
            JobUrgency.Standard, DateTimeOffset.UtcNow.AddHours(24));

        Assert.Equal(DistanceCategory.Long, job.DistanceCategory);
    }

    [Fact]
    public void CreateCargoJob_SetsCorrectDistanceCategory_UltraLong()
    {
        var job = Job.CreateCargoJob(
            Guid.NewGuid(), 1, "EGLL", 2, "EGCC",
            2000, Guid.NewGuid(), "Test", 1000, 1000m,
            JobUrgency.Standard, DateTimeOffset.UtcNow.AddHours(24));

        Assert.Equal(DistanceCategory.UltraLong, job.DistanceCategory);
    }

    [Fact]
    public void CreatePassengerJob_SetsCorrectValues()
    {
        var worldId = Guid.NewGuid();

        var job = Job.CreatePassengerJob(
            worldId, 1, "KJFK", 2, "KLAX",
            2500, 150, PassengerClass.Business,
            75000m, JobUrgency.Standard,
            DateTimeOffset.UtcNow.AddHours(48));

        Assert.Equal(worldId, job.WorldId);
        Assert.Equal(JobType.Passenger, job.Type);
        Assert.Equal(150, job.PassengerCount);
        Assert.Equal(PassengerClass.Business, job.PassengerClass);
        Assert.Equal(75000m, job.Payout);
        Assert.Equal(DistanceCategory.UltraLong, job.DistanceCategory);
        Assert.Contains("Business", job.Title);
    }

    [Fact]
    public void Accept_SetsCorrectValues()
    {
        var job = new Job
        {
            Status = JobStatus.Available,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        };
        var userId = Guid.NewGuid();
        var playerWorldId = Guid.NewGuid();
        var before = DateTimeOffset.UtcNow;

        job.Accept(userId, playerWorldId);
        var after = DateTimeOffset.UtcNow;

        Assert.Equal(JobStatus.Accepted, job.Status);
        Assert.Equal(userId, job.AssignedToUserId);
        Assert.Equal(playerWorldId, job.AssignedToPlayerWorldId);
        Assert.NotNull(job.AcceptedAt);
        Assert.InRange(job.AcceptedAt.Value, before, after);
    }

    [Fact]
    public void Accept_WhenAlreadyAccepted_Throws()
    {
        var job = new Job
        {
            Status = JobStatus.Accepted,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        };

        Assert.Throws<InvalidOperationException>(() =>
            job.Accept(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void Accept_WhenExpired_Throws()
    {
        var job = new Job
        {
            Status = JobStatus.Available,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

        Assert.Throws<InvalidOperationException>(() =>
            job.Accept(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void Start_SetsCorrectValues()
    {
        var job = new Job { Status = JobStatus.Accepted };
        var before = DateTimeOffset.UtcNow;

        job.Start();
        var after = DateTimeOffset.UtcNow;

        Assert.Equal(JobStatus.InProgress, job.Status);
        Assert.NotNull(job.StartedAt);
        Assert.InRange(job.StartedAt.Value, before, after);
    }

    [Fact]
    public void Start_WhenNotAccepted_Throws()
    {
        var job = new Job { Status = JobStatus.Available };

        Assert.Throws<InvalidOperationException>(() => job.Start());
    }

    [Fact]
    public void Complete_SetsCorrectValues()
    {
        var job = new Job { Status = JobStatus.InProgress };
        var before = DateTimeOffset.UtcNow;

        job.Complete(5000m);
        var after = DateTimeOffset.UtcNow;

        Assert.Equal(JobStatus.Completed, job.Status);
        Assert.True(job.IsCompleted);
        Assert.NotNull(job.CompletedAt);
        Assert.InRange(job.CompletedAt.Value, before, after);
        Assert.Equal(5000m, job.ActualPayout);
    }

    [Fact]
    public void Complete_WhenNotInProgress_Throws()
    {
        var job = new Job { Status = JobStatus.Accepted };

        Assert.Throws<InvalidOperationException>(() => job.Complete(5000m));
    }

    [Fact]
    public void Fail_SetsCorrectValues()
    {
        var job = new Job { Status = JobStatus.InProgress };
        var before = DateTimeOffset.UtcNow;

        job.Fail("Wrong destination");
        var after = DateTimeOffset.UtcNow;

        Assert.Equal(JobStatus.Failed, job.Status);
        Assert.True(job.IsFailed);
        Assert.Equal("Wrong destination", job.FailureReason);
        Assert.NotNull(job.CompletedAt);
        Assert.InRange(job.CompletedAt.Value, before, after);
        Assert.Equal(0m, job.ActualPayout);
    }

    [Fact]
    public void Cancel_SetsCorrectValues()
    {
        var job = new Job { Status = JobStatus.Accepted };

        job.Cancel();

        Assert.Equal(JobStatus.Cancelled, job.Status);
        Assert.Equal(0m, job.ActualPayout);
    }

    [Fact]
    public void Cancel_WhenCompleted_Throws()
    {
        var job = new Job { Status = JobStatus.Completed };

        Assert.Throws<InvalidOperationException>(() => job.Cancel());
    }

    [Fact]
    public void Cancel_WhenFailed_Throws()
    {
        var job = new Job { Status = JobStatus.Failed };

        Assert.Throws<InvalidOperationException>(() => job.Cancel());
    }
}
