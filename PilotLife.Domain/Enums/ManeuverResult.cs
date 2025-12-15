namespace PilotLife.Domain.Enums;

/// <summary>
/// Result of an exam maneuver.
/// </summary>
public enum ManeuverResult
{
    /// <summary>
    /// Maneuver has not been attempted yet.
    /// </summary>
    NotAttempted,

    /// <summary>
    /// Maneuver was passed successfully.
    /// </summary>
    Pass,

    /// <summary>
    /// Maneuver was failed.
    /// </summary>
    Fail
}
