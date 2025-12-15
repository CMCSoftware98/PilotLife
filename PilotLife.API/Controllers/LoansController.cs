using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.API.Services;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly PilotLifeDbContext _context;
    private readonly ILoanService _loanService;
    private readonly ICreditScoreService _creditScoreService;
    private readonly ILogger<LoansController> _logger;

    public LoansController(
        PilotLifeDbContext context,
        ILoanService loanService,
        ICreditScoreService creditScoreService,
        ILogger<LoansController> logger)
    {
        _context = context;
        _loanService = loanService;
        _creditScoreService = creditScoreService;
        _logger = logger;
    }

    // GET /api/loans
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanResponse>>> GetMyLoans()
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        var loans = await _loanService.GetPlayerLoansAsync(playerWorld.Id);
        var response = loans.Select(MapToLoanResponse);

        return Ok(response);
    }

    // GET /api/loans/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LoanDetailResponse>> GetLoan(Guid id)
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        var loan = await _loanService.GetLoanAsync(id);

        if (loan == null)
        {
            return NotFound(new { message = "Loan not found" });
        }

        if (loan.PlayerWorldId != playerWorld.Id)
        {
            return Forbid();
        }

        return Ok(MapToLoanDetailResponse(loan));
    }

    // GET /api/loans/summary
    [HttpGet("summary")]
    public async Task<ActionResult<LoanSummaryResponse>> GetLoanSummary()
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        var summary = await _loanService.GetLoanSummaryAsync(playerWorld.Id);

        return Ok(new LoanSummaryResponse
        {
            TotalLoans = summary.TotalLoans,
            ActiveLoans = summary.ActiveLoans,
            PaidOffLoans = summary.PaidOffLoans,
            DefaultedLoans = summary.DefaultedLoans,
            TotalDebt = summary.TotalDebt,
            TotalMonthlyPayment = summary.TotalMonthlyPayment,
            NextPaymentDue = summary.NextPaymentDue?.ToString("O"),
            HasStarterLoan = summary.HasStarterLoan
        });
    }

    // GET /api/loans/starter-loan/eligibility
    [HttpGet("starter-loan/eligibility")]
    public async Task<ActionResult<StarterLoanEligibilityResponse>> CheckStarterLoanEligibility()
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        var eligibility = await _loanService.CheckStarterLoanEligibilityAsync(playerWorld.Id);

        return Ok(new StarterLoanEligibilityResponse
        {
            IsEligible = eligibility.IsEligible,
            Reason = eligibility.Reason,
            RequiresCpl = eligibility.RequiresCpl,
            MaxAmount = eligibility.MaxAmount,
            InterestRate = eligibility.InterestRate,
            InterestRatePercent = eligibility.InterestRate.HasValue ? eligibility.InterestRate.Value * 100 : null,
            AvailableTerms = eligibility.AvailableTerms,
            CurrentBalance = eligibility.CurrentBalance,
            CurrentNetWorth = eligibility.CurrentNetWorth,
            BankName = eligibility.BankName
        });
    }

    // POST /api/loans/starter-loan/apply
    [HttpPost("starter-loan/apply")]
    public async Task<ActionResult<LoanApplicationResponse>> ApplyForStarterLoan([FromBody] StarterLoanApplicationRequest request)
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        var result = await _loanService.ApplyForStarterLoanAsync(
            playerWorld.Id,
            request.Amount,
            request.TermMonths);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        _logger.LogInformation("User {UserId} applied for starter loan of ${Amount}",
            userId, request.Amount);

        return Ok(new LoanApplicationResponse
        {
            Success = true,
            Message = result.Message,
            Loan = result.Loan != null ? MapToLoanResponse(result.Loan) : null,
            NewBalance = playerWorld.Balance + request.Amount
        });
    }

    // POST /api/loans/apply
    [HttpPost("apply")]
    public async Task<ActionResult<LoanApplicationResponse>> ApplyForLoan([FromBody] LoanApplicationRequest request)
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        if (!Enum.TryParse<LoanType>(request.LoanType, out var loanType))
        {
            return BadRequest(new { message = "Invalid loan type" });
        }

        Guid? collateralId = null;
        if (!string.IsNullOrEmpty(request.CollateralAircraftId) &&
            Guid.TryParse(request.CollateralAircraftId, out var parsedId))
        {
            collateralId = parsedId;
        }

        var result = await _loanService.ApplyForLoanAsync(
            playerWorld.Id,
            loanType,
            request.Amount,
            request.TermMonths,
            collateralId,
            request.Purpose);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        _logger.LogInformation("User {UserId} applied for {LoanType} loan of ${Amount}",
            userId, loanType, request.Amount);

        return Ok(new LoanApplicationResponse
        {
            Success = true,
            Message = result.Message,
            Loan = result.Loan != null ? MapToLoanResponse(result.Loan) : null,
            NewBalance = playerWorld.Balance + request.Amount
        });
    }

    // POST /api/loans/{id}/pay
    [HttpPost("{id:guid}/pay")]
    public async Task<ActionResult<LoanPaymentResponse>> MakePayment(Guid id, [FromBody] LoanPaymentRequest request)
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        // Verify loan belongs to player
        var loan = await _loanService.GetLoanAsync(id);
        if (loan == null)
        {
            return NotFound(new { message = "Loan not found" });
        }

        if (loan.PlayerWorldId != playerWorld.Id)
        {
            return Forbid();
        }

        var result = await _loanService.MakePaymentAsync(id, request.Amount);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        // Reload player world to get updated balance
        await _context.Entry(playerWorld).ReloadAsync();

        _logger.LogInformation("User {UserId} made payment of ${Amount} on loan {LoanId}",
            userId, request.Amount, id);

        return Ok(new LoanPaymentResponse
        {
            Success = true,
            Message = result.Message,
            Payment = result.Payment != null ? MapToPaymentResponse(result.Payment) : null,
            LoanPaidOff = result.LoanPaidOff,
            NewBalance = playerWorld.Balance
        });
    }

    // POST /api/loans/{id}/payoff
    [HttpPost("{id:guid}/payoff")]
    public async Task<ActionResult<LoanPaymentResponse>> PayOffLoan(Guid id)
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        // Verify loan belongs to player
        var loan = await _loanService.GetLoanAsync(id);
        if (loan == null)
        {
            return NotFound(new { message = "Loan not found" });
        }

        if (loan.PlayerWorldId != playerWorld.Id)
        {
            return Forbid();
        }

        var result = await _loanService.PayOffLoanAsync(id);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        // Reload player world to get updated balance
        await _context.Entry(playerWorld).ReloadAsync();

        _logger.LogInformation("User {UserId} paid off loan {LoanId}", userId, id);

        return Ok(new LoanPaymentResponse
        {
            Success = true,
            Message = result.Message,
            Payment = result.Payment != null ? MapToPaymentResponse(result.Payment) : null,
            LoanPaidOff = true,
            NewBalance = playerWorld.Balance
        });
    }

    // GET /api/loans/credit-score
    [HttpGet("credit-score")]
    public async Task<ActionResult<CreditScoreResponse>> GetCreditScore()
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        var breakdown = await _creditScoreService.GetCreditBreakdownAsync(playerWorld.Id);

        return Ok(new CreditScoreResponse
        {
            Score = breakdown.CurrentScore,
            Rating = breakdown.Rating,
            MinPossible = breakdown.MinPossible,
            MaxPossible = breakdown.MaxPossible,
            ActiveLoans = breakdown.ActiveLoans,
            TotalDebt = breakdown.TotalDebt,
            PaidOffLoans = breakdown.PaidOffLoans,
            DefaultedLoans = breakdown.DefaultedLoans,
            OnTimePaymentPercent = breakdown.OnTimePaymentPercent,
            RecentPositiveChanges = breakdown.RecentPositiveChanges,
            RecentNegativeChanges = breakdown.RecentNegativeChanges,
            LastUpdated = breakdown.LastUpdated.ToString("O")
        });
    }

    // GET /api/loans/credit-score/history
    [HttpGet("credit-score/history")]
    public async Task<ActionResult<IEnumerable<CreditScoreEventResponse>>> GetCreditHistory([FromQuery] int limit = 50)
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        var history = await _creditScoreService.GetCreditHistoryAsync(playerWorld.Id, limit);
        var response = history.Select(MapToCreditEventResponse);

        return Ok(response);
    }

    // GET /api/loans/banks
    [HttpGet("banks")]
    public async Task<ActionResult<IEnumerable<BankResponse>>> GetBanks()
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        var banks = await _context.Banks
            .Where(b => b.WorldId == playerWorld.WorldId && b.IsActive)
            .ToListAsync();

        var response = banks.Select(MapToBankResponse);

        return Ok(response);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private async Task<PlayerWorld?> GetCurrentPlayerWorld(Guid userId)
    {
        return await _context.PlayerWorlds
            .Where(pw => pw.UserId == userId)
            .OrderByDescending(pw => pw.LastActiveAt)
            .FirstOrDefaultAsync();
    }

    private static LoanResponse MapToLoanResponse(Loan loan)
    {
        return new LoanResponse
        {
            Id = loan.Id.ToString(),
            LoanType = loan.LoanType.ToString(),
            Status = loan.Status.ToString(),
            PrincipalAmount = loan.PrincipalAmount,
            InterestRatePerMonth = loan.InterestRatePerMonth,
            InterestRatePercent = loan.InterestRatePerMonth * 100,
            TermMonths = loan.TermMonths,
            MonthlyPayment = loan.MonthlyPayment,
            TotalRepaymentAmount = loan.TotalRepaymentAmount,
            RemainingPrincipal = loan.RemainingPrincipal,
            TotalPaid = loan.TotalPaid,
            PaymentsMade = loan.PaymentsMade,
            PaymentsRemaining = loan.PaymentsRemaining,
            NextPaymentDue = loan.NextPaymentDue?.ToString("O"),
            ApprovedAt = loan.ApprovedAt?.ToString("O"),
            PaidOffAt = loan.PaidOffAt?.ToString("O"),
            Purpose = loan.Purpose,
            BankName = loan.Bank?.Name,
            CollateralAircraftRegistration = loan.CollateralAircraft?.Registration
        };
    }

    private static LoanDetailResponse MapToLoanDetailResponse(Loan loan)
    {
        return new LoanDetailResponse
        {
            Id = loan.Id.ToString(),
            LoanType = loan.LoanType.ToString(),
            Status = loan.Status.ToString(),
            PrincipalAmount = loan.PrincipalAmount,
            InterestRatePerMonth = loan.InterestRatePerMonth,
            InterestRatePercent = loan.InterestRatePerMonth * 100,
            TermMonths = loan.TermMonths,
            MonthlyPayment = loan.MonthlyPayment,
            TotalRepaymentAmount = loan.TotalRepaymentAmount,
            RemainingPrincipal = loan.RemainingPrincipal,
            AccruedInterest = loan.AccruedInterest,
            TotalPaid = loan.TotalPaid,
            PaymentsMade = loan.PaymentsMade,
            PaymentsRemaining = loan.PaymentsRemaining,
            LatePaymentCount = loan.LatePaymentCount,
            MissedPaymentCount = loan.MissedPaymentCount,
            NextPaymentDue = loan.NextPaymentDue?.ToString("O"),
            ApprovedAt = loan.ApprovedAt?.ToString("O"),
            DisbursedAt = loan.DisbursedAt?.ToString("O"),
            PaidOffAt = loan.PaidOffAt?.ToString("O"),
            DefaultedAt = loan.DefaultedAt?.ToString("O"),
            Purpose = loan.Purpose,
            Notes = loan.Notes,
            BankName = loan.Bank?.Name,
            CollateralAircraftRegistration = loan.CollateralAircraft?.Registration,
            CollateralAircraftTitle = loan.CollateralAircraft?.Aircraft?.Title,
            PayoffAmount = loan.Status == LoanStatus.Active ? loan.GetPayoffAmount() : 0,
            Payments = loan.Payments.Select(MapToPaymentResponse).ToList(),
            CreatedAt = loan.CreatedAt.ToString("O")
        };
    }

    private static PaymentResponse MapToPaymentResponse(LoanPayment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id.ToString(),
            PaymentNumber = payment.PaymentNumber,
            Amount = payment.Amount,
            PrincipalPortion = payment.PrincipalPortion,
            InterestPortion = payment.InterestPortion,
            LateFee = payment.LateFee,
            RemainingBalanceAfter = payment.RemainingBalanceAfter,
            DueDate = payment.DueDate.ToString("O"),
            PaidAt = payment.PaidAt.ToString("O"),
            IsLate = payment.IsLate,
            DaysLate = payment.DaysLate
        };
    }

    private static CreditScoreEventResponse MapToCreditEventResponse(CreditScoreEvent evt)
    {
        return new CreditScoreEventResponse
        {
            Id = evt.Id.ToString(),
            EventType = evt.EventType.ToString(),
            ScoreBefore = evt.ScoreBefore,
            ScoreAfter = evt.ScoreAfter,
            ScoreChange = evt.ScoreChange,
            Description = evt.Description,
            OccurredAt = evt.CreatedAt.ToString("O")
        };
    }

    private static BankResponse MapToBankResponse(Bank bank)
    {
        return new BankResponse
        {
            Id = bank.Id.ToString(),
            Name = bank.Name,
            Description = bank.Description,
            OffersStarterLoan = bank.OffersStarterLoan,
            StarterLoanMaxAmount = bank.StarterLoanMaxAmount,
            StarterLoanInterestRatePercent = bank.StarterLoanInterestRate * 100,
            BaseInterestRatePercent = bank.BaseInterestRate * 100,
            MaxInterestRatePercent = bank.MaxInterestRate * 100,
            MinCreditScore = bank.MinCreditScore,
            MaxLoanToNetWorthRatio = bank.MaxLoanToNetWorthRatio,
            MinDownPaymentPercent = bank.MinDownPaymentPercent * 100,
            MaxTermMonths = bank.MaxTermMonths
        };
    }
}

// Request DTOs
public class StarterLoanApplicationRequest
{
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
}

public class LoanApplicationRequest
{
    public string LoanType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public string? Purpose { get; set; }
    public string? CollateralAircraftId { get; set; }
}

public class LoanPaymentRequest
{
    public decimal Amount { get; set; }
}

// Response DTOs
public class LoanResponse
{
    public string Id { get; set; } = string.Empty;
    public string LoanType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRatePerMonth { get; set; }
    public decimal InterestRatePercent { get; set; }
    public int TermMonths { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal TotalRepaymentAmount { get; set; }
    public decimal RemainingPrincipal { get; set; }
    public decimal TotalPaid { get; set; }
    public int PaymentsMade { get; set; }
    public int PaymentsRemaining { get; set; }
    public string? NextPaymentDue { get; set; }
    public string? ApprovedAt { get; set; }
    public string? PaidOffAt { get; set; }
    public string? Purpose { get; set; }
    public string? BankName { get; set; }
    public string? CollateralAircraftRegistration { get; set; }
}

public class LoanDetailResponse : LoanResponse
{
    public decimal AccruedInterest { get; set; }
    public int LatePaymentCount { get; set; }
    public int MissedPaymentCount { get; set; }
    public string? DisbursedAt { get; set; }
    public string? DefaultedAt { get; set; }
    public string? Notes { get; set; }
    public string? CollateralAircraftTitle { get; set; }
    public decimal PayoffAmount { get; set; }
    public List<PaymentResponse> Payments { get; set; } = new();
    public string CreatedAt { get; set; } = string.Empty;
}

public class LoanSummaryResponse
{
    public int TotalLoans { get; set; }
    public int ActiveLoans { get; set; }
    public int PaidOffLoans { get; set; }
    public int DefaultedLoans { get; set; }
    public decimal TotalDebt { get; set; }
    public decimal TotalMonthlyPayment { get; set; }
    public string? NextPaymentDue { get; set; }
    public bool HasStarterLoan { get; set; }
}

public class StarterLoanEligibilityResponse
{
    public bool IsEligible { get; set; }
    public string? Reason { get; set; }
    public bool RequiresCpl { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? InterestRate { get; set; }
    public decimal? InterestRatePercent { get; set; }
    public int[]? AvailableTerms { get; set; }
    public decimal? CurrentBalance { get; set; }
    public decimal? CurrentNetWorth { get; set; }
    public string? BankName { get; set; }
}

public class LoanApplicationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public LoanResponse? Loan { get; set; }
    public decimal NewBalance { get; set; }
}

public class LoanPaymentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PaymentResponse? Payment { get; set; }
    public bool LoanPaidOff { get; set; }
    public decimal NewBalance { get; set; }
}

public class PaymentResponse
{
    public string Id { get; set; } = string.Empty;
    public int PaymentNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal PrincipalPortion { get; set; }
    public decimal InterestPortion { get; set; }
    public decimal LateFee { get; set; }
    public decimal RemainingBalanceAfter { get; set; }
    public string DueDate { get; set; } = string.Empty;
    public string PaidAt { get; set; } = string.Empty;
    public bool IsLate { get; set; }
    public int DaysLate { get; set; }
}

public class CreditScoreResponse
{
    public int Score { get; set; }
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
    public string LastUpdated { get; set; } = string.Empty;
}

public class CreditScoreEventResponse
{
    public string Id { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int ScoreBefore { get; set; }
    public int ScoreAfter { get; set; }
    public int ScoreChange { get; set; }
    public string Description { get; set; } = string.Empty;
    public string OccurredAt { get; set; } = string.Empty;
}

public class BankResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool OffersStarterLoan { get; set; }
    public decimal StarterLoanMaxAmount { get; set; }
    public decimal StarterLoanInterestRatePercent { get; set; }
    public decimal BaseInterestRatePercent { get; set; }
    public decimal MaxInterestRatePercent { get; set; }
    public int MinCreditScore { get; set; }
    public decimal MaxLoanToNetWorthRatio { get; set; }
    public decimal MinDownPaymentPercent { get; set; }
    public int MaxTermMonths { get; set; }
}
