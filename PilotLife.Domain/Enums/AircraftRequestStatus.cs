namespace PilotLife.Domain.Enums;

/// <summary>
/// Status of an aircraft submission request.
/// </summary>
public enum AircraftRequestStatus
{
    /// <summary>
    /// Request is awaiting review.
    /// </summary>
    Pending,

    /// <summary>
    /// Request has been approved.
    /// </summary>
    Approved,

    /// <summary>
    /// Request has been rejected.
    /// </summary>
    Rejected
}
