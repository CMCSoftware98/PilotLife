using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a cargo/passenger transport job.
/// </summary>
public class Job : BaseEntity
{
    /// <summary>
    /// The world this job exists in.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    // Route information
    public int DepartureAirportId { get; set; }
    public Airport DepartureAirport { get; set; } = null!;
    public string DepartureIcao { get; set; } = string.Empty;

    public int ArrivalAirportId { get; set; }
    public Airport ArrivalAirport { get; set; } = null!;
    public string ArrivalIcao { get; set; } = string.Empty;

    public double DistanceNm { get; set; }
    public DistanceCategory DistanceCategory { get; set; }
    public RouteDifficulty RouteDifficulty { get; set; } = RouteDifficulty.Easy;

    // Job type and details
    public JobType Type { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Available;
    public JobUrgency Urgency { get; set; } = JobUrgency.Standard;

    // Cargo details (when Type == Cargo)
    public Guid? CargoTypeId { get; set; }
    public CargoType? CargoTypeRef { get; set; }

    /// <summary>
    /// Legacy cargo type string for backwards compatibility.
    /// </summary>
    public string CargoType { get; set; } = string.Empty;

    public int? WeightLbs { get; set; }
    public decimal? VolumeCuFt { get; set; }

    // Passenger details (when Type == Passenger)
    public int? PassengerCount { get; set; }
    public PassengerClass? PassengerClass { get; set; }

    // Requirements
    public string RequiredAircraftType { get; set; } = string.Empty;
    public int? MinCrewCount { get; set; }
    public bool RequiresSpecialCertification { get; set; }
    public string? RequiredCertification { get; set; }

    // Risk level (1-5 stars)
    public int RiskLevel { get; set; } = 1;

    // Payout
    public decimal BasePayout { get; set; }
    public decimal Payout { get; set; }
    public decimal? BonusPayout { get; set; }

    // Timing
    public int EstimatedFlightTimeMinutes { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
    public Guid? AssignedToPlayerWorldId { get; set; }
    public PlayerWorld? AssignedToPlayerWorld { get; set; }

    // Completion
    public bool IsCompleted { get; set; }
    public bool IsFailed { get; set; }
    public string? FailureReason { get; set; }
    public decimal? ActualPayout { get; set; }

    // Display
    public string? Title { get; set; }
    public string? Description { get; set; }

    /// <summary>
    /// Whether this job is available for acceptance.
    /// </summary>
    public bool IsAvailable => Status == JobStatus.Available && ExpiresAt > DateTimeOffset.UtcNow;

    /// <summary>
    /// Whether this job has expired.
    /// </summary>
    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow && Status == JobStatus.Available;

    /// <summary>
    /// Time remaining until expiry.
    /// </summary>
    public TimeSpan? TimeRemaining => IsAvailable ? ExpiresAt - DateTimeOffset.UtcNow : null;

    /// <summary>
    /// Creates a cargo job.
    /// </summary>
    public static Job CreateCargoJob(
        Guid worldId,
        int departureAirportId,
        string departureIcao,
        int arrivalAirportId,
        string arrivalIcao,
        double distanceNm,
        Guid cargoTypeId,
        string cargoTypeName,
        int weightLbs,
        decimal payout,
        JobUrgency urgency,
        DateTimeOffset expiresAt)
    {
        return new Job
        {
            WorldId = worldId,
            DepartureAirportId = departureAirportId,
            DepartureIcao = departureIcao,
            ArrivalAirportId = arrivalAirportId,
            ArrivalIcao = arrivalIcao,
            DistanceNm = distanceNm,
            DistanceCategory = GetDistanceCategory(distanceNm),
            Type = JobType.Cargo,
            CargoTypeId = cargoTypeId,
            CargoType = cargoTypeName,
            WeightLbs = weightLbs,
            BasePayout = payout,
            Payout = payout,
            Urgency = urgency,
            ExpiresAt = expiresAt,
            EstimatedFlightTimeMinutes = (int)(distanceNm / 3), // Rough estimate
            Title = $"Cargo: {cargoTypeName}",
            Description = $"Transport {weightLbs:N0} lbs of {cargoTypeName} from {departureIcao} to {arrivalIcao}"
        };
    }

    /// <summary>
    /// Creates a passenger job.
    /// </summary>
    public static Job CreatePassengerJob(
        Guid worldId,
        int departureAirportId,
        string departureIcao,
        int arrivalAirportId,
        string arrivalIcao,
        double distanceNm,
        int passengerCount,
        PassengerClass passengerClass,
        decimal payout,
        JobUrgency urgency,
        DateTimeOffset expiresAt)
    {
        return new Job
        {
            WorldId = worldId,
            DepartureAirportId = departureAirportId,
            DepartureIcao = departureIcao,
            ArrivalAirportId = arrivalAirportId,
            ArrivalIcao = arrivalIcao,
            DistanceNm = distanceNm,
            DistanceCategory = GetDistanceCategory(distanceNm),
            Type = JobType.Passenger,
            PassengerCount = passengerCount,
            PassengerClass = passengerClass,
            BasePayout = payout,
            Payout = payout,
            Urgency = urgency,
            ExpiresAt = expiresAt,
            EstimatedFlightTimeMinutes = (int)(distanceNm / 3),
            Title = $"{passengerClass} Passengers",
            Description = $"Transport {passengerCount} {passengerClass} passengers from {departureIcao} to {arrivalIcao}"
        };
    }

    /// <summary>
    /// Accepts the job for a player.
    /// </summary>
    public void Accept(Guid userId, Guid playerWorldId)
    {
        if (Status != JobStatus.Available)
            throw new InvalidOperationException($"Job cannot be accepted in status {Status}");

        if (ExpiresAt <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException("Job has expired");

        AssignedToUserId = userId;
        AssignedToPlayerWorldId = playerWorldId;
        Status = JobStatus.Accepted;
        AcceptedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Starts the job (flight begins).
    /// </summary>
    public void Start()
    {
        if (Status != JobStatus.Accepted)
            throw new InvalidOperationException($"Job cannot be started in status {Status}");

        Status = JobStatus.InProgress;
        StartedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Completes the job successfully.
    /// </summary>
    public void Complete(decimal actualPayout)
    {
        if (Status != JobStatus.InProgress)
            throw new InvalidOperationException($"Job cannot be completed in status {Status}");

        Status = JobStatus.Completed;
        IsCompleted = true;
        CompletedAt = DateTimeOffset.UtcNow;
        ActualPayout = actualPayout;
    }

    /// <summary>
    /// Fails the job.
    /// </summary>
    public void Fail(string reason)
    {
        Status = JobStatus.Failed;
        IsFailed = true;
        FailureReason = reason;
        CompletedAt = DateTimeOffset.UtcNow;
        ActualPayout = 0;
    }

    /// <summary>
    /// Cancels the job.
    /// </summary>
    public void Cancel()
    {
        if (Status is JobStatus.Completed or JobStatus.Failed)
            throw new InvalidOperationException($"Job cannot be cancelled in status {Status}");

        Status = JobStatus.Cancelled;
        ActualPayout = 0;
    }

    private static DistanceCategory GetDistanceCategory(double distanceNm)
    {
        return distanceNm switch
        {
            < 50 => DistanceCategory.VeryShort,
            < 150 => DistanceCategory.Short,
            < 500 => DistanceCategory.Medium,
            < 1500 => DistanceCategory.Long,
            _ => DistanceCategory.UltraLong
        };
    }
}
