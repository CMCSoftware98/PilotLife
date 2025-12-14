namespace PilotLife.Application.FlightTracking.DTOs;

/// <summary>
/// Data received from the connector for flight position updates.
/// </summary>
public record FlightPositionUpdate
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double Altitude { get; init; }
    public double Heading { get; init; }
    public double GroundSpeed { get; init; }
    public double VerticalSpeed { get; init; }
    public bool OnGround { get; init; }
}

/// <summary>
/// Data received from the connector when a flight session starts.
/// </summary>
public record FlightStartData
{
    public required string SessionId { get; init; }
    public required string AircraftTitle { get; init; }
    public string? AircraftIcaoType { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double Altitude { get; init; }
    public double Heading { get; init; }
    public double FuelGallons { get; init; }
    public double PayloadWeightLbs { get; init; }
    public double TotalWeightLbs { get; init; }
    public string? NearestAirportIcao { get; init; }
}

/// <summary>
/// Data received from the connector when a flight ends.
/// </summary>
public record FlightEndData
{
    public required string SessionId { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double FuelGallons { get; init; }
    public double? LandingRate { get; init; }
    public string? NearestAirportIcao { get; init; }
    public bool WasCrash { get; init; }
}

/// <summary>
/// Data received from the connector for periodic flight state updates.
/// </summary>
public record FlightStateUpdate
{
    public required string SessionId { get; init; }
    public required FlightPositionUpdate Position { get; init; }
    public double FuelGallons { get; init; }
    public bool EngineRunning { get; init; }
    public bool GearDown { get; init; }
    public bool Overspeed { get; init; }
    public bool StallWarning { get; init; }
    public bool OnGround { get; init; }
    public int SimulationRate { get; init; }
}

/// <summary>
/// Response to connector after processing an update.
/// </summary>
public record FlightUpdateResponse
{
    public bool Success { get; init; }
    public Guid? FlightId { get; init; }
    public string? State { get; init; }
    public string? Message { get; init; }
}
