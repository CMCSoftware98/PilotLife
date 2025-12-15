using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;

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

    /// <summary>
    /// Gets airports within map bounds with zoom-based filtering.
    /// At low zoom levels, only large airports are shown. At higher zoom levels, more airports are included.
    /// </summary>
    [HttpGet("in-bounds")]
    public async Task<ActionResult<List<AirportDto>>> GetAirportsInBounds(
        [FromQuery] double north,
        [FromQuery] double south,
        [FromQuery] double east,
        [FromQuery] double west,
        [FromQuery] int zoomLevel = 5,
        [FromQuery] int limit = 500)
    {
        var query = _context.Airports
            .Where(a =>
                a.Latitude >= south &&
                a.Latitude <= north &&
                a.Longitude >= west &&
                a.Longitude <= east);

        // Filter by airport type based on zoom level
        // Zoom 0-4: Only large airports
        // Zoom 5-7: Large and medium airports
        // Zoom 8+: All airports (large, medium, small)
        if (zoomLevel <= 4)
        {
            query = query.Where(a => a.Type == "large_airport");
        }
        else if (zoomLevel <= 7)
        {
            query = query.Where(a => a.Type == "large_airport" || a.Type == "medium_airport");
        }
        else
        {
            query = query.Where(a => a.Type == "large_airport" || a.Type == "medium_airport" || a.Type == "small_airport");
        }

        var airports = await query
            .OrderByDescending(a => a.Type == "large_airport")
            .ThenByDescending(a => a.Type == "medium_airport")
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

    /// <summary>
    /// Gets airports within a radius of a center point.
    /// </summary>
    [HttpGet("nearby")]
    public async Task<ActionResult<List<AirportDto>>> GetNearbyAirports(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusNm = 100,
        [FromQuery] string? types = null,
        [FromQuery] int limit = 100)
    {
        // Use bounding box for initial filter (1 degree lat ≈ 60nm)
        var latDegreeRange = radiusNm / 60.0;
        var lonDegreeRange = radiusNm / (60.0 * Math.Cos(latitude * Math.PI / 180));

        var query = _context.Airports
            .Where(a =>
                a.Latitude >= latitude - latDegreeRange &&
                a.Latitude <= latitude + latDegreeRange &&
                a.Longitude >= longitude - lonDegreeRange &&
                a.Longitude <= longitude + lonDegreeRange);

        // Filter by types if specified (comma-separated)
        if (!string.IsNullOrWhiteSpace(types))
        {
            var typeList = types.Split(',').Select(t => t.Trim()).ToList();
            query = query.Where(a => typeList.Contains(a.Type));
        }
        else
        {
            // Default to major airport types
            query = query.Where(a => a.Type == "large_airport" || a.Type == "medium_airport" || a.Type == "small_airport");
        }

        var airports = await query
            .OrderByDescending(a => a.Type == "large_airport")
            .ThenByDescending(a => a.Type == "medium_airport")
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
