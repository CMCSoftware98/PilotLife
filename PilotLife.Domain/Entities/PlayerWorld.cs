using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a player's state within a specific world.
/// All game progress (money, aircraft, licenses, etc.) is per-world.
/// </summary>
public class PlayerWorld : BaseEntity
{
    // Links
    /// <summary>
    /// The user account.
    /// </summary>
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// The world this player state belongs to.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    // Financial State
    /// <summary>
    /// Current account balance in the world's currency.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Credit score (300-850 range, like real credit scores).
    /// </summary>
    public int CreditScore { get; set; } = 650;

    // Experience & Statistics
    /// <summary>
    /// Total flight time in minutes.
    /// </summary>
    public int TotalFlightMinutes { get; set; }

    /// <summary>
    /// Total number of flights completed.
    /// </summary>
    public int TotalFlights { get; set; }

    /// <summary>
    /// Total jobs successfully completed.
    /// </summary>
    public int TotalJobsCompleted { get; set; }

    /// <summary>
    /// Total money earned from all sources.
    /// </summary>
    public decimal TotalEarnings { get; set; }

    /// <summary>
    /// Total money spent on all purchases.
    /// </summary>
    public decimal TotalSpent { get; set; }

    // Reputation
    /// <summary>
    /// Overall reputation score (0.0 to 5.0).
    /// </summary>
    public decimal ReputationScore { get; set; } = 3.0m;

    /// <summary>
    /// Number of on-time deliveries.
    /// </summary>
    public int OnTimeDeliveries { get; set; }

    /// <summary>
    /// Number of late deliveries.
    /// </summary>
    public int LateDeliveries { get; set; }

    /// <summary>
    /// Number of failed deliveries.
    /// </summary>
    public int FailedDeliveries { get; set; }

    // Violations
    /// <summary>
    /// Accumulated violation points (decay over time).
    /// </summary>
    public int ViolationPoints { get; set; }

    /// <summary>
    /// When the last violation occurred.
    /// </summary>
    public DateTimeOffset? LastViolationAt { get; set; }

    // Status
    /// <summary>
    /// Whether this player state is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the player joined this world.
    /// </summary>
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Last activity timestamp.
    /// </summary>
    public DateTimeOffset LastActiveAt { get; set; } = DateTimeOffset.UtcNow;

    // Current location
    /// <summary>
    /// Current airport where the player is located.
    /// </summary>
    public int? CurrentAirportId { get; set; }
    public Airport? CurrentAirport { get; set; }

    /// <summary>
    /// Home base airport.
    /// </summary>
    public int? HomeAirportId { get; set; }
    public Airport? HomeAirport { get; set; }

    // Navigation properties
    /// <summary>
    /// Loans taken by this player in this world.
    /// </summary>
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    /// <summary>
    /// Credit score history events.
    /// </summary>
    public ICollection<CreditScoreEvent> CreditScoreEvents { get; set; } = new List<CreditScoreEvent>();

    /// <summary>
    /// Player's skills and their levels.
    /// </summary>
    public ICollection<PlayerSkill> Skills { get; set; } = new List<PlayerSkill>();

    /// <summary>
    /// Reputation events history.
    /// </summary>
    public ICollection<ReputationEvent> ReputationEvents { get; set; } = new List<ReputationEvent>();

    // Calculated properties
    /// <summary>
    /// Gets the reputation level (1-5) based on the score.
    /// </summary>
    public int ReputationLevel => ReputationScore switch
    {
        < 1.0m => 1,
        < 2.0m => 2,
        < 3.0m => 3,
        < 4.0m => 4,
        _ => 5
    };

    /// <summary>
    /// Gets the reputation level name.
    /// </summary>
    public string ReputationLevelName => ReputationLevel switch
    {
        1 => "Unreliable",
        2 => "Novice",
        3 => "Standard",
        4 => "Trusted",
        5 => "Elite",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets the job completion rate as a percentage.
    /// </summary>
    public double JobCompletionRate
    {
        get
        {
            var totalJobs = OnTimeDeliveries + LateDeliveries + FailedDeliveries;
            return totalJobs > 0 ? (double)(OnTimeDeliveries + LateDeliveries) / totalJobs * 100 : 100;
        }
    }

    /// <summary>
    /// Gets the on-time delivery rate as a percentage.
    /// </summary>
    public double OnTimeRate
    {
        get
        {
            var totalDeliveries = OnTimeDeliveries + LateDeliveries;
            return totalDeliveries > 0 ? (double)OnTimeDeliveries / totalDeliveries * 100 : 100;
        }
    }
}
