using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PilotLife.API.Services.Licenses;
using PilotLife.Database.Data;
using PilotLife.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExamsController : ControllerBase
{
    private readonly ExamService _examService;
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<ExamsController> _logger;

    public ExamsController(
        ExamService examService,
        PilotLifeDbContext context,
        ILogger<ExamsController> logger)
    {
        _examService = examService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Schedules a new exam.
    /// </summary>
    [HttpPost("schedule")]
    public async Task<ActionResult<ExamScheduleResponse>> ScheduleExam([FromBody] ScheduleExamRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.UserId == userId && pw.WorldId == request.WorldId);

        if (playerWorld == null)
            return NotFound(new { message = "Player not found in this world" });

        var (success, message, exam) = await _examService.ScheduleExamAsync(
            playerWorld.Id,
            request.LicenseCode,
            request.DepartureIcao);

        if (!success)
            return BadRequest(new { message });

        return Ok(new ExamScheduleResponse
        {
            Success = true,
            Message = message,
            Exam = MapToDto(exam!)
        });
    }

    /// <summary>
    /// Starts a scheduled exam.
    /// </summary>
    [HttpPost("{examId}/start")]
    public async Task<ActionResult<ExamStartResponse>> StartExam(Guid examId, [FromBody] StartExamRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // Verify ownership
        var exam = await _context.LicenseExams
            .Include(le => le.PlayerWorld)
            .FirstOrDefaultAsync(le => le.Id == examId);

        if (exam == null)
            return NotFound(new { message = "Exam not found" });

        if (exam.PlayerWorld.UserId != userId)
            return Forbid();

        var (success, message, updatedExam) = await _examService.StartExamAsync(examId, request.AircraftUsed);

        if (!success)
            return BadRequest(new { message });

        return Ok(new ExamStartResponse
        {
            Success = true,
            Message = message,
            Exam = MapToDto(updatedExam!)
        });
    }

    /// <summary>
    /// Records a violation during an exam.
    /// </summary>
    [HttpPost("{examId}/violation")]
    public async Task<ActionResult> RecordViolation(Guid examId, [FromBody] RecordViolationRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // Verify ownership
        var exam = await _context.LicenseExams
            .Include(le => le.PlayerWorld)
            .FirstOrDefaultAsync(le => le.Id == examId);

        if (exam == null)
            return NotFound(new { message = "Exam not found" });

        if (exam.PlayerWorld.UserId != userId)
            return Forbid();

        var violation = await _examService.RecordViolationAsync(
            examId,
            request.Type,
            request.Value,
            request.Threshold,
            request.Latitude,
            request.Longitude,
            request.Altitude);

        if (violation == null)
            return BadRequest(new { message = "Failed to record violation" });

        return Ok(new
        {
            violationId = violation.Id,
            type = violation.Type.ToString(),
            pointsDeducted = violation.PointsDeducted,
            causedFailure = violation.CausedFailure
        });
    }

    /// <summary>
    /// Records a landing during an exam.
    /// </summary>
    [HttpPost("{examId}/landing")]
    public async Task<ActionResult> RecordLanding(Guid examId, [FromBody] RecordLandingRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // Verify ownership
        var exam = await _context.LicenseExams
            .Include(le => le.PlayerWorld)
            .FirstOrDefaultAsync(le => le.Id == examId);

        if (exam == null)
            return NotFound(new { message = "Exam not found" });

        if (exam.PlayerWorld.UserId != userId)
            return Forbid();

        var landing = await _examService.RecordLandingAsync(examId, new ExamLandingData
        {
            AirportIcao = request.AirportIcao,
            Type = request.Type,
            VerticalSpeedFpm = request.VerticalSpeedFpm,
            CenterlineDeviationFt = request.CenterlineDeviationFt,
            TouchdownZoneDistanceFt = request.TouchdownZoneDistanceFt,
            GroundSpeedKts = request.GroundSpeedKts,
            PitchDeg = request.PitchDeg,
            BankDeg = request.BankDeg,
            GearDown = request.GearDown,
            RunwayUsed = request.RunwayUsed,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AltitudeAtTouchdown = request.AltitudeAtTouchdown
        });

        if (landing == null)
            return BadRequest(new { message = "Failed to record landing" });

        return Ok(new
        {
            landingId = landing.Id,
            order = landing.Order,
            pointsAwarded = landing.PointsAwarded,
            maxPoints = landing.MaxPoints,
            notes = landing.Notes
        });
    }

    /// <summary>
    /// Marks a checkpoint as reached during an exam.
    /// </summary>
    [HttpPost("{examId}/checkpoint/{checkpointOrder}")]
    public async Task<ActionResult> ReachCheckpoint(Guid examId, int checkpointOrder, [FromBody] ReachCheckpointRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // Verify ownership
        var exam = await _context.LicenseExams
            .Include(le => le.PlayerWorld)
            .FirstOrDefaultAsync(le => le.Id == examId);

        if (exam == null)
            return NotFound(new { message = "Exam not found" });

        if (exam.PlayerWorld.UserId != userId)
            return Forbid();

        var success = await _examService.ReachCheckpointAsync(examId, checkpointOrder, request.Altitude, request.Speed);

        if (!success)
            return BadRequest(new { message = "Failed to mark checkpoint" });

        return Ok(new { success = true });
    }

    /// <summary>
    /// Completes an exam and calculates the final score.
    /// </summary>
    [HttpPost("{examId}/complete")]
    public async Task<ActionResult<ExamCompleteResponse>> CompleteExam(Guid examId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // Verify ownership
        var exam = await _context.LicenseExams
            .Include(le => le.PlayerWorld)
            .FirstOrDefaultAsync(le => le.Id == examId);

        if (exam == null)
            return NotFound(new { message = "Exam not found" });

        if (exam.PlayerWorld.UserId != userId)
            return Forbid();

        var (success, result) = await _examService.CompleteExamAsync(examId);

        if (!success || result == null)
            return BadRequest(new { message = "Failed to complete exam" });

        return Ok(new ExamCompleteResponse
        {
            Success = true,
            Score = result.Score,
            Passed = result.Passed,
            Exam = MapToDto(result.Exam),
            License = result.License != null ? new LicenseGrantedDto
            {
                Id = result.License.Id,
                LicenseCode = result.Exam.LicenseType.Code,
                LicenseName = result.Exam.LicenseType.Name,
                ExpiresAt = result.License.ExpiresAt
            } : null
        });
    }

    /// <summary>
    /// Abandons an exam.
    /// </summary>
    [HttpPost("{examId}/abandon")]
    public async Task<ActionResult> AbandonExam(Guid examId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // Verify ownership
        var exam = await _context.LicenseExams
            .Include(le => le.PlayerWorld)
            .FirstOrDefaultAsync(le => le.Id == examId);

        if (exam == null)
            return NotFound(new { message = "Exam not found" });

        if (exam.PlayerWorld.UserId != userId)
            return Forbid();

        var success = await _examService.AbandonExamAsync(examId);

        if (!success)
            return BadRequest(new { message = "Cannot abandon this exam" });

        return Ok(new { success = true, message = "Exam abandoned" });
    }

    /// <summary>
    /// Gets exam details.
    /// </summary>
    [HttpGet("{examId}")]
    public async Task<ActionResult<ExamDetailsDto>> GetExam(Guid examId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var exam = await _examService.GetExamAsync(examId);

        if (exam == null)
            return NotFound(new { message = "Exam not found" });

        // Verify ownership
        var playerWorld = await _context.PlayerWorlds.FindAsync(exam.PlayerWorldId);
        if (playerWorld?.UserId != userId)
            return Forbid();

        return Ok(MapToDetailsDto(exam));
    }

    /// <summary>
    /// Gets player's exam history.
    /// </summary>
    [HttpGet("history/{worldId}")]
    public async Task<ActionResult<List<ExamSummaryDto>>> GetExamHistory(Guid worldId, [FromQuery] int limit = 20)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.UserId == userId && pw.WorldId == worldId);

        if (playerWorld == null)
            return NotFound(new { message = "Player not found in this world" });

        var exams = await _examService.GetPlayerExamsAsync(playerWorld.Id, limit);

        return Ok(exams.Select(MapToDto).ToList());
    }

    /// <summary>
    /// Gets the currently active exam for a player.
    /// </summary>
    [HttpGet("active/{worldId}")]
    public async Task<ActionResult<ExamSummaryDto?>> GetActiveExam(Guid worldId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.UserId == userId && pw.WorldId == worldId);

        if (playerWorld == null)
            return NotFound(new { message = "Player not found in this world" });

        var activeExam = await _context.LicenseExams
            .Include(le => le.LicenseType)
            .Where(le => le.PlayerWorldId == playerWorld.Id
                && (le.Status == ExamStatus.Scheduled || le.Status == ExamStatus.InProgress))
            .FirstOrDefaultAsync();

        if (activeExam == null)
            return Ok((ExamSummaryDto?)null);

        return Ok(MapToDto(activeExam));
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        return userId;
    }

    private static ExamSummaryDto MapToDto(Domain.Entities.LicenseExam exam) => new()
    {
        Id = exam.Id,
        LicenseCode = exam.LicenseType.Code,
        LicenseName = exam.LicenseType.Name,
        Status = exam.Status.ToString(),
        ScheduledAt = exam.ScheduledAt,
        StartedAt = exam.StartedAt,
        CompletedAt = exam.CompletedAt,
        DepartureIcao = exam.DepartureIcao,
        Score = exam.Score,
        PassingScore = exam.PassingScore,
        AttemptNumber = exam.AttemptNumber,
        FeePaid = exam.FeePaid,
        TimeLimitMinutes = exam.TimeLimitMinutes,
        FailureReason = exam.FailureReason
    };

    private static ExamDetailsDto MapToDetailsDto(Domain.Entities.LicenseExam exam) => new()
    {
        Id = exam.Id,
        LicenseCode = exam.LicenseType.Code,
        LicenseName = exam.LicenseType.Name,
        Status = exam.Status.ToString(),
        ScheduledAt = exam.ScheduledAt,
        StartedAt = exam.StartedAt,
        CompletedAt = exam.CompletedAt,
        DepartureIcao = exam.DepartureIcao,
        RouteJson = exam.RouteJson,
        AssignedAltitudeFt = exam.AssignedAltitudeFt,
        Score = exam.Score,
        PassingScore = exam.PassingScore,
        AttemptNumber = exam.AttemptNumber,
        FeePaid = exam.FeePaid,
        TimeLimitMinutes = exam.TimeLimitMinutes,
        FailureReason = exam.FailureReason,
        ExaminerNotes = exam.ExaminerNotes,
        FlightTimeMinutes = exam.FlightTimeMinutes,
        AircraftUsed = exam.AircraftUsed,
        Maneuvers = exam.Maneuvers.Select(m => new ManeuverDto
        {
            ManeuverType = m.ManeuverType,
            Order = m.Order,
            IsRequired = m.IsRequired,
            MaxPoints = m.MaxPoints,
            PointsAwarded = m.PointsAwarded,
            Result = m.Result.ToString()
        }).ToList(),
        Checkpoints = exam.Checkpoints.Select(c => new CheckpointDto
        {
            Name = c.Name,
            Order = c.Order,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
            WasReached = c.WasReached,
            PointsAwarded = c.PointsAwarded,
            MaxPoints = c.MaxPoints
        }).ToList(),
        Landings = exam.Landings.Select(l => new LandingDto
        {
            Order = l.Order,
            AirportIcao = l.AirportIcao,
            Type = l.Type.ToString(),
            VerticalSpeedFpm = l.VerticalSpeedFpm,
            CenterlineDeviationFt = l.CenterlineDeviationFt,
            PointsAwarded = l.PointsAwarded,
            MaxPoints = l.MaxPoints,
            Notes = l.Notes
        }).ToList(),
        Violations = exam.Violations.Select(v => new ViolationDto
        {
            Type = v.Type.ToString(),
            Value = v.Value,
            PointsDeducted = v.PointsDeducted,
            CausedFailure = v.CausedFailure,
            Description = v.Description
        }).ToList()
    };
}

#region Request/Response DTOs

public class ScheduleExamRequest
{
    public Guid WorldId { get; set; }
    public string LicenseCode { get; set; } = string.Empty;
    public string DepartureIcao { get; set; } = string.Empty;
}

public class StartExamRequest
{
    public string AircraftUsed { get; set; } = string.Empty;
}

public class RecordViolationRequest
{
    public ViolationType Type { get; set; }
    public float Value { get; set; }
    public float Threshold { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int? Altitude { get; set; }
}

public class RecordLandingRequest
{
    public string AirportIcao { get; set; } = string.Empty;
    public LandingType Type { get; set; }
    public float VerticalSpeedFpm { get; set; }
    public float CenterlineDeviationFt { get; set; }
    public float TouchdownZoneDistanceFt { get; set; }
    public float? GroundSpeedKts { get; set; }
    public float? PitchDeg { get; set; }
    public float? BankDeg { get; set; }
    public bool GearDown { get; set; } = true;
    public string? RunwayUsed { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public float? AltitudeAtTouchdown { get; set; }
}

public class ReachCheckpointRequest
{
    public int Altitude { get; set; }
    public int Speed { get; set; }
}

public class ExamScheduleResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ExamSummaryDto? Exam { get; set; }
}

public class ExamStartResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ExamSummaryDto? Exam { get; set; }
}

public class ExamCompleteResponse
{
    public bool Success { get; set; }
    public int Score { get; set; }
    public bool Passed { get; set; }
    public ExamSummaryDto? Exam { get; set; }
    public LicenseGrantedDto? License { get; set; }
}

public class ExamSummaryDto
{
    public Guid Id { get; set; }
    public string LicenseCode { get; set; } = string.Empty;
    public string LicenseName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string DepartureIcao { get; set; } = string.Empty;
    public int Score { get; set; }
    public int PassingScore { get; set; }
    public int AttemptNumber { get; set; }
    public decimal FeePaid { get; set; }
    public int TimeLimitMinutes { get; set; }
    public string? FailureReason { get; set; }
}

public class ExamDetailsDto : ExamSummaryDto
{
    public string? RouteJson { get; set; }
    public int? AssignedAltitudeFt { get; set; }
    public string? ExaminerNotes { get; set; }
    public int? FlightTimeMinutes { get; set; }
    public string? AircraftUsed { get; set; }
    public List<ManeuverDto> Maneuvers { get; set; } = new();
    public List<CheckpointDto> Checkpoints { get; set; } = new();
    public List<LandingDto> Landings { get; set; } = new();
    public List<ViolationDto> Violations { get; set; } = new();
}

public class ManeuverDto
{
    public string ManeuverType { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public int MaxPoints { get; set; }
    public int PointsAwarded { get; set; }
    public string Result { get; set; } = string.Empty;
}

public class CheckpointDto
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool WasReached { get; set; }
    public int PointsAwarded { get; set; }
    public int MaxPoints { get; set; }
}

public class LandingDto
{
    public int Order { get; set; }
    public string AirportIcao { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public float VerticalSpeedFpm { get; set; }
    public float CenterlineDeviationFt { get; set; }
    public int PointsAwarded { get; set; }
    public int MaxPoints { get; set; }
    public string? Notes { get; set; }
}

public class ViolationDto
{
    public string Type { get; set; } = string.Empty;
    public float Value { get; set; }
    public int PointsDeducted { get; set; }
    public bool CausedFailure { get; set; }
    public string? Description { get; set; }
}

public class LicenseGrantedDto
{
    public Guid Id { get; set; }
    public string LicenseCode { get; set; } = string.Empty;
    public string LicenseName { get; set; } = string.Empty;
    public DateTimeOffset? ExpiresAt { get; set; }
}

#endregion
