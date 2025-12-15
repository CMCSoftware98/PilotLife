namespace PilotLife.Application.Reputation;

/// <summary>
/// Configuration options for the reputation system.
/// </summary>
public class ReputationConfiguration
{
    public const string SectionName = "Reputation";

    // Base values
    /// <summary>
    /// Starting reputation score for new players.
    /// Default: 3.0 (Standard level)
    /// </summary>
    public decimal BaseScore { get; set; } = 3.0m;

    /// <summary>
    /// Minimum possible reputation score.
    /// </summary>
    public decimal MinScore { get; set; } = 0.0m;

    /// <summary>
    /// Maximum possible reputation score.
    /// </summary>
    public decimal MaxScore { get; set; } = 5.0m;

    // Job-related bonuses/penalties
    /// <summary>
    /// Reputation bonus for completing a job on time.
    /// </summary>
    public decimal JobOnTimeBonus { get; set; } = 0.1m;

    /// <summary>
    /// Reputation bonus for completing a job early.
    /// </summary>
    public decimal JobEarlyBonus { get; set; } = 0.15m;

    /// <summary>
    /// Reputation penalty for completing a job late.
    /// </summary>
    public decimal JobLatePenalty { get; set; } = -0.1m;

    /// <summary>
    /// Reputation penalty for failing a job.
    /// </summary>
    public decimal JobFailedPenalty { get; set; } = -0.3m;

    /// <summary>
    /// Reputation penalty for cancelling an accepted job.
    /// </summary>
    public decimal JobCancelledPenalty { get; set; } = -0.15m;

    // Landing-related bonuses/penalties
    /// <summary>
    /// Reputation bonus for a smooth landing (< 100 fpm).
    /// </summary>
    public decimal SmoothLandingBonus { get; set; } = 0.02m;

    /// <summary>
    /// Reputation bonus for a good landing (100-200 fpm).
    /// </summary>
    public decimal GoodLandingBonus { get; set; } = 0.01m;

    /// <summary>
    /// Reputation penalty for a hard landing (> 600 fpm).
    /// </summary>
    public decimal HardLandingPenalty { get; set; } = -0.05m;

    // Safety-related penalties
    /// <summary>
    /// Reputation penalty for an overspeed event.
    /// </summary>
    public decimal OverspeedPenalty { get; set; } = -0.03m;

    /// <summary>
    /// Reputation penalty for a stall warning.
    /// </summary>
    public decimal StallWarningPenalty { get; set; } = -0.02m;

    /// <summary>
    /// Reputation penalty for a crash/accident.
    /// </summary>
    public decimal AccidentPenalty { get; set; } = -0.5m;

    // Special bonuses
    /// <summary>
    /// Bonus for completing a high-risk job.
    /// </summary>
    public decimal HighRiskJobBonus { get; set; } = 0.05m;

    /// <summary>
    /// Bonus for completing a VIP passenger job.
    /// </summary>
    public decimal VipJobBonus { get; set; } = 0.1m;

    // Decay
    /// <summary>
    /// Daily decay rate towards base score (when inactive).
    /// </summary>
    public decimal DecayRatePerDay { get; set; } = 0.01m;

    /// <summary>
    /// Days of inactivity before decay starts.
    /// </summary>
    public int DecayGracePeriodDays { get; set; } = 7;

    // Level thresholds
    /// <summary>
    /// Score threshold for Level 2 (Novice).
    /// </summary>
    public decimal Level2Threshold { get; set; } = 1.0m;

    /// <summary>
    /// Score threshold for Level 3 (Standard).
    /// </summary>
    public decimal Level3Threshold { get; set; } = 2.0m;

    /// <summary>
    /// Score threshold for Level 4 (Trusted).
    /// </summary>
    public decimal Level4Threshold { get; set; } = 3.0m;

    /// <summary>
    /// Score threshold for Level 5 (Elite).
    /// </summary>
    public decimal Level5Threshold { get; set; } = 4.0m;

    // Benefits
    /// <summary>
    /// Payout bonus percentage for Trusted level (level 4).
    /// </summary>
    public decimal TrustedPayoutBonus { get; set; } = 10m;

    /// <summary>
    /// Payout bonus percentage for Elite level (level 5).
    /// </summary>
    public decimal ElitePayoutBonus { get; set; } = 20m;
}
