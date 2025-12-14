using PilotLife.Application.FlightTracking.DTOs;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.Application.FlightTracking;

/// <summary>
/// Service for tracking flights from the PilotLife Connector.
/// Handles flight lifecycle from start to completion.
/// </summary>
public interface IFlightTrackingService
{
    /// <summary>
    /// Starts tracking a new flight session.
    /// Called when the connector detects a new simulator session.
    /// </summary>
    Task<FlightUpdateResponse> StartFlightAsync(Guid userId, FlightStartData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the current flight state and position.
    /// Called periodically by the connector during flight.
    /// </summary>
    Task<FlightUpdateResponse> UpdateFlightAsync(Guid userId, FlightStateUpdate update, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends the current flight session.
    /// Called when the connector detects engine shutdown or disconnect.
    /// </summary>
    Task<FlightUpdateResponse> EndFlightAsync(Guid userId, FlightEndData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active flight for a user, if any.
    /// </summary>
    Task<TrackedFlight?> GetActiveFlightAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a flight by its ID.
    /// </summary>
    Task<TrackedFlight?> GetFlightByIdAsync(Guid flightId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a flight by its connector session ID.
    /// </summary>
    Task<TrackedFlight?> GetFlightBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns jobs to an active flight.
    /// </summary>
    Task<bool> AssignJobsToFlightAsync(Guid flightId, IEnumerable<Guid> jobIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an active flight.
    /// </summary>
    Task<bool> CancelFlightAsync(Guid flightId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines the appropriate flight state based on current conditions.
    /// </summary>
    FlightState DetermineFlightState(FlightStateUpdate update, FlightState currentState);

    /// <summary>
    /// Finds the nearest airport to the given coordinates.
    /// </summary>
    Task<Airport?> FindNearestAirportAsync(double latitude, double longitude, double maxDistanceNm = 10, CancellationToken cancellationToken = default);
}
