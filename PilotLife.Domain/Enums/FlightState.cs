namespace PilotLife.Domain.Enums;

/// <summary>
/// Represents the current state of a tracked flight.
/// </summary>
public enum FlightState
{
    /// <summary>
    /// Flight has been created but not yet started.
    /// </summary>
    Pending,

    /// <summary>
    /// Player is in the cockpit, preparing for flight (engines off or starting).
    /// </summary>
    PreFlight,

    /// <summary>
    /// Aircraft is taxiing on the ground.
    /// </summary>
    Taxiing,

    /// <summary>
    /// Aircraft is taking off or in initial climb.
    /// </summary>
    Departing,

    /// <summary>
    /// Aircraft is in cruise flight.
    /// </summary>
    EnRoute,

    /// <summary>
    /// Aircraft is on approach or landing.
    /// </summary>
    Arriving,

    /// <summary>
    /// Aircraft has landed, engines still running.
    /// </summary>
    Arrived,

    /// <summary>
    /// Aircraft is parked with engines off. Flight complete.
    /// </summary>
    Shutdown,

    /// <summary>
    /// Flight was cancelled or abandoned.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Flight ended due to a crash or critical failure.
    /// </summary>
    Failed
}
