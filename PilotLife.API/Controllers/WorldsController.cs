using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using System.Security.Claims;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorldsController : ControllerBase
{
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<WorldsController> _logger;

    public WorldsController(PilotLifeDbContext context, ILogger<WorldsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all active worlds available for players to join.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorldResponse>>> GetWorlds()
    {
        var worlds = await _context.Worlds
            .Where(w => w.IsActive)
            .OrderByDescending(w => w.IsDefault)
            .ThenBy(w => w.Name)
            .Select(w => new WorldResponse
            {
                Id = w.Id,
                Name = w.Name,
                Slug = w.Slug,
                Description = w.Description,
                Difficulty = w.Difficulty.ToString(),
                StartingCapital = w.StartingCapital,
                JobPayoutMultiplier = w.JobPayoutMultiplier,
                AircraftPriceMultiplier = w.AircraftPriceMultiplier,
                MaintenanceCostMultiplier = w.MaintenanceCostMultiplier,
                IsDefault = w.IsDefault,
                MaxPlayers = w.MaxPlayers,
                CurrentPlayers = w.Players.Count(p => p.IsActive)
            })
            .ToListAsync();

        return Ok(worlds);
    }

    /// <summary>
    /// Get a specific world by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorldResponse>> GetWorld(Guid id)
    {
        var world = await _context.Worlds
            .Where(w => w.Id == id && w.IsActive)
            .Select(w => new WorldResponse
            {
                Id = w.Id,
                Name = w.Name,
                Slug = w.Slug,
                Description = w.Description,
                Difficulty = w.Difficulty.ToString(),
                StartingCapital = w.StartingCapital,
                JobPayoutMultiplier = w.JobPayoutMultiplier,
                AircraftPriceMultiplier = w.AircraftPriceMultiplier,
                MaintenanceCostMultiplier = w.MaintenanceCostMultiplier,
                IsDefault = w.IsDefault,
                MaxPlayers = w.MaxPlayers,
                CurrentPlayers = w.Players.Count(p => p.IsActive)
            })
            .FirstOrDefaultAsync();

        if (world == null)
        {
            return NotFound(new { message = "World not found" });
        }

        return Ok(world);
    }

    /// <summary>
    /// Get a specific world by slug.
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<WorldResponse>> GetWorldBySlug(string slug)
    {
        var world = await _context.Worlds
            .Where(w => w.Slug == slug && w.IsActive)
            .Select(w => new WorldResponse
            {
                Id = w.Id,
                Name = w.Name,
                Slug = w.Slug,
                Description = w.Description,
                Difficulty = w.Difficulty.ToString(),
                StartingCapital = w.StartingCapital,
                JobPayoutMultiplier = w.JobPayoutMultiplier,
                AircraftPriceMultiplier = w.AircraftPriceMultiplier,
                MaintenanceCostMultiplier = w.MaintenanceCostMultiplier,
                IsDefault = w.IsDefault,
                MaxPlayers = w.MaxPlayers,
                CurrentPlayers = w.Players.Count(p => p.IsActive)
            })
            .FirstOrDefaultAsync();

        if (world == null)
        {
            return NotFound(new { message = "World not found" });
        }

        return Ok(world);
    }

    /// <summary>
    /// Get the current user's progress in all worlds they've joined.
    /// </summary>
    [HttpGet("my-worlds")]
    public async Task<ActionResult<IEnumerable<PlayerWorldResponse>>> GetMyWorlds()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var playerWorlds = await _context.PlayerWorlds
            .Where(pw => pw.UserId == userId && pw.IsActive)
            .Include(pw => pw.World)
            .Include(pw => pw.CurrentAirport)
            .Include(pw => pw.HomeAirport)
            .Select(pw => new PlayerWorldResponse
            {
                Id = pw.Id,
                WorldId = pw.WorldId,
                WorldName = pw.World.Name,
                WorldSlug = pw.World.Slug,
                WorldDifficulty = pw.World.Difficulty.ToString(),
                Balance = pw.Balance,
                CreditScore = pw.CreditScore,
                ReputationScore = pw.ReputationScore,
                TotalFlights = pw.TotalFlights,
                TotalJobsCompleted = pw.TotalJobsCompleted,
                TotalFlightMinutes = pw.TotalFlightMinutes,
                TotalEarnings = pw.TotalEarnings,
                CurrentAirportId = pw.CurrentAirportId,
                CurrentAirportIdent = pw.CurrentAirport != null ? pw.CurrentAirport.Ident : null,
                HomeAirportId = pw.HomeAirportId,
                HomeAirportIdent = pw.HomeAirport != null ? pw.HomeAirport.Ident : null,
                JoinedAt = pw.JoinedAt,
                LastActiveAt = pw.LastActiveAt
            })
            .ToListAsync();

        return Ok(playerWorlds);
    }

    /// <summary>
    /// Join a world.
    /// </summary>
    [HttpPost("{id:guid}/join")]
    public async Task<ActionResult<PlayerWorldResponse>> JoinWorld(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var world = await _context.Worlds
            .Include(w => w.Players)
            .FirstOrDefaultAsync(w => w.Id == id && w.IsActive);

        if (world == null)
        {
            return NotFound(new { message = "World not found" });
        }

        // Check if user already joined this world
        var existingEntry = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.UserId == userId && pw.WorldId == id);

        if (existingEntry != null)
        {
            if (existingEntry.IsActive)
            {
                return BadRequest(new { message = "You have already joined this world" });
            }

            // Reactivate existing entry
            existingEntry.IsActive = true;
            existingEntry.LastActiveAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(await GetPlayerWorldResponse(existingEntry.Id));
        }

        // Check max players limit
        if (world.MaxPlayers > 0)
        {
            var activePlayers = world.Players.Count(p => p.IsActive);
            if (activePlayers >= world.MaxPlayers)
            {
                return BadRequest(new { message = "This world has reached its maximum player limit" });
            }
        }

        // Create new player world entry
        var playerWorld = new PlayerWorld
        {
            UserId = userId,
            WorldId = id,
            Balance = world.StartingCapital,
            CreditScore = 650,
            ReputationScore = 3.0m,
            JoinedAt = DateTimeOffset.UtcNow,
            LastActiveAt = DateTimeOffset.UtcNow
        };

        _context.PlayerWorlds.Add(playerWorld);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} joined world {WorldId} ({WorldName})", userId, id, world.Name);

        return Ok(await GetPlayerWorldResponse(playerWorld.Id));
    }

    /// <summary>
    /// Set home airport for a world.
    /// </summary>
    [HttpPost("my-worlds/{playerWorldId:guid}/set-home-airport")]
    public async Task<ActionResult<PlayerWorldResponse>> SetWorldHomeAirport(Guid playerWorldId, [FromBody] WorldSetHomeAirportRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId && pw.UserId == userId && pw.IsActive);

        if (playerWorld == null)
        {
            return NotFound(new { message = "Player world not found" });
        }

        var airport = await _context.Airports.FindAsync(request.AirportId);
        if (airport == null)
        {
            return BadRequest(new { message = "Airport not found" });
        }

        playerWorld.HomeAirportId = request.AirportId;
        if (playerWorld.CurrentAirportId == null)
        {
            playerWorld.CurrentAirportId = request.AirportId;
        }
        playerWorld.LastActiveAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(await GetPlayerWorldResponse(playerWorld.Id));
    }

    private async Task<PlayerWorldResponse> GetPlayerWorldResponse(Guid playerWorldId)
    {
        return await _context.PlayerWorlds
            .Where(pw => pw.Id == playerWorldId)
            .Include(pw => pw.World)
            .Include(pw => pw.CurrentAirport)
            .Include(pw => pw.HomeAirport)
            .Select(pw => new PlayerWorldResponse
            {
                Id = pw.Id,
                WorldId = pw.WorldId,
                WorldName = pw.World.Name,
                WorldSlug = pw.World.Slug,
                WorldDifficulty = pw.World.Difficulty.ToString(),
                Balance = pw.Balance,
                CreditScore = pw.CreditScore,
                ReputationScore = pw.ReputationScore,
                TotalFlights = pw.TotalFlights,
                TotalJobsCompleted = pw.TotalJobsCompleted,
                TotalFlightMinutes = pw.TotalFlightMinutes,
                TotalEarnings = pw.TotalEarnings,
                CurrentAirportId = pw.CurrentAirportId,
                CurrentAirportIdent = pw.CurrentAirport != null ? pw.CurrentAirport.Ident : null,
                HomeAirportId = pw.HomeAirportId,
                HomeAirportIdent = pw.HomeAirport != null ? pw.HomeAirport.Ident : null,
                JoinedAt = pw.JoinedAt,
                LastActiveAt = pw.LastActiveAt
            })
            .FirstAsync();
    }
}

public class WorldResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public decimal StartingCapital { get; set; }
    public decimal JobPayoutMultiplier { get; set; }
    public decimal AircraftPriceMultiplier { get; set; }
    public decimal MaintenanceCostMultiplier { get; set; }
    public bool IsDefault { get; set; }
    public int MaxPlayers { get; set; }
    public int CurrentPlayers { get; set; }
}

public class PlayerWorldResponse
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }
    public string WorldName { get; set; } = string.Empty;
    public string WorldSlug { get; set; } = string.Empty;
    public string WorldDifficulty { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public int CreditScore { get; set; }
    public decimal ReputationScore { get; set; }
    public int TotalFlights { get; set; }
    public int TotalJobsCompleted { get; set; }
    public int TotalFlightMinutes { get; set; }
    public decimal TotalEarnings { get; set; }
    public int? CurrentAirportId { get; set; }
    public string? CurrentAirportIdent { get; set; }
    public int? HomeAirportId { get; set; }
    public string? HomeAirportIdent { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset LastActiveAt { get; set; }
}

public class WorldSetHomeAirportRequest
{
    public int AirportId { get; set; }
}
