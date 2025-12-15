using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services;

public interface ICreditScoreService
{
    Task<int> GetCreditScoreAsync(Guid playerWorldId, CancellationToken ct = default);
    Task<IEnumerable<CreditScoreEvent>> GetCreditHistoryAsync(Guid playerWorldId, int limit = 50, CancellationToken ct = default);
    Task InitializeCreditScoreAsync(Guid playerWorldId, CancellationToken ct = default);
    Task RecordLoanOpenedAsync(Guid playerWorldId, Guid loanId, CancellationToken ct = default);
    Task RecordOnTimePaymentAsync(Guid playerWorldId, Guid loanId, CancellationToken ct = default);
    Task RecordLatePaymentAsync(Guid playerWorldId, Guid loanId, int daysLate, CancellationToken ct = default);
    Task RecordMissedPaymentAsync(Guid playerWorldId, Guid loanId, CancellationToken ct = default);
    Task RecordLoanPaidOffAsync(Guid playerWorldId, Guid loanId, CancellationToken ct = default);
    Task RecordLoanDefaultedAsync(Guid playerWorldId, Guid loanId, CancellationToken ct = default);
    Task RecordJobCompletedAsync(Guid playerWorldId, Guid jobId, CancellationToken ct = default);
    Task RecordJobFailedAsync(Guid playerWorldId, Guid jobId, CancellationToken ct = default);
    Task ProcessTimeRecoveryAsync(Guid worldId, CancellationToken ct = default);
    Task<CreditScoreBreakdown> GetCreditBreakdownAsync(Guid playerWorldId, CancellationToken ct = default);
}

public class CreditScoreService : ICreditScoreService
{
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<CreditScoreService> _logger;

    // Credit score constants
    private const int MinScore = 300;
    private const int MaxScore = 850;
    private const int InitialScore = 650;
    private const int NaturalRecoveryCap = 650;

    // Point values for various events
    private const int OnTimePaymentPoints = 5;
    private const int LatePaymentPenalty = 15;
    private const int MissedPaymentPenalty = 50;
    private const int LoanPaidOffBonus = 25;
    private const int LoanDefaultedPenalty = 150;
    private const int LoanOpenedPenalty = 5; // Small penalty for new debt
    private const int JobCompletedPoints = 2;
    private const int JobFailedPenalty = 10;
    private const int TimeRecoveryPoints = 1;

    public CreditScoreService(PilotLifeDbContext context, ILogger<CreditScoreService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> GetCreditScoreAsync(Guid playerWorldId, CancellationToken ct = default)
    {
        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId, ct);

        return playerWorld?.CreditScore ?? InitialScore;
    }

    public async Task<IEnumerable<CreditScoreEvent>> GetCreditHistoryAsync(Guid playerWorldId, int limit = 50, CancellationToken ct = default)
    {
        return await _context.CreditScoreEvents
            .Where(e => e.PlayerWorldId == playerWorldId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task InitializeCreditScoreAsync(Guid playerWorldId, CancellationToken ct = default)
    {
        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId, ct);

        if (playerWorld == null) return;

        // Set initial credit score
        playerWorld.CreditScore = InitialScore;

        // Record the initial event
        var initialEvent = CreditScoreEvent.CreateInitial(playerWorldId, playerWorld.WorldId, InitialScore);
        _context.CreditScoreEvents.Add(initialEvent);

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Initialized credit score for player {PlayerWorldId} to {Score}",
            playerWorldId, InitialScore);
    }

    public async Task RecordLoanOpenedAsync(Guid playerWorldId, Guid loanId, CancellationToken ct = default)
    {
        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId, ct);

        if (playerWorld == null) return;

        // Small penalty for taking on new debt
        var scoreBefore = playerWorld.CreditScore;
        var scoreAfter = Math.Max(MinScore, scoreBefore - LoanOpenedPenalty);

        playerWorld.CreditScore = scoreAfter;

        var creditEvent = new CreditScoreEvent
        {
            PlayerWorldId = playerWorldId,
            WorldId = playerWorld.WorldId,
            EventType = CreditScoreEventType.LoanOpened,
            ScoreBefore = scoreBefore,
            ScoreAfter = scoreAfter,
            ScoreChange = scoreAfter - scoreBefore,
            Description = "New loan opened",
            RelatedLoanId = loanId
        };

        _context.CreditScoreEvents.Add(creditEvent);
        await _context.SaveChangesAsync(ct);

        _logger.LogDebug("Credit score for {PlayerWorldId} changed from {Before} to {After} (loan opened)",
            playerWorldId, scoreBefore, scoreAfter);
    }

    public async Task RecordOnTimePaymentAsync(Guid playerWorldId, Guid loanId, CancellationToken ct = default)
    {
        await RecordCreditEventAsync(
            playerWorldId,
            CreditScoreEventType.PaymentOnTime,
            OnTimePaymentPoints,
            "On-time loan payment",
            loanId,
            ct);
    }

    public async Task RecordLatePaymentAsync(Guid playerWorldId, Guid loanId, int daysLate, CancellationToken ct = default)
    {
        // Scale penalty based on how late
        var penalty = LatePaymentPenalty + (daysLate * 2);

        await RecordCreditEventAsync(
            playerWorldId,
            CreditScoreEventType.PaymentLate,
            -penalty,
            $"Late loan payment ({daysLate} days late)",
            loanId,
            ct);
    }

    public async Task RecordMissedPaymentAsync(Guid playerWorldId, Guid loanId, CancellationToken ct = default)
    {
        await RecordCreditEventAsync(
            playerWorldId,
            CreditScoreEventType.PaymentMissed,
            -MissedPaymentPenalty,
            "Missed loan payment",
            loanId,
            ct);
    }

    public async Task RecordLoanPaidOffAsync(Guid playerWorldId, Guid loanId, CancellationToken ct = default)
    {
        await RecordCreditEventAsync(
            playerWorldId,
            CreditScoreEventType.LoanPaidOff,
            LoanPaidOffBonus,
            "Loan fully paid off",
            loanId,
            ct);
    }

    public async Task RecordLoanDefaultedAsync(Guid playerWorldId, Guid loanId, CancellationToken ct = default)
    {
        await RecordCreditEventAsync(
            playerWorldId,
            CreditScoreEventType.LoanDefaulted,
            -LoanDefaultedPenalty,
            "Loan defaulted",
            loanId,
            ct);
    }

    public async Task RecordJobCompletedAsync(Guid playerWorldId, Guid jobId, CancellationToken ct = default)
    {
        await RecordCreditEventAsync(
            playerWorldId,
            CreditScoreEventType.JobCompleted,
            JobCompletedPoints,
            "Job completed successfully",
            null,
            ct,
            jobId);
    }

    public async Task RecordJobFailedAsync(Guid playerWorldId, Guid jobId, CancellationToken ct = default)
    {
        await RecordCreditEventAsync(
            playerWorldId,
            CreditScoreEventType.JobFailed,
            -JobFailedPenalty,
            "Job failed or abandoned",
            null,
            ct,
            jobId);
    }

    public async Task ProcessTimeRecoveryAsync(Guid worldId, CancellationToken ct = default)
    {
        // Get world settings for recovery multiplier
        var world = await _context.Worlds
            .FirstOrDefaultAsync(w => w.Id == worldId, ct);

        if (world == null) return;

        // Find players with credit scores below the natural recovery cap
        var playersNeedingRecovery = await _context.PlayerWorlds
            .Where(pw => pw.WorldId == worldId &&
                        pw.IsActive &&
                        pw.CreditScore < NaturalRecoveryCap)
            .ToListAsync(ct);

        foreach (var playerWorld in playersNeedingRecovery)
        {
            var recoveryPoints = (int)(TimeRecoveryPoints * world.CreditRecoveryMultiplier);
            var scoreBefore = playerWorld.CreditScore;
            var scoreAfter = Math.Min(NaturalRecoveryCap, scoreBefore + recoveryPoints);

            if (scoreAfter > scoreBefore)
            {
                playerWorld.CreditScore = scoreAfter;

                var creditEvent = new CreditScoreEvent
                {
                    PlayerWorldId = playerWorld.Id,
                    WorldId = worldId,
                    EventType = CreditScoreEventType.TimeRecovery,
                    ScoreBefore = scoreBefore,
                    ScoreAfter = scoreAfter,
                    ScoreChange = scoreAfter - scoreBefore,
                    Description = "Time-based credit recovery"
                };

                _context.CreditScoreEvents.Add(creditEvent);
            }
        }

        if (playersNeedingRecovery.Any())
        {
            await _context.SaveChangesAsync(ct);
            _logger.LogDebug("Processed time recovery for {Count} players in world {WorldId}",
                playersNeedingRecovery.Count, worldId);
        }
    }

    public async Task<CreditScoreBreakdown> GetCreditBreakdownAsync(Guid playerWorldId, CancellationToken ct = default)
    {
        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId, ct);

        if (playerWorld == null)
        {
            return new CreditScoreBreakdown { CurrentScore = InitialScore };
        }

        // Get recent credit events for analysis
        var recentEvents = await _context.CreditScoreEvents
            .Where(e => e.PlayerWorldId == playerWorldId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(100)
            .ToListAsync(ct);

        // Get loan statistics
        var loans = await _context.Loans
            .Where(l => l.PlayerWorldId == playerWorldId)
            .ToListAsync(ct);

        var activeLoans = loans.Where(l => l.Status == LoanStatus.Active).ToList();
        var paidOffLoans = loans.Count(l => l.Status == LoanStatus.PaidOff);
        var defaultedLoans = loans.Count(l => l.Status == LoanStatus.Defaulted);

        // Calculate payment history
        var totalPayments = recentEvents.Count(e => e.EventType == CreditScoreEventType.PaymentOnTime ||
                                                    e.EventType == CreditScoreEventType.PaymentLate);
        var onTimePayments = recentEvents.Count(e => e.EventType == CreditScoreEventType.PaymentOnTime);
        var paymentHistoryPercent = totalPayments > 0
            ? (double)onTimePayments / totalPayments * 100
            : 100;

        // Determine credit rating
        var rating = playerWorld.CreditScore switch
        {
            >= 800 => "Excellent",
            >= 740 => "Very Good",
            >= 670 => "Good",
            >= 580 => "Fair",
            _ => "Poor"
        };

        return new CreditScoreBreakdown
        {
            CurrentScore = playerWorld.CreditScore,
            Rating = rating,
            MinPossible = MinScore,
            MaxPossible = MaxScore,
            ActiveLoans = activeLoans.Count,
            TotalDebt = activeLoans.Sum(l => l.RemainingPrincipal),
            PaidOffLoans = paidOffLoans,
            DefaultedLoans = defaultedLoans,
            OnTimePaymentPercent = paymentHistoryPercent,
            RecentPositiveChanges = recentEvents.Where(e => e.ScoreChange > 0).Sum(e => e.ScoreChange),
            RecentNegativeChanges = recentEvents.Where(e => e.ScoreChange < 0).Sum(e => Math.Abs(e.ScoreChange)),
            LastUpdated = recentEvents.FirstOrDefault()?.CreatedAt ?? playerWorld.JoinedAt
        };
    }

    private async Task RecordCreditEventAsync(
        Guid playerWorldId,
        CreditScoreEventType eventType,
        int pointChange,
        string description,
        Guid? loanId,
        CancellationToken ct,
        Guid? jobId = null)
    {
        var playerWorld = await _context.PlayerWorlds
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId, ct);

        if (playerWorld == null) return;

        var scoreBefore = playerWorld.CreditScore;
        var scoreAfter = pointChange > 0
            ? Math.Min(MaxScore, scoreBefore + pointChange)
            : Math.Max(MinScore, scoreBefore + pointChange);

        playerWorld.CreditScore = scoreAfter;

        var creditEvent = new CreditScoreEvent
        {
            PlayerWorldId = playerWorldId,
            WorldId = playerWorld.WorldId,
            EventType = eventType,
            ScoreBefore = scoreBefore,
            ScoreAfter = scoreAfter,
            ScoreChange = scoreAfter - scoreBefore,
            Description = description,
            RelatedLoanId = loanId,
            RelatedJobId = jobId
        };

        _context.CreditScoreEvents.Add(creditEvent);
        await _context.SaveChangesAsync(ct);

        _logger.LogDebug("Credit score for {PlayerWorldId} changed from {Before} to {After} ({EventType})",
            playerWorldId, scoreBefore, scoreAfter, eventType);
    }
}

public class CreditScoreBreakdown
{
    public int CurrentScore { get; set; }
    public string Rating { get; set; } = string.Empty;
    public int MinPossible { get; set; }
    public int MaxPossible { get; set; }
    public int ActiveLoans { get; set; }
    public decimal TotalDebt { get; set; }
    public int PaidOffLoans { get; set; }
    public int DefaultedLoans { get; set; }
    public double OnTimePaymentPercent { get; set; }
    public int RecentPositiveChanges { get; set; }
    public int RecentNegativeChanges { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}
