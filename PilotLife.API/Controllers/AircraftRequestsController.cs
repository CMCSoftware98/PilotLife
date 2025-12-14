using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Database.Entities;
using System.Security.Claims;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AircraftRequestsController : ControllerBase
{
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<AircraftRequestsController> _logger;

    public AircraftRequestsController(
        PilotLifeDbContext context,
        ILogger<AircraftRequestsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AircraftRequestResponse>>> GetRequests(
        [FromQuery] AircraftRequestStatus? status = null)
    {
        var query = _context.AircraftRequests
            .Include(r => r.RequestedByUser)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(requests.Select(ToResponse));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AircraftRequestResponse>> GetRequest(Guid id)
    {
        var request = await _context.AircraftRequests
            .Include(r => r.RequestedByUser)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound(new { message = "Aircraft request not found" });
        }

        return Ok(ToResponse(request));
    }

    [HttpPost]
    public async Task<ActionResult<AircraftRequestResponse>> CreateRequest(
        [FromBody] CreateAircraftRequestRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        // Check if a request for this aircraft already exists
        var existingRequest = await _context.AircraftRequests
            .FirstOrDefaultAsync(r => r.AircraftTitle == request.AircraftTitle);

        if (existingRequest != null)
        {
            return BadRequest(new { message = "An aircraft request with this title already exists" });
        }

        // Check if aircraft already exists in database
        var existingAircraft = await _context.Aircraft
            .FirstOrDefaultAsync(a => a.Title == request.AircraftTitle);

        if (existingAircraft != null)
        {
            return BadRequest(new { message = "This aircraft already exists in the database" });
        }

        var aircraftRequest = new AircraftRequest
        {
            AircraftTitle = request.AircraftTitle,
            AtcType = request.AtcType,
            AtcModel = request.AtcModel,
            Category = request.Category,
            EngineType = request.EngineType,
            EngineTypeStr = request.EngineTypeStr,
            NumberOfEngines = request.NumberOfEngines,
            MaxGrossWeightLbs = request.MaxGrossWeightLbs,
            EmptyWeightLbs = request.EmptyWeightLbs,
            CruiseSpeedKts = request.CruiseSpeedKts,
            SimulatorVersion = request.SimulatorVersion,
            RequestedByUserId = userId.Value,
            Status = AircraftRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.AircraftRequests.Add(aircraftRequest);
        await _context.SaveChangesAsync();

        // Reload with includes
        aircraftRequest = await _context.AircraftRequests
            .Include(r => r.RequestedByUser)
            .FirstOrDefaultAsync(r => r.Id == aircraftRequest.Id);

        _logger.LogInformation("Aircraft request created: {Title} by user {UserId}",
            request.AircraftTitle, userId);

        return CreatedAtAction(nameof(GetRequest), new { id = aircraftRequest!.Id }, ToResponse(aircraftRequest));
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<AircraftRequestResponse>> ApproveRequest(
        Guid id,
        [FromBody] ReviewAircraftRequestRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var aircraftRequest = await _context.AircraftRequests
            .Include(r => r.RequestedByUser)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (aircraftRequest == null)
        {
            return NotFound(new { message = "Aircraft request not found" });
        }

        if (aircraftRequest.Status != AircraftRequestStatus.Pending)
        {
            return BadRequest(new { message = "Request has already been reviewed" });
        }

        // Create the aircraft
        var aircraft = new Aircraft
        {
            Title = aircraftRequest.AircraftTitle,
            AtcType = aircraftRequest.AtcType,
            AtcModel = aircraftRequest.AtcModel,
            Category = aircraftRequest.Category,
            EngineType = aircraftRequest.EngineType,
            EngineTypeStr = aircraftRequest.EngineTypeStr,
            NumberOfEngines = aircraftRequest.NumberOfEngines,
            MaxGrossWeightLbs = aircraftRequest.MaxGrossWeightLbs,
            EmptyWeightLbs = aircraftRequest.EmptyWeightLbs,
            CruiseSpeedKts = aircraftRequest.CruiseSpeedKts,
            SimulatorVersion = aircraftRequest.SimulatorVersion,
            IsApproved = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Aircraft.Add(aircraft);

        aircraftRequest.Status = AircraftRequestStatus.Approved;
        aircraftRequest.ReviewNotes = request.ReviewNotes;
        aircraftRequest.ReviewedByUserId = userId;
        aircraftRequest.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Aircraft request approved: {Title} by user {UserId}",
            aircraftRequest.AircraftTitle, userId);

        return Ok(ToResponse(aircraftRequest));
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<AircraftRequestResponse>> RejectRequest(
        Guid id,
        [FromBody] ReviewAircraftRequestRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var aircraftRequest = await _context.AircraftRequests
            .Include(r => r.RequestedByUser)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (aircraftRequest == null)
        {
            return NotFound(new { message = "Aircraft request not found" });
        }

        if (aircraftRequest.Status != AircraftRequestStatus.Pending)
        {
            return BadRequest(new { message = "Request has already been reviewed" });
        }

        aircraftRequest.Status = AircraftRequestStatus.Rejected;
        aircraftRequest.ReviewNotes = request.ReviewNotes;
        aircraftRequest.ReviewedByUserId = userId;
        aircraftRequest.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Aircraft request rejected: {Title} by user {UserId}",
            aircraftRequest.AircraftTitle, userId);

        return Ok(ToResponse(aircraftRequest));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRequest(Guid id)
    {
        var aircraftRequest = await _context.AircraftRequests
            .FirstOrDefaultAsync(r => r.Id == id);

        if (aircraftRequest == null)
        {
            return NotFound(new { message = "Aircraft request not found" });
        }

        _context.AircraftRequests.Remove(aircraftRequest);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Aircraft request deleted: {Title}", aircraftRequest.AircraftTitle);

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }

    private static AircraftRequestResponse ToResponse(AircraftRequest request)
    {
        return new AircraftRequestResponse
        {
            Id = request.Id,
            AircraftTitle = request.AircraftTitle,
            AtcType = request.AtcType,
            AtcModel = request.AtcModel,
            Category = request.Category,
            EngineType = request.EngineType,
            EngineTypeStr = request.EngineTypeStr,
            NumberOfEngines = request.NumberOfEngines,
            MaxGrossWeightLbs = request.MaxGrossWeightLbs,
            EmptyWeightLbs = request.EmptyWeightLbs,
            CruiseSpeedKts = request.CruiseSpeedKts,
            SimulatorVersion = request.SimulatorVersion,
            Status = request.Status.ToString(),
            ReviewNotes = request.ReviewNotes,
            RequestedByUserName = request.RequestedByUser != null
                ? $"{request.RequestedByUser.FirstName} {request.RequestedByUser.LastName}"
                : null,
            CreatedAt = request.CreatedAt,
            ReviewedAt = request.ReviewedAt
        };
    }
}

public record CreateAircraftRequestRequest
{
    public required string AircraftTitle { get; init; }
    public string? AtcType { get; init; }
    public string? AtcModel { get; init; }
    public string? Category { get; init; }
    public int EngineType { get; init; }
    public string? EngineTypeStr { get; init; }
    public int NumberOfEngines { get; init; }
    public double MaxGrossWeightLbs { get; init; }
    public double EmptyWeightLbs { get; init; }
    public double CruiseSpeedKts { get; init; }
    public string? SimulatorVersion { get; init; }
}

public record ReviewAircraftRequestRequest
{
    public string? ReviewNotes { get; init; }
}

public record AircraftRequestResponse
{
    public Guid Id { get; init; }
    public required string AircraftTitle { get; init; }
    public string? AtcType { get; init; }
    public string? AtcModel { get; init; }
    public string? Category { get; init; }
    public int EngineType { get; init; }
    public string? EngineTypeStr { get; init; }
    public int NumberOfEngines { get; init; }
    public double MaxGrossWeightLbs { get; init; }
    public double EmptyWeightLbs { get; init; }
    public double CruiseSpeedKts { get; init; }
    public string? SimulatorVersion { get; init; }
    public required string Status { get; init; }
    public string? ReviewNotes { get; init; }
    public string? RequestedByUserName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
}
