using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AircraftController : ControllerBase
{
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<AircraftController> _logger;

    public AircraftController(
        PilotLifeDbContext context,
        ILogger<AircraftController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AircraftResponse>>> GetAllAircraft(
        [FromQuery] bool? approvedOnly = true)
    {
        var query = _context.Aircraft.AsQueryable();

        if (approvedOnly == true)
        {
            query = query.Where(a => a.IsApproved);
        }

        var aircraft = await query
            .OrderBy(a => a.Title)
            .ToListAsync();

        return Ok(aircraft.Select(ToResponse));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AircraftResponse>> GetAircraft(Guid id)
    {
        var aircraft = await _context.Aircraft
            .FirstOrDefaultAsync(a => a.Id == id);

        if (aircraft == null)
        {
            return NotFound(new { message = "Aircraft not found" });
        }

        return Ok(ToResponse(aircraft));
    }

    [HttpPost]
    public async Task<ActionResult<AircraftResponse>> CreateAircraft(
        [FromBody] CreateAircraftRequest request)
    {
        // Check if aircraft already exists
        var existing = await _context.Aircraft
            .FirstOrDefaultAsync(a => a.Title == request.Title);

        if (existing != null)
        {
            return BadRequest(new { message = "An aircraft with this title already exists" });
        }

        var aircraft = new Aircraft
        {
            Title = request.Title,
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
            IsApproved = request.IsApproved
        };

        _context.Aircraft.Add(aircraft);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Aircraft created: {Title}", aircraft.Title);

        return CreatedAtAction(nameof(GetAircraft), new { id = aircraft.Id }, ToResponse(aircraft));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AircraftResponse>> UpdateAircraft(
        Guid id,
        [FromBody] UpdateAircraftRequest request)
    {
        var aircraft = await _context.Aircraft
            .FirstOrDefaultAsync(a => a.Id == id);

        if (aircraft == null)
        {
            return NotFound(new { message = "Aircraft not found" });
        }

        // Check if new title conflicts with existing aircraft
        if (request.Title != aircraft.Title)
        {
            var existing = await _context.Aircraft
                .FirstOrDefaultAsync(a => a.Title == request.Title && a.Id != id);

            if (existing != null)
            {
                return BadRequest(new { message = "An aircraft with this title already exists" });
            }
        }

        aircraft.Title = request.Title;
        aircraft.AtcType = request.AtcType;
        aircraft.AtcModel = request.AtcModel;
        aircraft.Category = request.Category;
        aircraft.EngineType = request.EngineType;
        aircraft.EngineTypeStr = request.EngineTypeStr;
        aircraft.NumberOfEngines = request.NumberOfEngines;
        aircraft.MaxGrossWeightLbs = request.MaxGrossWeightLbs;
        aircraft.EmptyWeightLbs = request.EmptyWeightLbs;
        aircraft.CruiseSpeedKts = request.CruiseSpeedKts;
        aircraft.SimulatorVersion = request.SimulatorVersion;
        aircraft.IsApproved = request.IsApproved;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Aircraft updated: {Title}", aircraft.Title);

        return Ok(ToResponse(aircraft));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAircraft(Guid id)
    {
        var aircraft = await _context.Aircraft
            .FirstOrDefaultAsync(a => a.Id == id);

        if (aircraft == null)
        {
            return NotFound(new { message = "Aircraft not found" });
        }

        _context.Aircraft.Remove(aircraft);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Aircraft deleted: {Title}", aircraft.Title);

        return NoContent();
    }

    private static AircraftResponse ToResponse(Aircraft aircraft)
    {
        return new AircraftResponse
        {
            Id = aircraft.Id,
            Title = aircraft.Title,
            AtcType = aircraft.AtcType,
            AtcModel = aircraft.AtcModel,
            Category = aircraft.Category,
            EngineType = aircraft.EngineType,
            EngineTypeStr = aircraft.EngineTypeStr,
            NumberOfEngines = aircraft.NumberOfEngines,
            MaxGrossWeightLbs = aircraft.MaxGrossWeightLbs,
            EmptyWeightLbs = aircraft.EmptyWeightLbs,
            CruiseSpeedKts = aircraft.CruiseSpeedKts,
            SimulatorVersion = aircraft.SimulatorVersion,
            IsApproved = aircraft.IsApproved,
            CreatedAt = aircraft.CreatedAt,
            ModifiedAt = aircraft.ModifiedAt
        };
    }
}

public record CreateAircraftRequest
{
    public required string Title { get; init; }
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
    public bool IsApproved { get; init; }
}

public record UpdateAircraftRequest
{
    public required string Title { get; init; }
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
    public bool IsApproved { get; init; }
}

public record AircraftResponse
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
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
    public bool IsApproved { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ModifiedAt { get; init; }
}
