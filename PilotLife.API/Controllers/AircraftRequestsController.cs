using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;
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

            // File data
            ManifestJsonRaw = request.ManifestJsonRaw,
            AircraftCfgRaw = request.AircraftCfgRaw,
            ManifestContentType = request.ManifestContentType,
            ManifestTitle = request.ManifestTitle,
            ManifestManufacturer = request.ManifestManufacturer,
            ManifestCreator = request.ManifestCreator,
            ManifestPackageVersion = request.ManifestPackageVersion,
            ManifestMinimumGameVersion = request.ManifestMinimumGameVersion,
            ManifestTotalPackageSize = request.ManifestTotalPackageSize,
            ManifestContentId = request.ManifestContentId,
            CfgTitle = request.CfgTitle,
            CfgModel = request.CfgModel,
            CfgPanel = request.CfgPanel,
            CfgSound = request.CfgSound,
            CfgTexture = request.CfgTexture,
            CfgAtcType = request.CfgAtcType,
            CfgAtcModel = request.CfgAtcModel,
            CfgAtcId = request.CfgAtcId,
            CfgAtcAirline = request.CfgAtcAirline,
            CfgUiManufacturer = request.CfgUiManufacturer,
            CfgUiType = request.CfgUiType,
            CfgUiVariation = request.CfgUiVariation,
            CfgIcaoAirline = request.CfgIcaoAirline,
            CfgGeneralAtcType = request.CfgGeneralAtcType,
            CfgGeneralAtcModel = request.CfgGeneralAtcModel,
            CfgEditable = request.CfgEditable,
            CfgPerformance = request.CfgPerformance,
            CfgCategory = request.CfgCategory
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
            IsApproved = true
        };

        _context.Aircraft.Add(aircraft);

        aircraftRequest.Status = AircraftRequestStatus.Approved;
        aircraftRequest.ReviewNotes = request.ReviewNotes;
        aircraftRequest.ReviewedByUserId = userId;
        aircraftRequest.ReviewedAt = DateTimeOffset.UtcNow;

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
        aircraftRequest.ReviewedAt = DateTimeOffset.UtcNow;

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

    [HttpGet("check/{aircraftTitle}")]
    public async Task<ActionResult<AircraftRequestCheckResponse>> CheckExistingRequest(string aircraftTitle)
    {
        // Check if aircraft exists in database
        var existingAircraft = await _context.Aircraft
            .FirstOrDefaultAsync(a => a.Title == aircraftTitle);

        if (existingAircraft != null)
        {
            return Ok(new AircraftRequestCheckResponse { Status = "exists" });
        }

        // Check for existing request
        var existingRequest = await _context.AircraftRequests
            .FirstOrDefaultAsync(r => r.AircraftTitle == aircraftTitle);

        if (existingRequest == null)
        {
            return Ok(new AircraftRequestCheckResponse { Status = "none" });
        }

        return Ok(new AircraftRequestCheckResponse
        {
            Status = existingRequest.Status.ToString().ToLower(),
            RequestId = existingRequest.Id.ToString()
        });
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
        var hasFileData = !string.IsNullOrEmpty(request.ManifestJsonRaw) ||
                          !string.IsNullOrEmpty(request.AircraftCfgRaw);

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
            ReviewedAt = request.ReviewedAt,
            HasFileData = hasFileData,
            ManifestData = hasFileData && !string.IsNullOrEmpty(request.ManifestJsonRaw)
                ? new ManifestDataResponse
                {
                    ContentType = request.ManifestContentType,
                    Title = request.ManifestTitle,
                    Manufacturer = request.ManifestManufacturer,
                    Creator = request.ManifestCreator,
                    PackageVersion = request.ManifestPackageVersion,
                    MinimumGameVersion = request.ManifestMinimumGameVersion,
                    TotalPackageSize = request.ManifestTotalPackageSize,
                    ContentId = request.ManifestContentId
                }
                : null,
            AircraftCfgData = hasFileData && !string.IsNullOrEmpty(request.AircraftCfgRaw)
                ? new AircraftCfgDataResponse
                {
                    Title = request.CfgTitle,
                    Model = request.CfgModel,
                    Panel = request.CfgPanel,
                    Sound = request.CfgSound,
                    Texture = request.CfgTexture,
                    AtcType = request.CfgAtcType,
                    AtcModel = request.CfgAtcModel,
                    AtcId = request.CfgAtcId,
                    AtcAirline = request.CfgAtcAirline,
                    UiManufacturer = request.CfgUiManufacturer,
                    UiType = request.CfgUiType,
                    UiVariation = request.CfgUiVariation,
                    IcaoAirline = request.CfgIcaoAirline,
                    GeneralAtcType = request.CfgGeneralAtcType,
                    GeneralAtcModel = request.CfgGeneralAtcModel,
                    Editable = request.CfgEditable,
                    Performance = request.CfgPerformance,
                    Category = request.CfgCategory
                }
                : null
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

    // Raw file contents
    public string? ManifestJsonRaw { get; init; }
    public string? AircraftCfgRaw { get; init; }

    // Manifest fields
    public string? ManifestContentType { get; init; }
    public string? ManifestTitle { get; init; }
    public string? ManifestManufacturer { get; init; }
    public string? ManifestCreator { get; init; }
    public string? ManifestPackageVersion { get; init; }
    public string? ManifestMinimumGameVersion { get; init; }
    public string? ManifestTotalPackageSize { get; init; }
    public string? ManifestContentId { get; init; }

    // Aircraft.cfg [FLTSIM.0] fields
    public string? CfgTitle { get; init; }
    public string? CfgModel { get; init; }
    public string? CfgPanel { get; init; }
    public string? CfgSound { get; init; }
    public string? CfgTexture { get; init; }
    public string? CfgAtcType { get; init; }
    public string? CfgAtcModel { get; init; }
    public string? CfgAtcId { get; init; }
    public string? CfgAtcAirline { get; init; }
    public string? CfgUiManufacturer { get; init; }
    public string? CfgUiType { get; init; }
    public string? CfgUiVariation { get; init; }
    public string? CfgIcaoAirline { get; init; }

    // Aircraft.cfg [GENERAL] fields
    public string? CfgGeneralAtcType { get; init; }
    public string? CfgGeneralAtcModel { get; init; }
    public string? CfgEditable { get; init; }
    public string? CfgPerformance { get; init; }
    public string? CfgCategory { get; init; }
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
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReviewedAt { get; init; }

    // File data
    public bool HasFileData { get; init; }
    public ManifestDataResponse? ManifestData { get; init; }
    public AircraftCfgDataResponse? AircraftCfgData { get; init; }
}

public record ManifestDataResponse
{
    public string? ContentType { get; init; }
    public string? Title { get; init; }
    public string? Manufacturer { get; init; }
    public string? Creator { get; init; }
    public string? PackageVersion { get; init; }
    public string? MinimumGameVersion { get; init; }
    public string? TotalPackageSize { get; init; }
    public string? ContentId { get; init; }
}

public record AircraftCfgDataResponse
{
    // [FLTSIM.0] section
    public string? Title { get; init; }
    public string? Model { get; init; }
    public string? Panel { get; init; }
    public string? Sound { get; init; }
    public string? Texture { get; init; }
    public string? AtcType { get; init; }
    public string? AtcModel { get; init; }
    public string? AtcId { get; init; }
    public string? AtcAirline { get; init; }
    public string? UiManufacturer { get; init; }
    public string? UiType { get; init; }
    public string? UiVariation { get; init; }
    public string? IcaoAirline { get; init; }

    // [GENERAL] section
    public string? GeneralAtcType { get; init; }
    public string? GeneralAtcModel { get; init; }
    public string? Editable { get; init; }
    public string? Performance { get; init; }
    public string? Category { get; init; }
}

public record AircraftRequestCheckResponse
{
    public required string Status { get; init; }
    public string? RequestId { get; init; }
}
