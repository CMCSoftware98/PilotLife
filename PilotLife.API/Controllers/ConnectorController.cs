using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PilotLife.Application.FlightTracking;
using PilotLife.Application.FlightTracking.DTOs;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConnectorController : ControllerBase
{
    private readonly IFlightTrackingService _flightTrackingService;
    private readonly ILogger<ConnectorController> _logger;

    public ConnectorController(IFlightTrackingService flightTrackingService, ILogger<ConnectorController> logger)
    {
        _flightTrackingService = flightTrackingService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }

    /// <summary>
    /// Starts tracking a new flight session.
    /// Called by the connector when a simulator session begins.
    /// </summary>
    [HttpPost("flight/start")]
    [ProducesResponseType(typeof(FlightUpdateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FlightUpdateResponse>> StartFlight([FromBody] FlightStartData data, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation("User {UserId} starting flight with session {SessionId}", userId, data.SessionId);

            var response = await _flightTrackingService.StartFlightAsync(userId, data, cancellationToken);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    /// <summary>
    /// Updates the current flight state and position.
    /// Called periodically by the connector during flight.
    /// </summary>
    [HttpPost("flight/update")]
    [ProducesResponseType(typeof(FlightUpdateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlightUpdateResponse>> UpdateFlight([FromBody] FlightStateUpdate update, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var response = await _flightTrackingService.UpdateFlightAsync(userId, update, cancellationToken);

            if (!response.Success)
            {
                if (response.Message?.Contains("No active flight") == true)
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    /// <summary>
    /// Ends the current flight session.
    /// Called by the connector when the flight ends (shutdown or disconnect).
    /// </summary>
    [HttpPost("flight/end")]
    [ProducesResponseType(typeof(FlightUpdateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlightUpdateResponse>> EndFlight([FromBody] FlightEndData data, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation("User {UserId} ending flight with session {SessionId}", userId, data.SessionId);

            var response = await _flightTrackingService.EndFlightAsync(userId, data, cancellationToken);

            if (!response.Success)
            {
                if (response.Message?.Contains("No active flight") == true)
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    /// <summary>
    /// Gets the user's active flight, if any.
    /// </summary>
    [HttpGet("flight/active")]
    [ProducesResponseType(typeof(ActiveFlightResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActiveFlightResponse>> GetActiveFlight(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var flight = await _flightTrackingService.GetActiveFlightAsync(userId, cancellationToken);

            if (flight == null)
            {
                return NotFound(new { message = "No active flight" });
            }

            return Ok(new ActiveFlightResponse
            {
                FlightId = flight.Id,
                SessionId = flight.ConnectorSessionId,
                State = flight.State.ToString(),
                AircraftTitle = flight.AircraftTitle,
                DepartureIcao = flight.DepartureIcao,
                DepartureTime = flight.DepartureTime,
                CurrentLatitude = flight.CurrentLatitude,
                CurrentLongitude = flight.CurrentLongitude,
                CurrentAltitude = flight.CurrentAltitude,
                CurrentHeading = flight.CurrentHeading,
                CurrentGroundSpeed = flight.CurrentGroundSpeed,
                LastPositionUpdate = flight.LastPositionUpdate,
                FlightTimeMinutes = flight.FlightTimeMinutes,
                AssignedJobCount = flight.FlightJobs.Count
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    /// <summary>
    /// Cancels the user's active flight.
    /// </summary>
    [HttpPost("flight/cancel")]
    [ProducesResponseType(typeof(FlightUpdateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlightUpdateResponse>> CancelFlight([FromBody] CancelFlightRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var flight = await _flightTrackingService.GetActiveFlightAsync(userId, cancellationToken);

            if (flight == null)
            {
                return NotFound(new FlightUpdateResponse
                {
                    Success = false,
                    Message = "No active flight to cancel"
                });
            }

            var success = await _flightTrackingService.CancelFlightAsync(flight.Id, request?.Reason, cancellationToken);

            return Ok(new FlightUpdateResponse
            {
                Success = success,
                FlightId = flight.Id,
                State = "Cancelled",
                Message = success ? "Flight cancelled" : "Failed to cancel flight"
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    /// <summary>
    /// Assigns jobs to the user's active flight.
    /// </summary>
    [HttpPost("flight/assign-jobs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AssignJobs([FromBody] AssignJobsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var flight = await _flightTrackingService.GetActiveFlightAsync(userId, cancellationToken);

            if (flight == null)
            {
                return NotFound(new { message = "No active flight" });
            }

            var success = await _flightTrackingService.AssignJobsToFlightAsync(flight.Id, request.JobIds, cancellationToken);

            if (!success)
            {
                return BadRequest(new { message = "Failed to assign jobs" });
            }

            return Ok(new { message = $"Assigned {request.JobIds.Count} jobs to flight" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }
}

// Request/Response DTOs
public record CancelFlightRequest
{
    public string? Reason { get; init; }
}

public record AssignJobsRequest
{
    public required List<Guid> JobIds { get; init; }
}

public record ActiveFlightResponse
{
    public Guid FlightId { get; init; }
    public string? SessionId { get; init; }
    public required string State { get; init; }
    public string? AircraftTitle { get; init; }
    public string? DepartureIcao { get; init; }
    public DateTimeOffset? DepartureTime { get; init; }
    public double? CurrentLatitude { get; init; }
    public double? CurrentLongitude { get; init; }
    public double? CurrentAltitude { get; init; }
    public double? CurrentHeading { get; init; }
    public double? CurrentGroundSpeed { get; init; }
    public DateTimeOffset? LastPositionUpdate { get; init; }
    public int FlightTimeMinutes { get; init; }
    public int AssignedJobCount { get; init; }
}
