using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.API.Services;
using PilotLife.Database.Data;
using PilotLife.Database.Entities;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly PilotLifeDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        PilotLifeDbContext context,
        IJwtService jwtService,
        JwtSettings jwtSettings,
        ILogger<AuthController> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
        {
            return BadRequest(new { message = "Email already registered" });
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLower(),
            PasswordHash = HashPassword(request.Password),
            ExperienceLevel = request.ExperienceLevel,
            NewsletterSubscribed = request.NewsletterSubscribed,
            CreatedAt = DateTime.UtcNow,
            Balance = 10000m,
            TotalFlightMinutes = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var tokens = _jwtService.GenerateTokens(user);
        await SaveRefreshToken(user.Id, tokens.RefreshToken);

        _logger.LogInformation("New user registered: {Email}", user.Email);

        return Ok(CreateAuthResponse(user, tokens));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.CurrentAirport)
            .Include(u => u.HomeAirport)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var tokens = _jwtService.GenerateTokens(user);
        await SaveRefreshToken(user.Id, tokens.RefreshToken);

        _logger.LogInformation("User logged in: {Email}", user.Email);

        return Ok(CreateAuthResponse(user, tokens));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.CurrentAirport)
            .Include(rt => rt.User)
                .ThenInclude(u => u.HomeAirport)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken == null)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        if (!storedToken.IsActive)
        {
            return Unauthorized(new { message = "Refresh token is expired or revoked" });
        }

        var user = storedToken.User;
        var newTokens = _jwtService.GenerateTokens(user);

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByToken = newTokens.RefreshToken;

        await SaveRefreshToken(user.Id, newTokens.RefreshToken);

        _logger.LogInformation("Token refreshed for user: {Email}", user.Email);

        return Ok(CreateAuthResponse(user, newTokens));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken != null && storedToken.IsActive)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var user = await _context.Users
            .Include(u => u.CurrentAirport)
            .Include(u => u.HomeAirport)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(CreateUserResponse(user));
    }

    [HttpPost("set-home-airport")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> SetHomeAirport([FromBody] SetHomeAirportRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var airport = await _context.Airports
            .FirstOrDefaultAsync(a => a.Id == request.AirportId);

        if (airport == null)
        {
            return NotFound(new { message = "Airport not found" });
        }

        user.HomeAirportId = airport.Id;
        user.CurrentAirportId = airport.Id;
        await _context.SaveChangesAsync();

        // Reload with includes
        user = await _context.Users
            .Include(u => u.CurrentAirport)
            .Include(u => u.HomeAirport)
            .FirstOrDefaultAsync(u => u.Id == userId);

        _logger.LogInformation("User {Email} set home airport to {Airport}", user!.Email, airport.Ident);

        return Ok(CreateUserResponse(user));
    }

    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    private async Task SaveRefreshToken(Guid userId, string token)
    {
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }

    private static AuthResponse CreateAuthResponse(User user, TokenResponse tokens)
    {
        return new AuthResponse
        {
            User = CreateUserResponse(user),
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ExpiresAt = tokens.ExpiresAt
        };
    }

    private static UserResponse CreateUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ExperienceLevel = user.ExperienceLevel,
            Balance = user.Balance,
            TotalFlightMinutes = user.TotalFlightMinutes,
            CurrentAirportId = user.CurrentAirportId,
            CurrentAirport = user.CurrentAirport != null ? new AirportResponse
            {
                Id = user.CurrentAirport.Id,
                Ident = user.CurrentAirport.Ident,
                Name = user.CurrentAirport.Name,
                IataCode = user.CurrentAirport.IataCode,
                Latitude = user.CurrentAirport.Latitude,
                Longitude = user.CurrentAirport.Longitude
            } : null,
            HomeAirportId = user.HomeAirportId,
            HomeAirport = user.HomeAirport != null ? new AirportResponse
            {
                Id = user.HomeAirport.Id,
                Ident = user.HomeAirport.Ident,
                Name = user.HomeAirport.Name,
                IataCode = user.HomeAirport.IataCode,
                Latitude = user.HomeAirport.Latitude,
                Longitude = user.HomeAirport.Longitude
            } : null
        };
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}

public record RegisterRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? ExperienceLevel { get; init; }
    public bool NewsletterSubscribed { get; init; }
}

public record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}

public record LogoutRequest
{
    public required string RefreshToken { get; init; }
}

public record AuthResponse
{
    public required UserResponse User { get; init; }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }
}

public record UserResponse
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? ExperienceLevel { get; init; }
    public decimal Balance { get; init; }
    public int TotalFlightMinutes { get; init; }
    public int? CurrentAirportId { get; init; }
    public AirportResponse? CurrentAirport { get; init; }
    public int? HomeAirportId { get; init; }
    public AirportResponse? HomeAirport { get; init; }
}

public record AirportResponse
{
    public int Id { get; init; }
    public required string Ident { get; init; }
    public required string Name { get; init; }
    public string? IataCode { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}

public record SetHomeAirportRequest
{
    public int AirportId { get; init; }
}
