using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Database.Entities;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AirportsController : ControllerBase
{
    private readonly PilotLifeDbContext _context;

    public AirportsController(PilotLifeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<AirportListResponse>> GetAirports(
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.Airports.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(searchLower) ||
                a.Ident.ToLower().Contains(searchLower) ||
                (a.IataCode != null && a.IataCode.ToLower().Contains(searchLower)) ||
                (a.Municipality != null && a.Municipality.ToLower().Contains(searchLower)));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(a => a.Type == type);
        }

        var totalCount = await query.CountAsync();

        var airports = await query
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AirportDto
            {
                Id = a.Id,
                Ident = a.Ident,
                Name = a.Name,
                IataCode = a.IataCode,
                Type = a.Type,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                ElevationFt = a.ElevationFt,
                Country = a.Country,
                Municipality = a.Municipality
            })
            .ToListAsync();

        return Ok(new AirportListResponse
        {
            Airports = airports,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AirportDto>> GetAirport(int id)
    {
        var airport = await _context.Airports.FindAsync(id);

        if (airport == null)
        {
            return NotFound(new { message = "Airport not found" });
        }

        return Ok(new AirportDto
        {
            Id = airport.Id,
            Ident = airport.Ident,
            Name = airport.Name,
            IataCode = airport.IataCode,
            Type = airport.Type,
            Latitude = airport.Latitude,
            Longitude = airport.Longitude,
            ElevationFt = airport.ElevationFt,
            Country = airport.Country,
            Municipality = airport.Municipality
        });
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<AirportDto>>> SearchAirports([FromQuery] string q, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return Ok(new List<AirportDto>());
        }

        var searchLower = q.ToLower();

        var airports = await _context.Airports
            .Where(a =>
                a.Ident.ToLower().StartsWith(searchLower) ||
                (a.IataCode != null && a.IataCode.ToLower().StartsWith(searchLower)) ||
                a.Name.ToLower().Contains(searchLower))
            .OrderByDescending(a => a.Type == "large_airport")
            .ThenByDescending(a => a.Type == "medium_airport")
            .ThenBy(a => a.Name)
            .Take(limit)
            .Select(a => new AirportDto
            {
                Id = a.Id,
                Ident = a.Ident,
                Name = a.Name,
                IataCode = a.IataCode,
                Type = a.Type,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                ElevationFt = a.ElevationFt,
                Country = a.Country,
                Municipality = a.Municipality
            })
            .ToListAsync();

        return Ok(airports);
    }

    [HttpGet("by-ident/{ident}")]
    public async Task<ActionResult<AirportDto>> GetAirportByIdent(string ident)
    {
        var airport = await _context.Airports
            .FirstOrDefaultAsync(a => a.Ident.ToLower() == ident.ToLower());

        if (airport == null)
        {
            return NotFound(new { message = "Airport not found" });
        }

        return Ok(new AirportDto
        {
            Id = airport.Id,
            Ident = airport.Ident,
            Name = airport.Name,
            IataCode = airport.IataCode,
            Type = airport.Type,
            Latitude = airport.Latitude,
            Longitude = airport.Longitude,
            ElevationFt = airport.ElevationFt,
            Country = airport.Country,
            Municipality = airport.Municipality
        });
    }
}

public record AirportDto
{
    public int Id { get; init; }
    public required string Ident { get; init; }
    public required string Name { get; init; }
    public string? IataCode { get; init; }
    public required string Type { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public int? ElevationFt { get; init; }
    public string? Country { get; init; }
    public string? Municipality { get; init; }
}

public record AirportListResponse
{
    public required List<AirportDto> Airports { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
