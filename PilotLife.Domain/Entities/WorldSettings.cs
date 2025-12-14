using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Admin-adjustable settings for a world.
/// Extends the base World multipliers with additional gameplay settings.
/// </summary>
public class WorldSettings : BaseEntity
{
    /// <summary>
    /// The world these settings belong to.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    // Economy overrides (if different from World defaults)
    /// <summary>
    /// Override job payout multiplier (null = use World default).
    /// </summary>
    public decimal? JobPayoutMultiplierOverride { get; set; }

    /// <summary>
    /// Override aircraft price multiplier (null = use World default).
    /// </summary>
    public decimal? AircraftPriceMultiplierOverride { get; set; }

    /// <summary>
    /// Override maintenance cost multiplier (null = use World default).
    /// </summary>
    public decimal? MaintenanceCostMultiplierOverride { get; set; }

    // Feature toggles
    /// <summary>
    /// Whether new players can join this world.
    /// </summary>
    public bool AllowNewPlayers { get; set; } = true;

    /// <summary>
    /// Whether illegal cargo jobs are available.
    /// </summary>
    public bool AllowIllegalCargo { get; set; } = true;

    /// <summary>
    /// Whether player-to-player auctions are enabled.
    /// </summary>
    public bool EnableAuctions { get; set; } = true;

    /// <summary>
    /// Whether AI crew/workers feature is enabled.
    /// </summary>
    public bool EnableAICrews { get; set; } = true;

    /// <summary>
    /// Whether aircraft rental feature is enabled.
    /// </summary>
    public bool EnableAircraftRental { get; set; } = true;

    // Limits
    /// <summary>
    /// Maximum aircraft a player can own (0 = unlimited).
    /// </summary>
    public int MaxAircraftPerPlayer { get; set; }

    /// <summary>
    /// Maximum active loans a player can have (0 = unlimited).
    /// </summary>
    public int MaxLoansPerPlayer { get; set; } = 3;

    /// <summary>
    /// Maximum workers a player can hire (0 = unlimited).
    /// </summary>
    public int MaxWorkersPerPlayer { get; set; } = 10;

    /// <summary>
    /// Maximum active jobs a player can have at once (0 = unlimited).
    /// </summary>
    public int MaxActiveJobsPerPlayer { get; set; } = 5;

    // Moderation
    /// <summary>
    /// Whether players need admin approval to join.
    /// </summary>
    public bool RequireApprovalToJoin { get; set; }

    /// <summary>
    /// Whether in-game chat is enabled.
    /// </summary>
    public bool EnableChat { get; set; } = true;

    /// <summary>
    /// Whether players can report others.
    /// </summary>
    public bool EnableReporting { get; set; } = true;

    // Audit
    /// <summary>
    /// User who last modified these settings.
    /// </summary>
    public Guid? LastModifiedByUserId { get; set; }
    public User? LastModifiedByUser { get; set; }
}
