using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PilotLife.API.Services.Licenses;
using PilotLife.Database.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LicensesController : ControllerBase
{
    private readonly LicenseService _licenseService;
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<LicensesController> _logger;

    public LicensesController(
        LicenseService licenseService,
        PilotLifeDbContext context,
        ILogger<LicensesController> logger)
    {
        _licenseService = licenseService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all available license types.
    /// </summary>
    [HttpGet("types")]
    [AllowAnonymous]
    public async Task<ActionResult<List<LicenseTypeDto>>> GetLicenseTypes()
    {
        var types = await _licenseService.GetLicenseTypesAsync();
        return Ok(types.Select(lt => new LicenseTypeDto
        {
            Id = lt.Id,
            Code = lt.Code,
            Name = lt.Name,
            Description = lt.Description,
            Category = lt.Category.ToString(),
            BaseExamCost = lt.BaseExamCost,
            ExamDurationMinutes = lt.ExamDurationMinutes,
            PassingScore = lt.PassingScore,
            RequiredAircraftCategory = lt.RequiredAircraftCategory?.ToString(),
            ValidityGameDays = lt.ValidityGameDays,
            BaseRenewalCost = lt.BaseRenewalCost,
            PrerequisiteLicenses = string.IsNullOrEmpty(lt.PrerequisiteLicensesJson)
                ? new List<string>()
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(lt.PrerequisiteLicensesJson) ?? new List<string>(),
            DisplayOrder = lt.DisplayOrder
        }).ToList());
    }

    /// <summary>
    /// Gets the license shop for the current player.
    /// </summary>
    [HttpGet("shop/{worldId}")]
    public async Task<ActionResult<List<LicenseShopItemDto>>> GetLicenseShop(Guid worldId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.UserId == userId && pw.WorldId == worldId);

        if (playerWorld == null)
            return NotFound(new { message = "Player not found in this world" });

        var shopItems = await _licenseService.GetLicenseShopAsync(playerWorld.Id);

        return Ok(shopItems.Select(item => new LicenseShopItemDto
        {
            LicenseType = new LicenseTypeDto
            {
                Id = item.LicenseType.Id,
                Code = item.LicenseType.Code,
                Name = item.LicenseType.Name,
                Description = item.LicenseType.Description,
                Category = item.LicenseType.Category.ToString(),
                BaseExamCost = item.LicenseType.BaseExamCost,
                ExamDurationMinutes = item.LicenseType.ExamDurationMinutes,
                PassingScore = item.LicenseType.PassingScore,
                RequiredAircraftCategory = item.LicenseType.RequiredAircraftCategory?.ToString(),
                ValidityGameDays = item.LicenseType.ValidityGameDays,
                BaseRenewalCost = item.LicenseType.BaseRenewalCost,
                DisplayOrder = item.LicenseType.DisplayOrder
            },
            IsOwned = item.IsOwned,
            ExpiresAt = item.ExpiresAt,
            CanTakeExam = item.CanTakeExam,
            HasPrerequisites = item.HasPrerequisites,
            MissingPrerequisites = item.MissingPrerequisites,
            IsOnCooldown = item.IsOnCooldown,
            CooldownEndsAt = item.CooldownEndsAt,
            ExamCost = item.ExamCost,
            RenewalCost = item.RenewalCost,
            CanRenew = item.CanRenew
        }).ToList());
    }

    /// <summary>
    /// Gets the current player's licenses.
    /// </summary>
    [HttpGet("my/{worldId}")]
    public async Task<ActionResult<List<UserLicenseDto>>> GetMyLicenses(Guid worldId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.UserId == userId && pw.WorldId == worldId);

        if (playerWorld == null)
            return NotFound(new { message = "Player not found in this world" });

        var licenses = await _licenseService.GetPlayerLicensesAsync(playerWorld.Id);

        return Ok(licenses.Select(ul => new UserLicenseDto
        {
            Id = ul.Id,
            LicenseCode = ul.LicenseType.Code,
            LicenseName = ul.LicenseType.Name,
            Category = ul.LicenseType.Category.ToString(),
            EarnedAt = ul.EarnedAt,
            ExpiresAt = ul.ExpiresAt,
            IsValid = ul.IsValid,
            IsRevoked = ul.IsRevoked,
            ExamScore = ul.ExamScore,
            ExamAttempts = ul.ExamAttempts,
            TotalPaid = ul.TotalPaid,
            RenewalCount = ul.RenewalCount,
            IsExpired = ul.ExpiresAt.HasValue && ul.ExpiresAt < DateTimeOffset.UtcNow,
            DaysUntilExpiry = ul.ExpiresAt.HasValue
                ? (int)(ul.ExpiresAt.Value - DateTimeOffset.UtcNow).TotalDays
                : (int?)null
        }).ToList());
    }

    /// <summary>
    /// Checks if the player has a specific license.
    /// </summary>
    [HttpGet("check/{worldId}/{licenseCode}")]
    public async Task<ActionResult<LicenseCheckResult>> CheckLicense(Guid worldId, string licenseCode)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.UserId == userId && pw.WorldId == worldId);

        if (playerWorld == null)
            return NotFound(new { message = "Player not found in this world" });

        var hasLicense = await _licenseService.HasValidLicenseAsync(playerWorld.Id, licenseCode);

        return Ok(new LicenseCheckResult
        {
            LicenseCode = licenseCode,
            HasValidLicense = hasLicense
        });
    }

    /// <summary>
    /// Renews a license.
    /// </summary>
    [HttpPost("renew/{worldId}/{licenseId}")]
    public async Task<ActionResult<RenewalResult>> RenewLicense(Guid worldId, Guid licenseId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.UserId == userId && pw.WorldId == worldId);

        if (playerWorld == null)
            return NotFound(new { message = "Player not found in this world" });

        var (success, message, cost) = await _licenseService.RenewLicenseAsync(playerWorld.Id, licenseId);

        if (!success)
            return BadRequest(new { message });

        return Ok(new RenewalResult
        {
            Success = true,
            Message = message,
            Cost = cost
        });
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        return userId;
    }
}

#region DTOs

public class LicenseTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal BaseExamCost { get; set; }
    public int ExamDurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public string? RequiredAircraftCategory { get; set; }
    public int? ValidityGameDays { get; set; }
    public decimal? BaseRenewalCost { get; set; }
    public List<string> PrerequisiteLicenses { get; set; } = new();
    public int DisplayOrder { get; set; }
}

public class LicenseShopItemDto
{
    public LicenseTypeDto LicenseType { get; set; } = null!;
    public bool IsOwned { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool CanTakeExam { get; set; }
    public bool HasPrerequisites { get; set; }
    public List<string> MissingPrerequisites { get; set; } = new();
    public bool IsOnCooldown { get; set; }
    public DateTimeOffset? CooldownEndsAt { get; set; }
    public decimal ExamCost { get; set; }
    public decimal? RenewalCost { get; set; }
    public bool CanRenew { get; set; }
}

public class UserLicenseDto
{
    public Guid Id { get; set; }
    public string LicenseCode { get; set; } = string.Empty;
    public string LicenseName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTimeOffset EarnedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsValid { get; set; }
    public bool IsRevoked { get; set; }
    public int ExamScore { get; set; }
    public int ExamAttempts { get; set; }
    public decimal TotalPaid { get; set; }
    public int RenewalCount { get; set; }
    public bool IsExpired { get; set; }
    public int? DaysUntilExpiry { get; set; }
}

public class LicenseCheckResult
{
    public string LicenseCode { get; set; } = string.Empty;
    public bool HasValidLicense { get; set; }
}

public class RenewalResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal? Cost { get; set; }
}

#endregion
