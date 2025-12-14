using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a flight being tracked by the PilotLife Connector.
/// Tracks real-time flight data from the simulator.
/// </summary>
public class TrackedFlight : BaseEntity
{
    /// <summary>
    /// The user flying this flight.
    /// </summary>
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// Current state of the flight.
    /// </summary>
    public FlightState State { get; set; } = FlightState.Pending;

    /// <summary>
    /// The aircraft being used (template reference).
    /// </summary>
    public Guid? AircraftId { get; set; }
    public Aircraft? Aircraft { get; set; }

    /// <summary>
    /// Aircraft title as reported by the simulator.
    /// </summary>
    public string? AircraftTitle { get; set; }

    /// <summary>
    /// ICAO type code of the aircraft.
    /// </summary>
    public string? AircraftIcaoType { get; set; }

    // Departure information
    public int? DepartureAirportId { get; set; }
    public Airport? DepartureAirport { get; set; }
    public string? DepartureIcao { get; set; }
    public DateTimeOffset? DepartureTime { get; set; }

    // Arrival information
    public int? ArrivalAirportId { get; set; }
    public Airport? ArrivalAirport { get; set; }
    public string? ArrivalIcao { get; set; }
    public DateTimeOffset? ArrivalTime { get; set; }

    // Current position (updated in real-time)
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public double? CurrentAltitude { get; set; }
    public double? CurrentHeading { get; set; }
    public double? CurrentGroundSpeed { get; set; }
    public DateTimeOffset? LastPositionUpdate { get; set; }

    // Flight statistics
    public int FlightTimeMinutes { get; set; }
    public double DistanceNm { get; set; }
    public double MaxAltitude { get; set; }
    public double MaxGroundSpeed { get; set; }

    // Flight quality metrics
    public int HardLandingCount { get; set; }
    public int OverspeedCount { get; set; }
    public int StallWarningCount { get; set; }
    public double? LandingRate { get; set; } // feet per minute (negative = descent)

    // Fuel tracking
    public double? FuelUsedGallons { get; set; }
    public double? StartFuelGallons { get; set; }
    public double? EndFuelGallons { get; set; }

    // Payload tracking
    public double? PayloadWeightLbs { get; set; }
    public double? TotalWeightLbs { get; set; }

    /// <summary>
    /// When the flight was completed (state changed to Shutdown or Failed).
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Notes or error messages about the flight.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Session ID from the connector for correlation.
    /// </summary>
    public string? ConnectorSessionId { get; set; }

    // Navigation property for associated jobs
    public ICollection<FlightJob> FlightJobs { get; set; } = new List<FlightJob>();

    // Navigation property for financials
    public FlightFinancials? Financials { get; set; }
}
