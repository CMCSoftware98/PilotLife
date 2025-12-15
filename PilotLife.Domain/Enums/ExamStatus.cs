namespace PilotLife.Domain.Enums;

/// <summary>
/// Status of a license exam through its lifecycle.
/// </summary>
public enum ExamStatus
{
    /// <summary>
    /// Exam has been scheduled but not started.
    /// </summary>
    Scheduled,

    /// <summary>
    /// Exam is currently in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// Exam was passed successfully.
    /// </summary>
    Passed,

    /// <summary>
    /// Exam was failed.
    /// </summary>
    Failed,

    /// <summary>
    /// Exam was abandoned by the player.
    /// </summary>
    Abandoned,

    /// <summary>
    /// Exam expired before being started (scheduling timeout).
    /// </summary>
    Expired
}
