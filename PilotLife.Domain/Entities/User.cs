using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a user account in the system.
/// User accounts are global; game progress is stored in PlayerWorld per world.
/// </summary>
public class User : BaseEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? ExperienceLevel { get; set; }
    public bool EmailVerified { get; set; }
    public bool NewsletterSubscribed { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    // Legacy flight sim properties (kept for backwards compatibility)
    // New code should use PlayerWorld instead
    public int? CurrentAirportId { get; set; }
    public Airport? CurrentAirport { get; set; }
    public int? HomeAirportId { get; set; }
    public Airport? HomeAirport { get; set; }
    public decimal Balance { get; set; }
    public int TotalFlightMinutes { get; set; }

    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// Player's progress in each world they've joined.
    /// </summary>
    public ICollection<PlayerWorld> PlayerWorlds { get; set; } = new List<PlayerWorld>();

    /// <summary>
    /// Roles assigned to this user.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Tracked flights for this user.
    /// </summary>
    public ICollection<TrackedFlight> TrackedFlights { get; set; } = new List<TrackedFlight>();

    /// <summary>
    /// Full name of the user.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}
