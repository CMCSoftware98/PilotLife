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

    // Navigation properties for owned items (will be added in later phases)
    // public ICollection<OwnedAircraft> OwnedAircraft { get; set; }
    // public ICollection<UserLicense> Licenses { get; set; }
    // public ICollection<Loan> Loans { get; set; }
}
