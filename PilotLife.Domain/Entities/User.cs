using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a user account in the system.
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

    // Flight sim properties
    public int? CurrentAirportId { get; set; }
    public Airport? CurrentAirport { get; set; }
    public int? HomeAirportId { get; set; }
    public Airport? HomeAirport { get; set; }
    public decimal Balance { get; set; }
    public int TotalFlightMinutes { get; set; }

    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
