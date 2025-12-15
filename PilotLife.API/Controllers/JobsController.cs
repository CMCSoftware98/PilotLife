using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.Application.Jobs;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly PilotLifeDbContext _context;
    private readonly IJobGenerator _jobGenerator;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        PilotLifeDbContext context,
        IJobGenerator jobGenerator,
        ILogger<JobsController> logger)
    {
        _context = context;
        _jobGenerator = jobGenerator;
        _logger = logger;
    }

    /// <summary>
    /// Search for available jobs across all airports with advanced filtering.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<JobSearchResult>> SearchJobs([FromQuery] JobSearchRequest request)
    {
        var query = _context.Jobs
            .Include(j => j.DepartureAirport)
            .Include(j => j.ArrivalAirport)
            .Where(j =>
                j.WorldId == request.WorldId &&
                j.Status == JobStatus.Available &&
                j.AssignedToUserId == null &&
                j.ExpiresAt > DateTimeOffset.UtcNow);

        // Vicinity-based search - find jobs departing from airports within range of center point
        if (request.CenterLatitude.HasValue && request.CenterLongitude.HasValue && request.VicinityRadiusNm.HasValue)
        {
            var centerLat = request.CenterLatitude.Value;
            var centerLon = request.CenterLongitude.Value;
            var radiusNm = request.VicinityRadiusNm.Value;

            // Get airports within range first (using bounding box for efficiency, then filter by actual distance)
            var latDegreeRange = radiusNm / 60.0; // 1 degree lat ≈ 60nm
            var lonDegreeRange = radiusNm / (60.0 * Math.Cos(centerLat * Math.PI / 180)); // Adjust for latitude

            var nearbyAirportIds = await _context.Airports
                .Where(a =>
                    a.Latitude >= centerLat - latDegreeRange &&
                    a.Latitude <= centerLat + latDegreeRange &&
                    a.Longitude >= centerLon - lonDegreeRange &&
                    a.Longitude <= centerLon + lonDegreeRange)
                .Select(a => a.Id)
                .ToListAsync();

            query = query.Where(j => nearbyAirportIds.Contains(j.DepartureAirportId));
        }

        // Filter by departure airport
        if (request.DepartureAirportId.HasValue)
        {
            query = query.Where(j => j.DepartureAirportId == request.DepartureAirportId.Value);
        }

        // Filter by departure ICAO
        if (!string.IsNullOrWhiteSpace(request.DepartureIcao))
        {
            query = query.Where(j => j.DepartureIcao == request.DepartureIcao.ToUpper());
        }

        // Filter by arrival airport
        if (request.ArrivalAirportId.HasValue)
        {
            query = query.Where(j => j.ArrivalAirportId == request.ArrivalAirportId.Value);
        }

        // Filter by arrival ICAO
        if (!string.IsNullOrWhiteSpace(request.ArrivalIcao))
        {
            query = query.Where(j => j.ArrivalIcao == request.ArrivalIcao.ToUpper());
        }

        // Filter by job type
        if (request.JobType.HasValue)
        {
            query = query.Where(j => j.Type == request.JobType.Value);
        }

        // Filter by urgency
        if (request.Urgency.HasValue)
        {
            query = query.Where(j => j.Urgency == request.Urgency.Value);
        }

        // Filter by cargo type
        if (!string.IsNullOrWhiteSpace(request.CargoType))
        {
            query = query.Where(j => j.CargoType == request.CargoType);
        }

        // Filter by passenger class
        if (request.PassengerClass.HasValue)
        {
            query = query.Where(j => j.PassengerClass == request.PassengerClass.Value);
        }

        // Filter by distance
        if (request.MinDistanceNm.HasValue)
        {
            query = query.Where(j => j.DistanceNm >= request.MinDistanceNm.Value);
        }
        if (request.MaxDistanceNm.HasValue)
        {
            query = query.Where(j => j.DistanceNm <= request.MaxDistanceNm.Value);
        }

        // Filter by distance category
        if (request.DistanceCategory.HasValue)
        {
            query = query.Where(j => j.DistanceCategory == request.DistanceCategory.Value);
        }

        // Filter by payout
        if (request.MinPayout.HasValue)
        {
            query = query.Where(j => j.Payout >= request.MinPayout.Value);
        }
        if (request.MaxPayout.HasValue)
        {
            query = query.Where(j => j.Payout <= request.MaxPayout.Value);
        }

        // Filter by weight/size
        if (request.MinWeightLbs.HasValue)
        {
            query = query.Where(j => j.WeightLbs >= request.MinWeightLbs.Value);
        }
        if (request.MaxWeightLbs.HasValue)
        {
            query = query.Where(j => j.WeightLbs <= request.MaxWeightLbs.Value);
        }

        // Filter by passenger count
        if (request.MinPassengers.HasValue)
        {
            query = query.Where(j => j.PassengerCount >= request.MinPassengers.Value);
        }
        if (request.MaxPassengers.HasValue)
        {
            query = query.Where(j => j.PassengerCount <= request.MaxPassengers.Value);
        }

        // Filter by special requirements
        if (request.RequiresSpecialCertification.HasValue)
        {
            query = query.Where(j => j.RequiresSpecialCertification == request.RequiresSpecialCertification.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "payout" => request.SortDescending ? query.OrderByDescending(j => j.Payout) : query.OrderBy(j => j.Payout),
            "distance" => request.SortDescending ? query.OrderByDescending(j => j.DistanceNm) : query.OrderBy(j => j.DistanceNm),
            "weight" => request.SortDescending ? query.OrderByDescending(j => j.WeightLbs) : query.OrderBy(j => j.WeightLbs),
            "expiry" => request.SortDescending ? query.OrderByDescending(j => j.ExpiresAt) : query.OrderBy(j => j.ExpiresAt),
            "urgency" => request.SortDescending ? query.OrderByDescending(j => j.Urgency) : query.OrderBy(j => j.Urgency),
            _ => query.OrderByDescending(j => j.Payout)
        };

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var jobs = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(j => MapToDto(j))
            .ToListAsync();

        return Ok(new JobSearchResult
        {
            Jobs = jobs,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    /// <summary>
    /// Gets available jobs at a specific airport.
    /// </summary>
    [HttpGet("available")]
    public async Task<ActionResult<List<JobDto>>> GetAvailableJobs(
        [FromQuery] Guid worldId,
        [FromQuery] int airportId,
        [FromQuery] int limit = 50)
    {
        var jobs = await _context.Jobs
            .Include(j => j.DepartureAirport)
            .Include(j => j.ArrivalAirport)
            .Where(j =>
                j.WorldId == worldId &&
                j.DepartureAirportId == airportId &&
                j.Status == JobStatus.Available &&
                j.AssignedToUserId == null &&
                j.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(j => j.Payout)
            .Take(limit)
            .Select(j => MapToDto(j))
            .ToListAsync();

        return Ok(jobs);
    }

    /// <summary>
    /// Gets a specific job by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
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

        return Ok(MapToDto(job));
    }

    /// <summary>
    /// Accepts a job for a player.
    /// </summary>
    [HttpPost("{id:guid}/accept")]
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

        if (job.Status != JobStatus.Available)
        {
            return BadRequest(new { message = "Job is no longer available" });
        }

        if (job.AssignedToUserId != null)
        {
            return BadRequest(new { message = "Job is already assigned" });
        }

        if (job.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return BadRequest(new { message = "Job has expired" });
        }

        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.UserId == request.UserId && pw.WorldId == job.WorldId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "You must join this world first" });
        }

        if (playerWorld.CurrentAirportId != job.DepartureAirportId)
        {
            return BadRequest(new { message = "You must be at the departure airport to accept this job" });
        }

        var worldSettings = await _context.WorldSettings
            .FirstOrDefaultAsync(ws => ws.WorldId == job.WorldId);

        var maxJobs = worldSettings?.MaxActiveJobsPerPlayer ?? 5;
        var activeJobCount = await _context.Jobs
            .CountAsync(j =>
                j.WorldId == job.WorldId &&
                j.AssignedToUserId == request.UserId &&
                j.Status == JobStatus.Accepted);

        if (activeJobCount >= maxJobs)
        {
            return BadRequest(new { message = $"You can only have {maxJobs} active jobs at a time" });
        }

        job.Accept(request.UserId, playerWorld.Id);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} accepted job {JobId}", request.UserId, id);

        return Ok(MapToDto(job));
    }

    /// <summary>
    /// Completes a job.
    /// </summary>
    [HttpPost("{id:guid}/complete")]
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

        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.UserId == request.UserId && pw.WorldId == job.WorldId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "Player world not found" });
        }

        job.Complete(job.Payout);

        playerWorld.CurrentAirportId = job.ArrivalAirportId;
        playerWorld.Balance += job.Payout;
        playerWorld.TotalFlightMinutes += job.EstimatedFlightTimeMinutes;
        playerWorld.TotalJobsCompleted++;
        playerWorld.TotalEarnings += job.Payout;
        playerWorld.OnTimeDeliveries++;
        playerWorld.LastActiveAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} completed job {JobId}, earned {Payout}",
            request.UserId, id, job.Payout);

        return Ok(new
        {
            message = "Job completed successfully",
            payout = job.Payout,
            newBalance = playerWorld.Balance,
            newLocation = job.ArrivalAirport?.Name
        });
    }

    /// <summary>
    /// Cancels an accepted job.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult> CancelJob(Guid id, [FromBody] CancelJobRequest request)
    {
        var job = await _context.Jobs.FindAsync(id);

        if (job == null)
        {
            return NotFound(new { message = "Job not found" });
        }

        if (job.AssignedToUserId != request.UserId)
        {
            return BadRequest(new { message = "This job is not assigned to you" });
        }

        if (job.Status is JobStatus.Completed or JobStatus.Failed)
        {
            return BadRequest(new { message = "Cannot cancel a completed or failed job" });
        }

        job.Status = JobStatus.Available;
        job.AssignedToUserId = null;
        job.AssignedToPlayerWorldId = null;
        job.AcceptedAt = null;
        job.StartedAt = null;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} cancelled job {JobId}", request.UserId, id);

        return Ok(new { message = "Job cancelled" });
    }

    /// <summary>
    /// Gets jobs assigned to a player in a world.
    /// </summary>
    [HttpGet("my-jobs")]
    public async Task<ActionResult<List<JobDto>>> GetMyJobs(
        [FromQuery] Guid worldId,
        [FromQuery] Guid userId,
        [FromQuery] bool includeCompleted = false)
    {
        var query = _context.Jobs
            .Include(j => j.DepartureAirport)
            .Include(j => j.ArrivalAirport)
            .Where(j => j.WorldId == worldId && j.AssignedToUserId == userId);

        if (!includeCompleted)
        {
            query = query.Where(j => j.Status == JobStatus.Accepted || j.Status == JobStatus.InProgress);
        }

        var jobs = await query
            .OrderByDescending(j => j.AcceptedAt)
            .Select(j => MapToDto(j))
            .ToListAsync();

        return Ok(jobs);
    }

    /// <summary>
    /// Gets available cargo types for filtering.
    /// </summary>
    [HttpGet("cargo-types")]
    public async Task<ActionResult<List<CargoTypeDto>>> GetCargoTypes()
    {
        var cargoTypes = await _context.CargoTypes
            .Where(c => c.IsActive)
            .OrderBy(c => c.Category)
            .ThenBy(c => c.Name)
            .Select(c => new CargoTypeDto
            {
                Id = c.Id,
                Name = c.Name,
                Category = c.Category.ToString(),
                Subcategory = c.Subcategory,
                Description = c.Description,
                RequiresSpecialHandling = c.RequiresSpecialHandling,
                SpecialHandlingType = c.SpecialHandlingType,
                IsTimeCritical = c.IsTimeCritical
            })
            .ToListAsync();

        return Ok(cargoTypes);
    }

    /// <summary>
    /// Gets job statistics for a world (optionally filtered by airport).
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<JobStatsResult>> GetJobStats([FromQuery] Guid worldId, [FromQuery] int? airportId = null)
    {
        var query = _context.Jobs.Where(j => j.WorldId == worldId);

        if (airportId.HasValue)
        {
            query = query.Where(j => j.DepartureAirportId == airportId.Value);
        }

        var availableJobs = await query
            .Where(j => j.Status == JobStatus.Available && j.ExpiresAt > DateTimeOffset.UtcNow)
            .CountAsync();

        var avgPayout = await query
            .Where(j => j.Status == JobStatus.Available && j.ExpiresAt > DateTimeOffset.UtcNow)
            .AverageAsync(j => (decimal?)j.Payout) ?? 0;

        var byType = await query
            .Where(j => j.Status == JobStatus.Available && j.ExpiresAt > DateTimeOffset.UtcNow)
            .GroupBy(j => j.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var byUrgency = await query
            .Where(j => j.Status == JobStatus.Available && j.ExpiresAt > DateTimeOffset.UtcNow)
            .GroupBy(j => j.Urgency)
            .Select(g => new { Urgency = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var byDistance = await query
            .Where(j => j.Status == JobStatus.Available && j.ExpiresAt > DateTimeOffset.UtcNow)
            .GroupBy(j => j.DistanceCategory)
            .Select(g => new { Category = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        return Ok(new JobStatsResult
        {
            AvailableJobs = availableJobs,
            AveragePayout = avgPayout,
            ByType = byType.ToDictionary(x => x.Type, x => x.Count),
            ByUrgency = byUrgency.ToDictionary(x => x.Urgency, x => x.Count),
            ByDistanceCategory = byDistance.ToDictionary(x => x.Category, x => x.Count)
        });
    }

    /// <summary>
    /// Triggers full job population for a world (admin endpoint).
    /// </summary>
    [HttpPost("populate")]
    public async Task<ActionResult> PopulateJobs([FromBody] PopulateJobsRequest request)
    {
        await _jobGenerator.PopulateWorldJobsAsync(request.WorldId);
        return Ok(new { message = "Job population completed" });
    }

    /// <summary>
    /// Triggers job refresh for stale airports (admin endpoint).
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshJobs([FromBody] PopulateJobsRequest request)
    {
        await _jobGenerator.RefreshStaleJobsAsync(request.WorldId);
        return Ok(new { message = "Job refresh completed" });
    }

    /// <summary>
    /// Cleans up expired jobs (admin endpoint).
    /// </summary>
    [HttpPost("cleanup")]
    public async Task<ActionResult> CleanupJobs([FromQuery] Guid? worldId = null)
    {
        var cleaned = await _jobGenerator.CleanupExpiredJobsAsync(worldId);
        return Ok(new { message = $"Cleaned up {cleaned} expired jobs", count = cleaned });
    }

    private static JobDto MapToDto(Job j) => new()
    {
        Id = j.Id,
        WorldId = j.WorldId,
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
        Type = j.Type,
        Status = j.Status,
        Urgency = j.Urgency,
        CargoType = j.CargoType,
        WeightLbs = j.WeightLbs ?? 0,
        VolumeCuFt = j.VolumeCuFt,
        PassengerCount = j.PassengerCount,
        PassengerClass = j.PassengerClass,
        BasePayout = j.BasePayout,
        Payout = j.Payout,
        DistanceNm = j.DistanceNm,
        DistanceCategory = j.DistanceCategory,
        EstimatedFlightTimeMinutes = j.EstimatedFlightTimeMinutes,
        RequiredAircraftType = j.RequiredAircraftType,
        RequiresSpecialCertification = j.RequiresSpecialCertification,
        RequiredCertification = j.RequiredCertification,
        RiskLevel = j.RiskLevel,
        ExpiresAt = j.ExpiresAt,
        AcceptedAt = j.AcceptedAt,
        Title = j.Title,
        Description = j.Description
    };
}

// Search Request/Response DTOs
public record JobSearchRequest
{
    public Guid WorldId { get; init; }
    public int? DepartureAirportId { get; init; }
    public string? DepartureIcao { get; init; }
    public int? ArrivalAirportId { get; init; }
    public string? ArrivalIcao { get; init; }
    public JobType? JobType { get; init; }
    public JobUrgency? Urgency { get; init; }
    public string? CargoType { get; init; }
    public PassengerClass? PassengerClass { get; init; }
    public double? MinDistanceNm { get; init; }
    public double? MaxDistanceNm { get; init; }
    public DistanceCategory? DistanceCategory { get; init; }
    public decimal? MinPayout { get; init; }
    public decimal? MaxPayout { get; init; }
    public int? MinWeightLbs { get; init; }
    public int? MaxWeightLbs { get; init; }
    public int? MinPassengers { get; init; }
    public int? MaxPassengers { get; init; }
    public bool? RequiresSpecialCertification { get; init; }
    public string? SortBy { get; init; } = "payout";
    public bool SortDescending { get; init; } = true;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    // Vicinity search - search for jobs departing from airports within range of a point
    public double? CenterLatitude { get; init; }
    public double? CenterLongitude { get; init; }
    public double? VicinityRadiusNm { get; init; }
}

public record JobSearchResult
{
    public List<JobDto> Jobs { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

public record JobStatsResult
{
    public int AvailableJobs { get; init; }
    public decimal AveragePayout { get; init; }
    public Dictionary<string, int> ByType { get; init; } = new();
    public Dictionary<string, int> ByUrgency { get; init; } = new();
    public Dictionary<string, int> ByDistanceCategory { get; init; } = new();
}

public record CargoTypeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? Subcategory { get; init; }
    public string? Description { get; init; }
    public bool RequiresSpecialHandling { get; init; }
    public string? SpecialHandlingType { get; init; }
    public bool IsTimeCritical { get; init; }
}

public record PopulateJobsRequest
{
    public Guid WorldId { get; init; }
}

// Job DTOs
public record JobDto
{
    public Guid Id { get; init; }
    public Guid WorldId { get; init; }
    public required JobAirportDto DepartureAirport { get; init; }
    public required JobAirportDto ArrivalAirport { get; init; }
    public JobType Type { get; init; }
    public JobStatus Status { get; init; }
    public JobUrgency Urgency { get; init; }
    public string? CargoType { get; init; }
    public int WeightLbs { get; init; }
    public decimal? VolumeCuFt { get; init; }
    public int? PassengerCount { get; init; }
    public PassengerClass? PassengerClass { get; init; }
    public decimal BasePayout { get; init; }
    public decimal Payout { get; init; }
    public double DistanceNm { get; init; }
    public DistanceCategory DistanceCategory { get; init; }
    public int EstimatedFlightTimeMinutes { get; init; }
    public string? RequiredAircraftType { get; init; }
    public bool RequiresSpecialCertification { get; init; }
    public string? RequiredCertification { get; init; }
    public int RiskLevel { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? AcceptedAt { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
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

public record CancelJobRequest
{
    public Guid UserId { get; init; }
}
