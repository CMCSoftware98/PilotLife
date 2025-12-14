using Microsoft.EntityFrameworkCore;
using PilotLife.Application.FlightTracking;
using PilotLife.Application.FlightTracking.DTOs;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services;

public class FlightTrackingService : IFlightTrackingService
{
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<FlightTrackingService> _logger;

    // Thresholds for state determination
    private const double TaxiSpeedKts = 30;
    private const double ClimbAltitudeFt = 1000;
    private const double ApproachAltitudeFt = 3000;

    public FlightTrackingService(PilotLifeDbContext context, ILogger<FlightTrackingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FlightUpdateResponse> StartFlightAsync(Guid userId, FlightStartData data, CancellationToken cancellationToken = default)
    {
        // Check if user already has an active flight
        var existingFlight = await GetActiveFlightAsync(userId, cancellationToken);
        if (existingFlight != null)
        {
            _logger.LogWarning("User {UserId} already has an active flight {FlightId}", userId, existingFlight.Id);
            return new FlightUpdateResponse
            {
                Success = false,
                FlightId = existingFlight.Id,
                State = existingFlight.State.ToString(),
                Message = "User already has an active flight"
            };
        }

        // Find or create aircraft record
        var aircraft = await _context.Aircraft
            .FirstOrDefaultAsync(a => a.Title == data.AircraftTitle && a.IsApproved, cancellationToken);

        // Find departure airport
        Airport? departureAirport = null;
        if (!string.IsNullOrEmpty(data.NearestAirportIcao))
        {
            departureAirport = await _context.Airports
                .FirstOrDefaultAsync(a => a.Ident == data.NearestAirportIcao, cancellationToken);
        }

        if (departureAirport == null)
        {
            departureAirport = await FindNearestAirportAsync(data.Latitude, data.Longitude, 10, cancellationToken);
        }

        var flight = new TrackedFlight
        {
            UserId = userId,
            State = FlightState.PreFlight,
            AircraftId = aircraft?.Id,
            AircraftTitle = data.AircraftTitle,
            AircraftIcaoType = data.AircraftIcaoType,
            DepartureAirportId = departureAirport?.Id,
            DepartureIcao = departureAirport?.Ident ?? data.NearestAirportIcao,
            CurrentLatitude = data.Latitude,
            CurrentLongitude = data.Longitude,
            CurrentAltitude = data.Altitude,
            CurrentHeading = data.Heading,
            CurrentGroundSpeed = 0,
            LastPositionUpdate = DateTimeOffset.UtcNow,
            StartFuelGallons = data.FuelGallons,
            PayloadWeightLbs = data.PayloadWeightLbs,
            TotalWeightLbs = data.TotalWeightLbs,
            ConnectorSessionId = data.SessionId
        };

        _context.TrackedFlights.Add(flight);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Started flight {FlightId} for user {UserId} at {Airport}",
            flight.Id, userId, flight.DepartureIcao ?? "unknown");

        return new FlightUpdateResponse
        {
            Success = true,
            FlightId = flight.Id,
            State = flight.State.ToString(),
            Message = "Flight started"
        };
    }

    public async Task<FlightUpdateResponse> UpdateFlightAsync(Guid userId, FlightStateUpdate update, CancellationToken cancellationToken = default)
    {
        var flight = await GetFlightBySessionIdAsync(update.SessionId, cancellationToken);

        if (flight == null)
        {
            _logger.LogWarning("No flight found for session {SessionId}", update.SessionId);
            return new FlightUpdateResponse
            {
                Success = false,
                Message = "No active flight found for this session"
            };
        }

        if (flight.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to update flight {FlightId} owned by {OwnerId}",
                userId, flight.Id, flight.UserId);
            return new FlightUpdateResponse
            {
                Success = false,
                Message = "Flight does not belong to this user"
            };
        }

        // Update position
        flight.CurrentLatitude = update.Position.Latitude;
        flight.CurrentLongitude = update.Position.Longitude;
        flight.CurrentAltitude = update.Position.Altitude;
        flight.CurrentHeading = update.Position.Heading;
        flight.CurrentGroundSpeed = update.Position.GroundSpeed;
        flight.LastPositionUpdate = DateTimeOffset.UtcNow;

        // Update statistics
        if (update.Position.Altitude > flight.MaxAltitude)
            flight.MaxAltitude = update.Position.Altitude;

        if (update.Position.GroundSpeed > flight.MaxGroundSpeed)
            flight.MaxGroundSpeed = update.Position.GroundSpeed;

        // Track incidents
        if (update.Overspeed)
            flight.OverspeedCount++;

        if (update.StallWarning)
            flight.StallWarningCount++;

        // Determine and update flight state
        var newState = DetermineFlightState(update, flight.State);
        if (newState != flight.State)
        {
            _logger.LogInformation("Flight {FlightId} state changed from {OldState} to {NewState}",
                flight.Id, flight.State, newState);
            flight.State = newState;

            // Record departure time when we start taxiing/departing
            if (newState == FlightState.Taxiing && flight.DepartureTime == null)
            {
                flight.DepartureTime = DateTimeOffset.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new FlightUpdateResponse
        {
            Success = true,
            FlightId = flight.Id,
            State = flight.State.ToString(),
            Message = "Flight updated"
        };
    }

    public async Task<FlightUpdateResponse> EndFlightAsync(Guid userId, FlightEndData data, CancellationToken cancellationToken = default)
    {
        var flight = await GetFlightBySessionIdAsync(data.SessionId, cancellationToken);

        if (flight == null)
        {
            _logger.LogWarning("No flight found for session {SessionId}", data.SessionId);
            return new FlightUpdateResponse
            {
                Success = false,
                Message = "No active flight found for this session"
            };
        }

        if (flight.UserId != userId)
        {
            return new FlightUpdateResponse
            {
                Success = false,
                Message = "Flight does not belong to this user"
            };
        }

        // Update final position
        flight.CurrentLatitude = data.Latitude;
        flight.CurrentLongitude = data.Longitude;

        // Find arrival airport
        Airport? arrivalAirport = null;
        if (!string.IsNullOrEmpty(data.NearestAirportIcao))
        {
            arrivalAirport = await _context.Airports
                .FirstOrDefaultAsync(a => a.Ident == data.NearestAirportIcao, cancellationToken);
        }

        if (arrivalAirport == null)
        {
            arrivalAirport = await FindNearestAirportAsync(data.Latitude, data.Longitude, 10, cancellationToken);
        }

        flight.ArrivalAirportId = arrivalAirport?.Id;
        flight.ArrivalIcao = arrivalAirport?.Ident ?? data.NearestAirportIcao;
        flight.ArrivalTime = DateTimeOffset.UtcNow;
        flight.CompletedAt = DateTimeOffset.UtcNow;

        // Update fuel and landing data
        flight.EndFuelGallons = data.FuelGallons;
        if (flight.StartFuelGallons.HasValue)
        {
            flight.FuelUsedGallons = flight.StartFuelGallons.Value - data.FuelGallons;
        }

        flight.LandingRate = data.LandingRate;

        // Track hard landing
        if (data.LandingRate.HasValue && data.LandingRate.Value < -600)
        {
            flight.HardLandingCount++;
        }

        // Calculate flight time
        if (flight.DepartureTime.HasValue)
        {
            flight.FlightTimeMinutes = (int)(flight.CompletedAt.Value - flight.DepartureTime.Value).TotalMinutes;
        }

        // Calculate distance
        if (flight.DepartureAirport != null && arrivalAirport != null)
        {
            flight.DistanceNm = CalculateDistanceNm(
                flight.DepartureAirport.Latitude, flight.DepartureAirport.Longitude,
                arrivalAirport.Latitude, arrivalAirport.Longitude);
        }

        // Set final state
        flight.State = data.WasCrash ? FlightState.Failed : FlightState.Shutdown;

        // Update user's location and flight time
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user != null && arrivalAirport != null)
        {
            user.CurrentAirportId = arrivalAirport.Id;
            user.TotalFlightMinutes += flight.FlightTimeMinutes;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Flight {FlightId} ended with state {State} at {Airport}. Duration: {Minutes} min",
            flight.Id, flight.State, flight.ArrivalIcao ?? "unknown", flight.FlightTimeMinutes);

        return new FlightUpdateResponse
        {
            Success = true,
            FlightId = flight.Id,
            State = flight.State.ToString(),
            Message = data.WasCrash ? "Flight ended (crash)" : "Flight completed successfully"
        };
    }

    public async Task<TrackedFlight?> GetActiveFlightAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.TrackedFlights
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .Include(f => f.Aircraft)
            .Include(f => f.FlightJobs)
                .ThenInclude(fj => fj.Job)
            .FirstOrDefaultAsync(f =>
                f.UserId == userId &&
                f.State != FlightState.Shutdown &&
                f.State != FlightState.Cancelled &&
                f.State != FlightState.Failed,
                cancellationToken);
    }

    public async Task<TrackedFlight?> GetFlightByIdAsync(Guid flightId, CancellationToken cancellationToken = default)
    {
        return await _context.TrackedFlights
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .Include(f => f.Aircraft)
            .Include(f => f.FlightJobs)
                .ThenInclude(fj => fj.Job)
            .Include(f => f.Financials)
            .FirstOrDefaultAsync(f => f.Id == flightId, cancellationToken);
    }

    public async Task<TrackedFlight?> GetFlightBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.TrackedFlights
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .Include(f => f.Aircraft)
            .FirstOrDefaultAsync(f => f.ConnectorSessionId == sessionId, cancellationToken);
    }

    public async Task<bool> AssignJobsToFlightAsync(Guid flightId, IEnumerable<Guid> jobIds, CancellationToken cancellationToken = default)
    {
        var flight = await _context.TrackedFlights.FindAsync([flightId], cancellationToken);
        if (flight == null || flight.State == FlightState.Shutdown ||
            flight.State == FlightState.Cancelled || flight.State == FlightState.Failed)
        {
            return false;
        }

        var jobs = await _context.Jobs
            .Where(j => jobIds.Contains(j.Id) && !j.IsCompleted)
            .ToListAsync(cancellationToken);

        foreach (var job in jobs)
        {
            var flightJob = new FlightJob
            {
                TrackedFlightId = flightId,
                JobId = job.Id
            };
            _context.FlightJobs.Add(flightJob);

            job.AssignedToUserId = flight.UserId;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assigned {Count} jobs to flight {FlightId}", jobs.Count, flightId);
        return true;
    }

    public async Task<bool> CancelFlightAsync(Guid flightId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var flight = await _context.TrackedFlights.FindAsync([flightId], cancellationToken);
        if (flight == null || flight.State == FlightState.Shutdown ||
            flight.State == FlightState.Cancelled || flight.State == FlightState.Failed)
        {
            return false;
        }

        flight.State = FlightState.Cancelled;
        flight.CompletedAt = DateTimeOffset.UtcNow;
        flight.Notes = reason;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Flight {FlightId} cancelled. Reason: {Reason}", flightId, reason ?? "None");
        return true;
    }

    public FlightState DetermineFlightState(FlightStateUpdate update, FlightState currentState)
    {
        // Don't regress to earlier states (except for landing)
        if (currentState == FlightState.Shutdown || currentState == FlightState.Cancelled ||
            currentState == FlightState.Failed)
        {
            return currentState;
        }

        var onGround = update.OnGround;
        var speed = update.Position.GroundSpeed;
        var altitude = update.Position.Altitude;
        var engineRunning = update.EngineRunning;

        // Engine off on ground = PreFlight or Shutdown
        if (!engineRunning && onGround)
        {
            return currentState == FlightState.Pending ? FlightState.PreFlight : FlightState.Shutdown;
        }

        // On ground with engine running
        if (onGround)
        {
            if (speed < TaxiSpeedKts)
            {
                // Just landed or taxiing slowly
                if (currentState == FlightState.Arriving || currentState == FlightState.Arrived)
                {
                    return FlightState.Arrived;
                }
                return currentState == FlightState.PreFlight ? FlightState.PreFlight : FlightState.Taxiing;
            }
            else
            {
                // High speed on ground = takeoff or landing roll
                if (currentState == FlightState.Arriving || currentState == FlightState.EnRoute)
                {
                    return FlightState.Arriving; // Landing roll
                }
                return FlightState.Departing; // Takeoff roll
            }
        }

        // In the air
        if (altitude < ClimbAltitudeFt)
        {
            // Low altitude
            if (currentState == FlightState.EnRoute || currentState == FlightState.Arriving)
            {
                return FlightState.Arriving; // Descending for landing
            }
            return FlightState.Departing; // Initial climb
        }
        else if (altitude < ApproachAltitudeFt)
        {
            // Medium altitude
            if (currentState == FlightState.EnRoute && update.Position.VerticalSpeed < -200)
            {
                return FlightState.Arriving; // Beginning descent
            }
            return currentState == FlightState.Departing ? FlightState.Departing : FlightState.EnRoute;
        }
        else
        {
            // High altitude = en route
            return FlightState.EnRoute;
        }
    }

    public async Task<Airport?> FindNearestAirportAsync(double latitude, double longitude, double maxDistanceNm = 10, CancellationToken cancellationToken = default)
    {
        // Convert max distance to approximate degrees (1 degree ≈ 60nm at equator)
        var maxDegrees = maxDistanceNm / 60.0;

        var candidates = await _context.Airports
            .Where(a =>
                a.Latitude >= latitude - maxDegrees &&
                a.Latitude <= latitude + maxDegrees &&
                a.Longitude >= longitude - maxDegrees &&
                a.Longitude <= longitude + maxDegrees)
            .ToListAsync(cancellationToken);

        Airport? nearest = null;
        var minDistance = double.MaxValue;

        foreach (var airport in candidates)
        {
            var distance = CalculateDistanceNm(latitude, longitude, airport.Latitude, airport.Longitude);
            if (distance < minDistance && distance <= maxDistanceNm)
            {
                minDistance = distance;
                nearest = airport;
            }
        }

        return nearest;
    }

    private static double CalculateDistanceNm(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula
        const double earthRadiusNm = 3440.065; // nautical miles

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusNm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
