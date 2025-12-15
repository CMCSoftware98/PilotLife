using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PilotLife.Application.Reputation;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services.Reputation;

public class ReputationService : IReputationService
{
    private readonly PilotLifeDbContext _context;
    private readonly ReputationConfiguration _config;
    private readonly ILogger<ReputationService> _logger;

    public ReputationService(
        PilotLifeDbContext context,
        IOptions<ReputationConfiguration> config,
        ILogger<ReputationService> logger)
    {
        _context = context;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<ReputationStatus> GetReputationStatusAsync(
        Guid playerWorldId,
        CancellationToken cancellationToken = default)
    {
        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId, cancellationToken);

        if (playerWorld == null)
        {
            throw new InvalidOperationException("Player world not found");
        }

        var level = GetReputationLevel(playerWorld.ReputationScore);
        var progressToNext = GetProgressToNextLevel(playerWorld.ReputationScore);
        var payoutBonus = GetPayoutBonus(level);

        return new ReputationStatus
        {
            PlayerWorldId = playerWorldId,
            Score = playerWorld.ReputationScore,
            Level = level,
            LevelName = GetLevelName(level),
            ProgressToNextLevel = progressToNext,
            OnTimeDeliveries = playerWorld.OnTimeDeliveries,
            LateDeliveries = playerWorld.LateDeliveries,
            FailedDeliveries = playerWorld.FailedDeliveries,
            JobCompletionRate = playerWorld.JobCompletionRate,
            OnTimeRate = playerWorld.OnTimeRate,
            PayoutBonus = payoutBonus,
            Benefits = GetBenefitsForLevel(level)
        };
    }

    public async Task<ReputationResult> AddReputationEventAsync(
        Guid playerWorldId,
        ReputationEventType eventType,
        string? description = null,
        Guid? relatedJobId = null,
        Guid? relatedFlightId = null,
        CancellationToken cancellationToken = default)
    {
        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId, cancellationToken);

        if (playerWorld == null)
        {
            return new ReputationResult
            {
                Success = false,
                NewScore = 0,
                PointChange = 0,
                NewLevel = 0,
                LevelChanged = false,
                Message = "Player world not found"
            };
        }

        var oldLevel = GetReputationLevel(playerWorld.ReputationScore);
        var pointChange = GetPointChangeForEvent(eventType);
        var newScore = Math.Clamp(
            playerWorld.ReputationScore + pointChange,
            _config.MinScore,
            _config.MaxScore);

        // Create the event
        var reputationEvent = new ReputationEvent
        {
            PlayerWorldId = playerWorldId,
            EventType = eventType,
            PointChange = pointChange,
            ResultingScore = newScore,
            Description = description ?? GetDefaultDescription(eventType),
            RelatedJobId = relatedJobId,
            RelatedFlightId = relatedFlightId
        };

        _context.ReputationEvents.Add(reputationEvent);

        // Update the player's score
        playerWorld.ReputationScore = newScore;
        playerWorld.LastActiveAt = DateTimeOffset.UtcNow;

        // Update delivery stats if job-related
        UpdateDeliveryStats(playerWorld, eventType);

        await _context.SaveChangesAsync(cancellationToken);

        var newLevel = GetReputationLevel(newScore);
        var levelChanged = newLevel != oldLevel;

        if (levelChanged)
        {
            _logger.LogInformation(
                "Player {PlayerWorldId} reputation level changed from {OldLevel} to {NewLevel}",
                playerWorldId, oldLevel, newLevel);
        }

        return new ReputationResult
        {
            Success = true,
            NewScore = newScore,
            PointChange = pointChange,
            NewLevel = newLevel,
            LevelChanged = levelChanged,
            Message = levelChanged
                ? $"Level {(newLevel > oldLevel ? "up" : "down")}! You are now {GetLevelName(newLevel)}"
                : null
        };
    }

    public async Task<IEnumerable<ReputationEvent>> GetReputationHistoryAsync(
        Guid playerWorldId,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.ReputationEvents
            .Where(e => e.PlayerWorldId == playerWorldId)
            .OrderByDescending(e => e.OccurredAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task ProcessFlightCompletedAsync(
        Guid playerWorldId,
        TrackedFlight flight,
        CancellationToken cancellationToken = default)
    {
        // Process landing quality
        if (flight.LandingRate.HasValue)
        {
            var landingRate = Math.Abs(flight.LandingRate.Value);

            if (landingRate < 100)
            {
                await AddReputationEventAsync(
                    playerWorldId,
                    ReputationEventType.SmoothLanding,
                    $"Smooth landing at {flight.ArrivalIcao} ({landingRate:F0} fpm)",
                    relatedFlightId: flight.Id,
                    cancellationToken: cancellationToken);
            }
            else if (landingRate < 200)
            {
                await AddReputationEventAsync(
                    playerWorldId,
                    ReputationEventType.GoodLanding,
                    $"Good landing at {flight.ArrivalIcao} ({landingRate:F0} fpm)",
                    relatedFlightId: flight.Id,
                    cancellationToken: cancellationToken);
            }
            else if (landingRate > 600)
            {
                await AddReputationEventAsync(
                    playerWorldId,
                    ReputationEventType.HardLanding,
                    $"Hard landing at {flight.ArrivalIcao} ({landingRate:F0} fpm)",
                    relatedFlightId: flight.Id,
                    cancellationToken: cancellationToken);
            }
        }

        // Process safety violations
        if (flight.OverspeedCount > 0)
        {
            await AddReputationEventAsync(
                playerWorldId,
                ReputationEventType.OverspeedViolation,
                $"Overspeed violation during flight ({flight.OverspeedCount} events)",
                relatedFlightId: flight.Id,
                cancellationToken: cancellationToken);
        }

        if (flight.StallWarningCount > 0)
        {
            await AddReputationEventAsync(
                playerWorldId,
                ReputationEventType.StallWarning,
                $"Stall warning during flight ({flight.StallWarningCount} events)",
                relatedFlightId: flight.Id,
                cancellationToken: cancellationToken);
        }

        // Process crash/accident
        if (flight.State == FlightState.Failed)
        {
            await AddReputationEventAsync(
                playerWorldId,
                ReputationEventType.Accident,
                "Flight ended in accident",
                relatedFlightId: flight.Id,
                cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessJobCompletedAsync(
        Guid playerWorldId,
        Job job,
        bool wasOnTime,
        bool wasEarly,
        CancellationToken cancellationToken = default)
    {
        if (job.IsFailed)
        {
            await AddReputationEventAsync(
                playerWorldId,
                ReputationEventType.JobFailed,
                $"Failed to complete job: {job.Title}",
                relatedJobId: job.Id,
                cancellationToken: cancellationToken);
            return;
        }

        ReputationEventType eventType;
        string description;

        if (wasEarly)
        {
            eventType = ReputationEventType.JobCompletedEarly;
            description = $"Delivered early: {job.Title}";
        }
        else if (wasOnTime)
        {
            eventType = ReputationEventType.JobCompletedOnTime;
            description = $"Delivered on time: {job.Title}";
        }
        else
        {
            eventType = ReputationEventType.JobCompletedLate;
            description = $"Delivered late: {job.Title}";
        }

        await AddReputationEventAsync(
            playerWorldId,
            eventType,
            description,
            relatedJobId: job.Id,
            cancellationToken: cancellationToken);

        // Bonus for high-risk jobs
        if (job.RiskLevel >= 4 && !job.IsFailed)
        {
            await AddReputationEventAsync(
                playerWorldId,
                ReputationEventType.HighRiskJobCompleted,
                $"Completed high-risk job: {job.Title}",
                relatedJobId: job.Id,
                cancellationToken: cancellationToken);
        }
    }

    public async Task<decimal> GetPayoutBonusAsync(
        Guid playerWorldId,
        CancellationToken cancellationToken = default)
    {
        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId, cancellationToken);

        if (playerWorld == null)
        {
            return 0;
        }

        var level = GetReputationLevel(playerWorld.ReputationScore);
        return GetPayoutBonus(level);
    }

    // Private helper methods

    private decimal GetPointChangeForEvent(ReputationEventType eventType) => eventType switch
    {
        ReputationEventType.JobCompletedOnTime => _config.JobOnTimeBonus,
        ReputationEventType.JobCompletedEarly => _config.JobEarlyBonus,
        ReputationEventType.JobCompletedLate => _config.JobLatePenalty,
        ReputationEventType.JobFailed => _config.JobFailedPenalty,
        ReputationEventType.JobCancelled => _config.JobCancelledPenalty,
        ReputationEventType.SmoothLanding => _config.SmoothLandingBonus,
        ReputationEventType.GoodLanding => _config.GoodLandingBonus,
        ReputationEventType.HardLanding => _config.HardLandingPenalty,
        ReputationEventType.OverspeedViolation => _config.OverspeedPenalty,
        ReputationEventType.StallWarning => _config.StallWarningPenalty,
        ReputationEventType.Accident => _config.AccidentPenalty,
        ReputationEventType.HighRiskJobCompleted => _config.HighRiskJobBonus,
        ReputationEventType.VipJobCompleted => _config.VipJobBonus,
        ReputationEventType.InactivityDecay => -_config.DecayRatePerDay,
        _ => 0
    };

    private int GetReputationLevel(decimal score) => score switch
    {
        < 1.0m => 1,
        < 2.0m => 2,
        < 3.0m => 3,
        < 4.0m => 4,
        _ => 5
    };

    private static string GetLevelName(int level) => level switch
    {
        1 => "Unreliable",
        2 => "Novice",
        3 => "Standard",
        4 => "Trusted",
        5 => "Elite",
        _ => "Unknown"
    };

    private decimal GetProgressToNextLevel(decimal score)
    {
        var level = GetReputationLevel(score);
        if (level >= 5) return 100;

        decimal currentThreshold = level switch
        {
            1 => 0,
            2 => _config.Level2Threshold,
            3 => _config.Level3Threshold,
            4 => _config.Level4Threshold,
            _ => 0
        };

        decimal nextThreshold = level switch
        {
            1 => _config.Level2Threshold,
            2 => _config.Level3Threshold,
            3 => _config.Level4Threshold,
            4 => _config.Level5Threshold,
            _ => _config.MaxScore
        };

        var progress = (score - currentThreshold) / (nextThreshold - currentThreshold) * 100;
        return Math.Min(100, Math.Max(0, progress));
    }

    private decimal GetPayoutBonus(int level) => level switch
    {
        4 => _config.TrustedPayoutBonus,
        5 => _config.ElitePayoutBonus,
        _ => 0
    };

    private static List<ReputationBenefit> GetBenefitsForLevel(int currentLevel)
    {
        return
        [
            new ReputationBenefit
            {
                Name = "Basic Jobs",
                Description = "Access to standard cargo and passenger jobs",
                IsUnlocked = currentLevel >= 2,
                RequiredLevel = 2
            },
            new ReputationBenefit
            {
                Name = "Priority Jobs",
                Description = "Access to priority and time-sensitive jobs",
                IsUnlocked = currentLevel >= 3,
                RequiredLevel = 3
            },
            new ReputationBenefit
            {
                Name = "10% Payout Bonus",
                Description = "Earn 10% more on all jobs",
                IsUnlocked = currentLevel >= 4,
                RequiredLevel = 4
            },
            new ReputationBenefit
            {
                Name = "VIP Jobs",
                Description = "Access to VIP and exclusive jobs",
                IsUnlocked = currentLevel >= 5,
                RequiredLevel = 5
            },
            new ReputationBenefit
            {
                Name = "20% Payout Bonus",
                Description = "Earn 20% more on all jobs",
                IsUnlocked = currentLevel >= 5,
                RequiredLevel = 5
            }
        ];
    }

    private static void UpdateDeliveryStats(PlayerWorld playerWorld, ReputationEventType eventType)
    {
        switch (eventType)
        {
            case ReputationEventType.JobCompletedOnTime:
            case ReputationEventType.JobCompletedEarly:
                playerWorld.OnTimeDeliveries++;
                break;
            case ReputationEventType.JobCompletedLate:
                playerWorld.LateDeliveries++;
                break;
            case ReputationEventType.JobFailed:
                playerWorld.FailedDeliveries++;
                break;
        }
    }

    private static string GetDefaultDescription(ReputationEventType eventType) => eventType switch
    {
        ReputationEventType.JobCompletedOnTime => "Job completed on time",
        ReputationEventType.JobCompletedEarly => "Job completed early",
        ReputationEventType.JobCompletedLate => "Job completed late",
        ReputationEventType.JobFailed => "Job failed",
        ReputationEventType.JobCancelled => "Job cancelled",
        ReputationEventType.SmoothLanding => "Smooth landing",
        ReputationEventType.GoodLanding => "Good landing",
        ReputationEventType.HardLanding => "Hard landing",
        ReputationEventType.OverspeedViolation => "Overspeed violation",
        ReputationEventType.StallWarning => "Stall warning",
        ReputationEventType.Accident => "Accident",
        ReputationEventType.HighRiskJobCompleted => "High-risk job completed",
        ReputationEventType.VipJobCompleted => "VIP job completed",
        ReputationEventType.InactivityDecay => "Inactivity decay",
        _ => "Reputation event"
    };
}
