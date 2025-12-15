using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.API.DTOs;
using PilotLife.Application.Skills;
using PilotLife.Database.Data;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SkillsController : ControllerBase
{
    private readonly ISkillsService _skillsService;
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<SkillsController> _logger;

    public SkillsController(
        ISkillsService skillsService,
        PilotLifeDbContext context,
        ILogger<SkillsController> logger)
    {
        _skillsService = skillsService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all skills for a player in a world.
    /// </summary>
    [HttpGet("{worldId:guid}")]
    public async Task<ActionResult<IEnumerable<PlayerSkillResponse>>> GetAllSkills(Guid worldId)
    {
        var userId = GetUserId();
        var playerWorld = await GetPlayerWorldAsync(userId, worldId);

        if (playerWorld == null)
        {
            return NotFound(new { message = "Player not found in this world" });
        }

        var skills = await _skillsService.GetAllSkillsAsync(playerWorld.Id);
        return Ok(skills.Select(MapToResponse));
    }

    /// <summary>
    /// Gets a specific skill for a player.
    /// </summary>
    [HttpGet("{worldId:guid}/{skillType}")]
    public async Task<ActionResult<PlayerSkillResponse>> GetSkill(Guid worldId, string skillType)
    {
        var userId = GetUserId();
        var playerWorld = await GetPlayerWorldAsync(userId, worldId);

        if (playerWorld == null)
        {
            return NotFound(new { message = "Player not found in this world" });
        }

        if (!Enum.TryParse<SkillType>(skillType, out var parsedSkillType))
        {
            return BadRequest(new { message = "Invalid skill type" });
        }

        var skill = await _skillsService.GetSkillAsync(playerWorld.Id, parsedSkillType);

        if (skill == null)
        {
            return NotFound(new { message = "Skill not found" });
        }

        return Ok(MapToResponse(skill));
    }

    /// <summary>
    /// Gets XP history for a player's skills.
    /// </summary>
    [HttpGet("{worldId:guid}/history")]
    public async Task<ActionResult<IEnumerable<SkillXpEventResponse>>> GetXpHistory(
        Guid worldId,
        [FromQuery] string? skillType = null,
        [FromQuery] int limit = 50)
    {
        var userId = GetUserId();
        var playerWorld = await GetPlayerWorldAsync(userId, worldId);

        if (playerWorld == null)
        {
            return NotFound(new { message = "Player not found in this world" });
        }

        SkillType? parsedSkillType = null;
        if (!string.IsNullOrEmpty(skillType))
        {
            if (Enum.TryParse<SkillType>(skillType, out var parsed))
            {
                parsedSkillType = parsed;
            }
            else
            {
                return BadRequest(new { message = "Invalid skill type" });
            }
        }

        var history = await _skillsService.GetXpHistoryAsync(playerWorld.Id, parsedSkillType, limit);
        return Ok(history.Select(MapToXpEventResponse));
    }

    /// <summary>
    /// Gets the total skill level (sum of all skills) for a player.
    /// </summary>
    [HttpGet("{worldId:guid}/total")]
    public async Task<ActionResult<int>> GetTotalSkillLevel(Guid worldId)
    {
        var userId = GetUserId();
        var playerWorld = await GetPlayerWorldAsync(userId, worldId);

        if (playerWorld == null)
        {
            return NotFound(new { message = "Player not found in this world" });
        }

        var total = await _skillsService.GetTotalSkillLevelAsync(playerWorld.Id);
        return Ok(new { totalLevel = total });
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

    private static PlayerSkillResponse MapToResponse(PlayerSkillStatus status)
    {
        return new PlayerSkillResponse
        {
            SkillId = status.SkillId.ToString(),
            SkillType = status.SkillType.ToString(),
            SkillName = status.SkillName,
            Description = status.Description,
            CurrentXp = status.CurrentXp,
            Level = status.Level,
            LevelName = status.LevelName,
            XpForNextLevel = status.XpForNextLevel,
            XpForCurrentLevel = status.XpForCurrentLevel,
            ProgressToNextLevel = status.ProgressToNextLevel,
            IsMaxLevel = status.IsMaxLevel
        };
    }

    private static SkillXpEventResponse MapToXpEventResponse(Domain.Entities.SkillXpEvent e)
    {
        return new SkillXpEventResponse
        {
            Id = e.Id.ToString(),
            SkillType = e.PlayerSkill.SkillType.ToString(),
            XpGained = e.XpGained,
            ResultingXp = e.ResultingXp,
            ResultingLevel = e.ResultingLevel,
            CausedLevelUp = e.CausedLevelUp,
            Source = e.Source,
            Description = e.Description,
            OccurredAt = e.OccurredAt.ToString("O"),
            RelatedFlightId = e.RelatedFlightId?.ToString(),
            RelatedJobId = e.RelatedJobId?.ToString()
        };
    }
}
