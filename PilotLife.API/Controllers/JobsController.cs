using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<JobsController> _logger;

    public JobsController(PilotLifeDbContext context, ILogger<JobsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<JobDto>>> GetAvailableJobs(
        [FromQuery] int airportId,
        [FromQuery] string? cargoType = null,
        [FromQuery] string? aircraftType = null,
        [FromQuery] int? maxDistance = null)
    {
        var query = _context.Jobs
            .Include(j => j.DepartureAirport)
            .Include(j => j.ArrivalAirport)
            .Where(j => j.DepartureAirportId == airportId
                        && !j.IsCompleted
                        && j.AssignedToUserId == null
                        && j.ExpiresAt > DateTimeOffset.UtcNow);

        if (!string.IsNullOrWhiteSpace(cargoType))
        {
            query = query.Where(j => j.CargoType == cargoType);
        }

        if (!string.IsNullOrWhiteSpace(aircraftType))
        {
            query = query.Where(j => j.RequiredAircraftType == aircraftType);
        }

        if (maxDistance.HasValue)
        {
            query = query.Where(j => j.DistanceNm <= maxDistance.Value);
        }

        var jobs = await query
            .OrderByDescending(j => j.Payout)
            .Take(50)
            .Select(j => new JobDto
            {
                Id = j.Id,
                DepartureAirport = new JobAirportDto
                {
                    Id = j.DepartureAirport.Id,
                    Ident = j.DepartureAirport.Ident,
                    Name = j.DepartureAirport.Name,
                    IataCode = j.DepartureAirport.IataCode,
                    Latitude = j.DepartureAirport.Latitude,
                    Longitude = j.DepartureAirport.Longitude
                },
                ArrivalAirport = new JobAirportDto
                {
                    Id = j.ArrivalAirport.Id,
                    Ident = j.ArrivalAirport.Ident,
                    Name = j.ArrivalAirport.Name,
                    IataCode = j.ArrivalAirport.IataCode,
                    Latitude = j.ArrivalAirport.Latitude,
                    Longitude = j.ArrivalAirport.Longitude
                },
                CargoType = j.CargoType,
                Weight = j.Weight,
                Payout = j.Payout,
                DistanceNm = j.DistanceNm,
                EstimatedFlightTimeMinutes = j.EstimatedFlightTimeMinutes,
                RequiredAircraftType = j.RequiredAircraftType,
                ExpiresAt = j.ExpiresAt
            })
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobDto>> GetJob(Guid id)
    {
        var job = await _context.Jobs
            .Include(j => j.DepartureAirport)
            .Include(j => j.ArrivalAirport)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null)
        {
            return NotFound(new { message = "Job not found" });
        }

        return Ok(new JobDto
        {
            Id = job.Id,
            DepartureAirport = new JobAirportDto
            {
                Id = job.DepartureAirport.Id,
                Ident = job.DepartureAirport.Ident,
                Name = job.DepartureAirport.Name,
                IataCode = job.DepartureAirport.IataCode,
                Latitude = job.DepartureAirport.Latitude,
                Longitude = job.DepartureAirport.Longitude
            },
            ArrivalAirport = new JobAirportDto
            {
                Id = job.ArrivalAirport.Id,
                Ident = job.ArrivalAirport.Ident,
                Name = job.ArrivalAirport.Name,
                IataCode = job.ArrivalAirport.IataCode,
                Latitude = job.ArrivalAirport.Latitude,
                Longitude = job.ArrivalAirport.Longitude
            },
            CargoType = job.CargoType,
            Weight = job.Weight,
            Payout = job.Payout,
            DistanceNm = job.DistanceNm,
            EstimatedFlightTimeMinutes = job.EstimatedFlightTimeMinutes,
            RequiredAircraftType = job.RequiredAircraftType,
            ExpiresAt = job.ExpiresAt
        });
    }

    [HttpPost("{id}/accept")]
    public async Task<ActionResult<JobDto>> AcceptJob(Guid id, [FromBody] AcceptJobRequest request)
    {
        var job = await _context.Jobs
            .Include(j => j.DepartureAirport)
            .Include(j => j.ArrivalAirport)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null)
        {
            return NotFound(new { message = "Job not found" });
        }

        if (job.AssignedToUserId != null)
        {
            return BadRequest(new { message = "Job is already assigned" });
        }

        if (job.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return BadRequest(new { message = "Job has expired" });
        }

        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
        {
            return BadRequest(new { message = "User not found" });
        }

        if (user.CurrentAirportId != job.DepartureAirportId)
        {
            return BadRequest(new { message = "You must be at the departure airport to accept this job" });
        }

        job.AssignedToUserId = request.UserId;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} accepted job {JobId}", request.UserId, id);

        return Ok(new JobDto
        {
            Id = job.Id,
            DepartureAirport = new JobAirportDto
            {
                Id = job.DepartureAirport.Id,
                Ident = job.DepartureAirport.Ident,
                Name = job.DepartureAirport.Name,
                IataCode = job.DepartureAirport.IataCode,
                Latitude = job.DepartureAirport.Latitude,
                Longitude = job.DepartureAirport.Longitude
            },
            ArrivalAirport = new JobAirportDto
            {
                Id = job.ArrivalAirport.Id,
                Ident = job.ArrivalAirport.Ident,
                Name = job.ArrivalAirport.Name,
                IataCode = job.ArrivalAirport.IataCode,
                Latitude = job.ArrivalAirport.Latitude,
                Longitude = job.ArrivalAirport.Longitude
            },
            CargoType = job.CargoType,
            Weight = job.Weight,
            Payout = job.Payout,
            DistanceNm = job.DistanceNm,
            EstimatedFlightTimeMinutes = job.EstimatedFlightTimeMinutes,
            RequiredAircraftType = job.RequiredAircraftType,
            ExpiresAt = job.ExpiresAt
        });
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult> CompleteJob(Guid id, [FromBody] CompleteJobRequest request)
    {
        var job = await _context.Jobs
            .Include(j => j.ArrivalAirport)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null)
        {
            return NotFound(new { message = "Job not found" });
        }

        if (job.AssignedToUserId != request.UserId)
        {
            return BadRequest(new { message = "This job is not assigned to you" });
        }

        if (job.IsCompleted)
        {
            return BadRequest(new { message = "Job is already completed" });
        }

        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
        {
            return BadRequest(new { message = "User not found" });
        }

        // Complete the job
        job.IsCompleted = true;

        // Update user
        user.CurrentAirportId = job.ArrivalAirportId;
        user.Balance += job.Payout;
        user.TotalFlightMinutes += job.EstimatedFlightTimeMinutes;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} completed job {JobId}, earned {Payout}",
            request.UserId, id, job.Payout);

        return Ok(new
        {
            message = "Job completed successfully",
            payout = job.Payout,
            newBalance = user.Balance,
            newLocation = job.ArrivalAirport.Name
        });
    }
}

public record JobDto
{
    public Guid Id { get; init; }
    public required JobAirportDto DepartureAirport { get; init; }
    public required JobAirportDto ArrivalAirport { get; init; }
    public required string CargoType { get; init; }
    public int Weight { get; init; }
    public decimal Payout { get; init; }
    public double DistanceNm { get; init; }
    public int EstimatedFlightTimeMinutes { get; init; }
    public required string RequiredAircraftType { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}

public record JobAirportDto
{
    public int Id { get; init; }
    public required string Ident { get; init; }
    public required string Name { get; init; }
    public string? IataCode { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}

public record AcceptJobRequest
{
    public Guid UserId { get; init; }
}

public record CompleteJobRequest
{
    public Guid UserId { get; init; }
}
