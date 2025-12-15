using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.API.DTOs;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HangarController : ControllerBase
{
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<HangarController> _logger;

    public HangarController(PilotLifeDbContext context, ILogger<HangarController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET /api/hangar/my-aircraft
    [HttpGet("my-aircraft")]
    public async Task<ActionResult<IEnumerable<OwnedAircraftResponse>>> GetMyAircraft(
        [FromQuery] string? worldId,
        [FromQuery] string? locationIcao)
    {
        var userId = GetUserId();

        var query = _context.OwnedAircraft
            .Include(o => o.Aircraft)
            .Include(o => o.Owner)
            .Where(o => o.Owner.UserId == userId);

        if (Guid.TryParse(worldId, out var wId))
        {
            query = query.Where(o => o.WorldId == wId);
        }

        if (!string.IsNullOrEmpty(locationIcao))
        {
            query = query.Where(o => o.CurrentLocationIcao == locationIcao);
        }

        var aircraft = await query.OrderByDescending(o => o.PurchasedAt).ToListAsync();

        var response = aircraft.Select(MapToResponse);
        return Ok(response);
    }

    // GET /api/hangar/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OwnedAircraftResponse>> GetAircraft(Guid id)
    {
        var userId = GetUserId();

        var aircraft = await _context.OwnedAircraft
            .Include(o => o.Aircraft)
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.Id == id && o.Owner.UserId == userId);

        if (aircraft == null)
        {
            return NotFound(new { message = "Aircraft not found" });
        }

        return Ok(MapToResponse(aircraft));
    }

    // PUT /api/hangar/{id}/nickname
    [HttpPut("{id:guid}/nickname")]
    public async Task<ActionResult<OwnedAircraftResponse>> UpdateNickname(Guid id, [FromBody] UpdateNicknameRequest request)
    {
        var userId = GetUserId();

        var aircraft = await _context.OwnedAircraft
            .Include(o => o.Aircraft)
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.Id == id && o.Owner.UserId == userId);

        if (aircraft == null)
        {
            return NotFound(new { message = "Aircraft not found" });
        }

        aircraft.Nickname = request.Nickname;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated nickname for aircraft {AircraftId} to '{Nickname}'",
            userId, id, request.Nickname);

        return Ok(MapToResponse(aircraft));
    }

    // POST /api/hangar/{id}/list-for-sale
    [HttpPost("{id:guid}/list-for-sale")]
    public async Task<ActionResult<OwnedAircraftResponse>> ListForSale(Guid id, [FromBody] ListForSaleRequest request)
    {
        var userId = GetUserId();

        var aircraft = await _context.OwnedAircraft
            .Include(o => o.Aircraft)
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.Id == id && o.Owner.UserId == userId);

        if (aircraft == null)
        {
            return NotFound(new { message = "Aircraft not found" });
        }

        if (aircraft.IsInUse)
        {
            return BadRequest(new { message = "Cannot list aircraft that is currently in use" });
        }

        if (aircraft.IsInMaintenance)
        {
            return BadRequest(new { message = "Cannot list aircraft that is currently in maintenance" });
        }

        aircraft.IsListedForSale = true;
        aircraft.AskingPrice = request.AskingPrice;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} listed aircraft {AircraftId} for sale at {Price}",
            userId, id, request.AskingPrice);

        return Ok(MapToResponse(aircraft));
    }

    // POST /api/hangar/{id}/cancel-sale
    [HttpPost("{id:guid}/cancel-sale")]
    public async Task<ActionResult<OwnedAircraftResponse>> CancelSale(Guid id)
    {
        var userId = GetUserId();

        var aircraft = await _context.OwnedAircraft
            .Include(o => o.Aircraft)
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.Id == id && o.Owner.UserId == userId);

        if (aircraft == null)
        {
            return NotFound(new { message = "Aircraft not found" });
        }

        if (!aircraft.IsListedForSale)
        {
            return BadRequest(new { message = "Aircraft is not listed for sale" });
        }

        aircraft.IsListedForSale = false;
        aircraft.AskingPrice = null;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} cancelled sale listing for aircraft {AircraftId}",
            userId, id);

        return Ok(MapToResponse(aircraft));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private static OwnedAircraftResponse MapToResponse(OwnedAircraft aircraft)
    {
        return new OwnedAircraftResponse
        {
            Id = aircraft.Id.ToString(),
            WorldId = aircraft.WorldId.ToString(),
            PlayerWorldId = aircraft.PlayerWorldId.ToString(),
            AircraftId = aircraft.AircraftId.ToString(),
            Aircraft = aircraft.Aircraft != null ? new MarketplaceAircraftResponse
            {
                Id = aircraft.Aircraft.Id.ToString(),
                Title = aircraft.Aircraft.Title,
                AtcType = aircraft.Aircraft.AtcType,
                AtcModel = aircraft.Aircraft.AtcModel,
                Category = aircraft.Aircraft.Category,
                EngineType = aircraft.Aircraft.EngineType,
                EngineTypeStr = aircraft.Aircraft.EngineTypeStr,
                NumberOfEngines = aircraft.Aircraft.NumberOfEngines,
                MaxGrossWeightLbs = aircraft.Aircraft.MaxGrossWeightLbs,
                EmptyWeightLbs = aircraft.Aircraft.EmptyWeightLbs,
                CruiseSpeedKts = aircraft.Aircraft.CruiseSpeedKts,
                SimulatorVersion = aircraft.Aircraft.SimulatorVersion,
                IsApproved = aircraft.Aircraft.IsApproved
            } : null,
            Registration = aircraft.Registration,
            Nickname = aircraft.Nickname,
            Condition = aircraft.Condition,
            TotalFlightMinutes = aircraft.TotalFlightMinutes,
            TotalFlightHours = aircraft.TotalFlightMinutes / 60.0,
            TotalCycles = aircraft.TotalCycles,
            HoursSinceLastInspection = aircraft.HoursSinceLastInspection,
            CurrentLocationIcao = aircraft.CurrentLocationIcao,
            IsAirworthy = aircraft.IsAirworthy,
            IsInMaintenance = aircraft.IsInMaintenance,
            IsInUse = aircraft.IsInUse,
            IsListedForSale = aircraft.IsListedForSale,
            HasWarranty = aircraft.HasWarranty,
            WarrantyExpiresAt = aircraft.WarrantyExpiresAt?.ToString("O"),
            HasInsurance = aircraft.HasInsurance,
            InsuranceExpiresAt = aircraft.InsuranceExpiresAt?.ToString("O"),
            PurchasePrice = aircraft.PurchasePrice,
            PurchasedAt = aircraft.PurchasedAt.ToString("O"),
            EstimatedValue = aircraft.EstimatedValue(aircraft.PurchasePrice)
        };
    }
}
