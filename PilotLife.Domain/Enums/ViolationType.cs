namespace PilotLife.Domain.Enums;

/// <summary>
/// Types of violations that can occur during an exam.
/// </summary>
public enum ViolationType
{
    /// <summary>
    /// Exceeded speed limits (e.g., >250kts below 10,000ft).
    /// </summary>
    SpeedExcess,

    /// <summary>
    /// Deviated from assigned altitude beyond tolerance.
    /// </summary>
    AltitudeDeviation,

    /// <summary>
    /// Deviated from assigned heading beyond tolerance.
    /// </summary>
    HeadingDeviation,

    /// <summary>
    /// Exceeded G-force limits.
    /// </summary>
    GForceExcess,

    /// <summary>
    /// Landing with excessive vertical speed.
    /// </summary>
    HardLanding,

    /// <summary>
    /// Deviated from runway centerline beyond tolerance.
    /// </summary>
    CenterlineDeviation,

    /// <summary>
    /// Missed a required checkpoint or waypoint.
    /// </summary>
    MissedCheckpoint,

    /// <summary>
    /// Exceeded the time limit for the exam.
    /// </summary>
    TimeExceeded,

    /// <summary>
    /// Crashed the aircraft.
    /// </summary>
    Crash,

    /// <summary>
    /// Landed with gear up.
    /// </summary>
    GearUpLanding,

    /// <summary>
    /// Stalled the aircraft during a maneuver.
    /// </summary>
    Stall,

    /// <summary>
    /// Entered a spin.
    /// </summary>
    Spin
}
