using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.API.DTOs;
using PilotLife.Application.Reputation;
using PilotLife.Database.Data;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReputationController : ControllerBase
{
    private readonly IReputationService _reputationService;
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<ReputationController> _logger;

    public ReputationController(
        IReputationService reputationService,
        PilotLifeDbContext context,
        ILogger<ReputationController> logger)
    {
        _reputationService = reputationService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current reputation status for a player in a world.
    /// </summary>
    [HttpGet("{worldId:guid}")]
    public async Task<ActionResult<ReputationStatusResponse>> GetReputationStatus(Guid worldId)
    {
        var userId = GetUserId();
        var playerWorld = await GetPlayerWorldAsync(userId, worldId);

        if (playerWorld == null)
        {
            return NotFound(new { message = "Player not found in this world" });
        }

        var status = await _reputationService.GetReputationStatusAsync(playerWorld.Id);
        return Ok(MapToResponse(status));
    }

    /// <summary>
    /// Gets the reputation history for a player.
    /// </summary>
    [HttpGet("{worldId:guid}/history")]
    public async Task<ActionResult<IEnumerable<ReputationEventResponse>>> GetReputationHistory(
        Guid worldId,
        [FromQuery] int limit = 50)
    {
        var userId = GetUserId();
        var playerWorld = await GetPlayerWorldAsync(userId, worldId);

        if (playerWorld == null)
        {
            return NotFound(new { message = "Player not found in this world" });
        }

        var history = await _reputationService.GetReputationHistoryAsync(playerWorld.Id, limit);
        return Ok(history.Select(MapToEventResponse));
    }

    /// <summary>
    /// Gets the payout bonus percentage for the player's current reputation level.
    /// </summary>
    [HttpGet("{worldId:guid}/bonus")]
    public async Task<ActionResult<decimal>> GetPayoutBonus(Guid worldId)
    {
        var userId = GetUserId();
        var playerWorld = await GetPlayerWorldAsync(userId, worldId);

        if (playerWorld == null)
        {
            return NotFound(new { message = "Player not found in this world" });
        }

        var bonus = await _reputationService.GetPayoutBonusAsync(playerWorld.Id);
        return Ok(new { bonusPercent = bonus });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private async Task<Domain.Entities.PlayerWorld?> GetPlayerWorldAsync(Guid userId, Guid worldId)
    {
        return await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.UserId == userId && pw.WorldId == worldId);
    }

    private static ReputationStatusResponse MapToResponse(ReputationStatus status)
    {
        return new ReputationStatusResponse
        {
            PlayerWorldId = status.PlayerWorldId.ToString(),
            Score = status.Score,
            Level = status.Level,
            LevelName = status.LevelName,
            ProgressToNextLevel = status.ProgressToNextLevel,
            OnTimeDeliveries = status.OnTimeDeliveries,
            LateDeliveries = status.LateDeliveries,
            FailedDeliveries = status.FailedDeliveries,
            JobCompletionRate = status.JobCompletionRate,
            OnTimeRate = status.OnTimeRate,
            PayoutBonus = status.PayoutBonus,
            Benefits = status.Benefits.Select(b => new ReputationBenefitResponse
            {
                Name = b.Name,
                Description = b.Description,
                IsUnlocked = b.IsUnlocked,
                RequiredLevel = b.RequiredLevel
            }).ToList()
        };
    }

    private static ReputationEventResponse MapToEventResponse(Domain.Entities.ReputationEvent e)
    {
        return new ReputationEventResponse
        {
            Id = e.Id.ToString(),
            EventType = e.EventType.ToString(),
            PointChange = e.PointChange,
            ResultingScore = e.ResultingScore,
            Description = e.Description,
            OccurredAt = e.OccurredAt.ToString("O"),
            RelatedJobId = e.RelatedJobId?.ToString(),
            RelatedFlightId = e.RelatedFlightId?.ToString()
        };
    }
}
