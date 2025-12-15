using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PilotLife.API.DTOs;
using PilotLife.Application.Maintenance;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(
        IMaintenanceService maintenanceService,
        ILogger<MaintenanceController> logger)
    {
        _maintenanceService = maintenanceService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the maintenance status for an owned aircraft.
    /// </summary>
    [HttpGet("{aircraftId:guid}/status")]
    public async Task<ActionResult<MaintenanceStatusResponse>> GetMaintenanceStatus(Guid aircraftId)
    {
        try
        {
            var userId = GetUserId();
            var status = await _maintenanceService.GetMaintenanceStatusAsync(aircraftId, userId);

            return Ok(MapToStatusResponse(status));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets available maintenance options for an aircraft.
    /// </summary>
    [HttpGet("{aircraftId:guid}/options")]
    public async Task<ActionResult<IEnumerable<MaintenanceOptionResponse>>> GetAvailableOptions(Guid aircraftId)
    {
        try
        {
            var userId = GetUserId();
            var options = await _maintenanceService.GetAvailableMaintenanceAsync(aircraftId, userId);

            return Ok(options.Select(MapToOptionResponse));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets maintenance history for an aircraft.
    /// </summary>
    [HttpGet("{aircraftId:guid}/history")]
    public async Task<ActionResult<IEnumerable<MaintenanceLogResponse>>> GetMaintenanceHistory(
        Guid aircraftId,
        [FromQuery] int limit = 50)
    {
        try
        {
            var userId = GetUserId();
            var history = await _maintenanceService.GetMaintenanceHistoryAsync(aircraftId, userId, limit);

            return Ok(history.Select(MapToLogResponse));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Schedules maintenance for an aircraft.
    /// </summary>
    [HttpPost("{aircraftId:guid}/schedule")]
    public async Task<ActionResult<MaintenanceResultResponse>> ScheduleMaintenance(
        Guid aircraftId,
        [FromBody] ScheduleMaintenanceRequest request)
    {
        var userId = GetUserId();

        if (!Enum.TryParse<MaintenanceType>(request.MaintenanceType, out var maintenanceType))
        {
            return BadRequest(new { message = "Invalid maintenance type" });
        }

        Guid? componentId = null;
        if (!string.IsNullOrEmpty(request.ComponentId) && Guid.TryParse(request.ComponentId, out var parsedComponentId))
        {
            componentId = parsedComponentId;
        }

        var result = await _maintenanceService.ScheduleMaintenanceAsync(
            aircraftId,
            maintenanceType,
            userId,
            componentId);

        if (!result.Success)
        {
            return BadRequest(MapToResultResponse(result));
        }

        _logger.LogInformation("User {UserId} scheduled {MaintenanceType} for aircraft {AircraftId}",
            userId, maintenanceType, aircraftId);

        return Ok(MapToResultResponse(result));
    }

    /// <summary>
    /// Cancels scheduled maintenance.
    /// </summary>
    [HttpPost("{maintenanceId:guid}/cancel")]
    public async Task<ActionResult<MaintenanceResultResponse>> CancelMaintenance(Guid maintenanceId)
    {
        var userId = GetUserId();
        var result = await _maintenanceService.CancelMaintenanceAsync(maintenanceId, userId);

        if (!result.Success)
        {
            return BadRequest(MapToResultResponse(result));
        }

        _logger.LogInformation("User {UserId} cancelled maintenance {MaintenanceId}", userId, maintenanceId);

        return Ok(MapToResultResponse(result));
    }

    /// <summary>
    /// Gets the active maintenance job for an aircraft.
    /// </summary>
    [HttpGet("{aircraftId:guid}/active")]
    public async Task<ActionResult<ActiveMaintenanceResponse?>> GetActiveMaintenance(Guid aircraftId)
    {
        var activeMaintenance = await _maintenanceService.GetActiveMaintenanceAsync(aircraftId);

        if (activeMaintenance == null)
        {
            return Ok(null);
        }

        return Ok(MapToActiveMaintenanceResponse(activeMaintenance));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    // ========================================
    // Mapping Methods
    // ========================================

    private static MaintenanceStatusResponse MapToStatusResponse(MaintenanceStatusResult status)
    {
        return new MaintenanceStatusResponse
        {
            AircraftId = status.OwnedAircraftId.ToString(),
            OverallCondition = status.OverallCondition,
            IsAirworthy = status.IsAirworthy,
            IsInMaintenance = status.IsInMaintenance,
            HoursSinceLastInspection = status.HoursSinceLastInspection,
            HoursSinceLastInspectionHours = status.HoursSinceLastInspection / 60.0,
            InspectionDue = status.InspectionDue,
            AnnualInspectionDue = status.AnnualInspectionDue,
            LastInspectionDate = status.LastInspectionDate?.ToString("O"),
            LastAnnualInspectionDate = status.LastAnnualInspectionDate?.ToString("O"),
            ActiveMaintenance = status.ActiveMaintenance != null
                ? MapToActiveMaintenanceResponse(status.ActiveMaintenance)
                : null,
            Components = status.Components.Select(c => new ComponentStatusResponse
            {
                ComponentId = c.ComponentId.ToString(),
                ComponentType = c.ComponentType.ToString(),
                Condition = c.Condition,
                TboPercentUsed = c.TboPercentUsed,
                LifePercentUsed = c.LifePercentUsed,
                NeedsAttention = c.NeedsAttention,
                IsServiceable = c.IsServiceable
            }).ToList(),
            Alerts = status.Alerts.Select(a => new MaintenanceAlertResponse
            {
                Severity = a.Severity,
                Message = a.Message,
                RecommendedAction = a.RecommendedAction,
                ComponentId = a.ComponentId?.ToString()
            }).ToList()
        };
    }

    private static ActiveMaintenanceResponse MapToActiveMaintenanceResponse(Domain.Entities.MaintenanceLog maintenance)
    {
        var estimatedCompletion = maintenance.StartedAt.AddHours(maintenance.EstimatedDurationHours);
        var elapsed = DateTimeOffset.UtcNow - maintenance.StartedAt;
        var progress = Math.Min(100, (int)(elapsed.TotalHours / maintenance.EstimatedDurationHours * 100));

        return new ActiveMaintenanceResponse
        {
            Id = maintenance.Id.ToString(),
            MaintenanceType = maintenance.MaintenanceType.ToString(),
            Title = maintenance.Title,
            StartedAt = maintenance.StartedAt.ToString("O"),
            EstimatedDurationHours = maintenance.EstimatedDurationHours,
            EstimatedCompletionAt = estimatedCompletion.ToString("O"),
            ProgressPercent = progress
        };
    }

    private static MaintenanceOptionResponse MapToOptionResponse(MaintenanceOption option)
    {
        return new MaintenanceOptionResponse
        {
            MaintenanceType = option.MaintenanceType.ToString(),
            Title = option.Title,
            Description = option.Description,
            EstimatedCost = option.EstimatedCost,
            EstimatedDurationHours = option.EstimatedDurationHours,
            ConditionImprovement = option.ConditionImprovement,
            RequiredForAirworthy = option.RequiredForAirworthy,
            ComponentId = option.ComponentId?.ToString(),
            WarrantyCoverage = option.WarrantyCoverage,
            InsuranceCoverage = option.InsuranceCoverage
        };
    }

    private static MaintenanceResultResponse MapToResultResponse(MaintenanceResult result)
    {
        return new MaintenanceResultResponse
        {
            Success = result.Success,
            MaintenanceLogId = result.MaintenanceLogId?.ToString(),
            Message = result.Message,
            EstimatedCompletion = result.EstimatedCompletion?.ToString("O"),
            TotalCost = result.TotalCost,
            OutOfPocketCost = result.OutOfPocketCost
        };
    }

    private static MaintenanceLogResponse MapToLogResponse(Domain.Entities.MaintenanceLog log)
    {
        return new MaintenanceLogResponse
        {
            Id = log.Id.ToString(),
            MaintenanceType = log.MaintenanceType.ToString(),
            Title = log.Title,
            Description = log.Description,
            PerformedAtAirport = log.PerformedAtAirport,
            FacilityName = log.FacilityName,
            StartedAt = log.StartedAt.ToString("O"),
            CompletedAt = log.CompletedAt?.ToString("O"),
            EstimatedDurationHours = log.EstimatedDurationHours,
            ActualDurationHours = log.ActualDurationHours,
            LaborCost = log.LaborCost,
            PartsCost = log.PartsCost,
            TotalCost = log.TotalCost,
            CoveredByWarranty = log.CoveredByWarranty,
            CoveredByInsurance = log.CoveredByInsurance,
            IsCompleted = log.IsCompleted,
            ConditionImprovement = log.ConditionImprovement,
            ResultingCondition = log.ResultingCondition,
            AircraftFlightMinutesAtService = log.AircraftFlightMinutesAtService,
            AircraftFlightHoursAtService = log.AircraftFlightMinutesAtService / 60.0,
            ComponentType = log.AircraftComponent?.ComponentType.ToString(),
            Notes = log.Notes
        };
    }
}
