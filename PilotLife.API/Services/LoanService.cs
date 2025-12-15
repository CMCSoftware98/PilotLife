using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services;

public interface ILoanService
{
    Task<StarterLoanEligibility> CheckStarterLoanEligibilityAsync(Guid playerWorldId, CancellationToken ct = default);
    Task<LoanApplicationResult> ApplyForStarterLoanAsync(Guid playerWorldId, decimal amount, int termMonths, CancellationToken ct = default);
    Task<LoanApplicationResult> ApplyForLoanAsync(Guid playerWorldId, LoanType loanType, decimal amount, int termMonths, Guid? collateralAircraftId = null, string? purpose = null, CancellationToken ct = default);
    Task<LoanPaymentResult> MakePaymentAsync(Guid loanId, decimal amount, CancellationToken ct = default);
    Task<LoanPaymentResult> PayOffLoanAsync(Guid loanId, CancellationToken ct = default);
    Task<IEnumerable<Loan>> GetPlayerLoansAsync(Guid playerWorldId, CancellationToken ct = default);
    Task<Loan?> GetLoanAsync(Guid loanId, CancellationToken ct = default);
    Task<LoanSummary> GetLoanSummaryAsync(Guid playerWorldId, CancellationToken ct = default);
    Task ProcessOverdueLoansAsync(Guid worldId, CancellationToken ct = default);
}

public class LoanService : ILoanService
{
    private readonly PilotLifeDbContext _context;
    private readonly ICreditScoreService _creditScoreService;
    private readonly ILogger<LoanService> _logger;

    public LoanService(
        PilotLifeDbContext context,
        ICreditScoreService creditScoreService,
        ILogger<LoanService> logger)
    {
        _context = context;
        _creditScoreService = creditScoreService;
        _logger = logger;
    }

    public async Task<StarterLoanEligibility> CheckStarterLoanEligibilityAsync(Guid playerWorldId, CancellationToken ct = default)
    {
        var playerWorld = await _context.PlayerWorlds
            .Include(pw => pw.Loans)
            .Include(pw => pw.World)
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId, ct);

        if (playerWorld == null)
        {
            return new StarterLoanEligibility
            {
                IsEligible = false,
                Reason = "Player world not found"
            };
        }

        // Get bank that offers starter loans
        var bank = await _context.Banks
            .FirstOrDefaultAsync(b => b.WorldId == playerWorld.WorldId && b.OffersStarterLoan && b.IsActive, ct);

        if (bank == null)
        {
            return new StarterLoanEligibility
            {
                IsEligible = false,
                Reason = "No bank in this world offers starter loans"
            };
        }

        // Check if player already has a starter loan in this world
        var hasStarterLoan = playerWorld.Loans.Any(l => l.LoanType == LoanType.StarterLoan);
        if (hasStarterLoan)
        {
            return new StarterLoanEligibility
            {
                IsEligible = false,
                Reason = "You have already used your one-time starter loan in this world"
            };
        }

        // Check for CPL - get the license type first (LicenseTypes are global)
        var cplLicenseType = await _context.LicenseTypes
            .FirstOrDefaultAsync(lt => lt.Code == "CPL", ct);

        var hasCpl = false;
        if (cplLicenseType != null)
        {
            hasCpl = await _context.UserLicenses
                .AnyAsync(ul => ul.PlayerWorldId == playerWorldId &&
                               ul.LicenseTypeId == cplLicenseType.Id &&
                               ul.IsValid &&
                               !ul.IsRevoked, ct);
        }

        if (!hasCpl)
        {
            return new StarterLoanEligibility
            {
                IsEligible = false,
                Reason = "You need a Commercial Pilot License (CPL) to apply for a starter loan",
                RequiresCpl = true
            };
        }

        // Calculate net worth (balance + aircraft value - existing debt)
        var ownedAircraft = await _context.OwnedAircraft
            .Include(oa => oa.Aircraft)
            .Where(oa => oa.PlayerWorldId == playerWorldId)
            .ToListAsync(ct);

        var aircraftValue = ownedAircraft.Sum(oa => oa.EstimatedValue(oa.PurchasePrice));
        var existingDebt = playerWorld.Loans
            .Where(l => l.Status == LoanStatus.Active)
            .Sum(l => l.RemainingPrincipal);

        var netWorth = playerWorld.Balance + aircraftValue - existingDebt;

        // Check net worth limit ($100k for starter loan)
        if (netWorth >= 100000m)
        {
            return new StarterLoanEligibility
            {
                IsEligible = false,
                Reason = $"Your net worth (${netWorth:N0}) exceeds the $100,000 limit for starter loans",
                CurrentNetWorth = netWorth
            };
        }

        return new StarterLoanEligibility
        {
            IsEligible = true,
            MaxAmount = bank.StarterLoanMaxAmount,
            InterestRate = bank.StarterLoanInterestRate,
            AvailableTerms = new[] { 6, 9, 12 },
            CurrentBalance = playerWorld.Balance,
            CurrentNetWorth = netWorth,
            BankId = bank.Id,
            BankName = bank.Name
        };
    }

    public async Task<LoanApplicationResult> ApplyForStarterLoanAsync(
        Guid playerWorldId,
        decimal amount,
        int termMonths,
        CancellationToken ct = default)
    {
        // Check eligibility first
        var eligibility = await CheckStarterLoanEligibilityAsync(playerWorldId, ct);
        if (!eligibility.IsEligible)
        {
            return new LoanApplicationResult
            {
                Success = false,
                Message = eligibility.Reason
            };
        }

        // Validate amount
        if (amount <= 0 || amount > eligibility.MaxAmount)
        {
            return new LoanApplicationResult
            {
                Success = false,
                Message = $"Loan amount must be between $1 and ${eligibility.MaxAmount:N0}"
            };
        }

        // Validate term
        if (!eligibility.AvailableTerms!.Contains(termMonths))
        {
            return new LoanApplicationResult
            {
                Success = false,
                Message = $"Invalid term. Available terms: {string.Join(", ", eligibility.AvailableTerms)} months"
            };
        }

        var playerWorld = await _context.PlayerWorlds
            .FirstAsync(pw => pw.Id == playerWorldId, ct);

        // Create the loan
        var loan = Loan.CreateStarterLoan(
            playerWorld.WorldId,
            playerWorldId,
            eligibility.BankId!.Value,
            amount,
            termMonths,
            eligibility.InterestRate!.Value);

        // Approve and disburse immediately (starter loans are auto-approved)
        loan.Approve();

        // Add loan to database
        _context.Loans.Add(loan);

        // Disburse funds to player
        playerWorld.Balance += amount;

        // Record credit score event
        await _creditScoreService.RecordLoanOpenedAsync(playerWorldId, loan.Id, ct);

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Starter loan {LoanId} for ${Amount} approved and disbursed to player {PlayerWorldId}",
            loan.Id, amount, playerWorldId);

        return new LoanApplicationResult
        {
            Success = true,
            Message = "Starter loan approved! Funds have been deposited to your account.",
            Loan = loan
        };
    }

    public async Task<LoanApplicationResult> ApplyForLoanAsync(
        Guid playerWorldId,
        LoanType loanType,
        decimal amount,
        int termMonths,
        Guid? collateralAircraftId = null,
        string? purpose = null,
        CancellationToken ct = default)
    {
        if (loanType == LoanType.StarterLoan)
        {
            return await ApplyForStarterLoanAsync(playerWorldId, amount, termMonths, ct);
        }

        var playerWorld = await _context.PlayerWorlds
            .Include(pw => pw.Loans)
            .Include(pw => pw.World)
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId, ct);

        if (playerWorld == null)
        {
            return new LoanApplicationResult
            {
                Success = false,
                Message = "Player world not found"
            };
        }

        // Get bank
        var bank = await _context.Banks
            .FirstOrDefaultAsync(b => b.WorldId == playerWorld.WorldId && b.IsActive, ct);

        if (bank == null)
        {
            return new LoanApplicationResult
            {
                Success = false,
                Message = "No active bank in this world"
            };
        }

        // Check credit score
        if (playerWorld.CreditScore < bank.MinCreditScore)
        {
            return new LoanApplicationResult
            {
                Success = false,
                Message = $"Your credit score ({playerWorld.CreditScore}) is below the minimum required ({bank.MinCreditScore})"
            };
        }

        // Check term
        if (termMonths < 1 || termMonths > bank.MaxTermMonths)
        {
            return new LoanApplicationResult
            {
                Success = false,
                Message = $"Loan term must be between 1 and {bank.MaxTermMonths} months"
            };
        }

        // Calculate max loan amount based on net worth
        var ownedAircraft = await _context.OwnedAircraft
            .Include(oa => oa.Aircraft)
            .Where(oa => oa.PlayerWorldId == playerWorldId)
            .ToListAsync(ct);

        var aircraftValue = ownedAircraft.Sum(oa => oa.EstimatedValue(oa.PurchasePrice));
        var existingDebt = playerWorld.Loans
            .Where(l => l.Status == LoanStatus.Active)
            .Sum(l => l.RemainingPrincipal);

        var netWorth = playerWorld.Balance + aircraftValue - existingDebt;
        var maxLoanAmount = netWorth * bank.MaxLoanToNetWorthRatio;

        if (amount > maxLoanAmount)
        {
            return new LoanApplicationResult
            {
                Success = false,
                Message = $"Maximum loan amount based on your net worth is ${maxLoanAmount:N0}"
            };
        }

        // Calculate interest rate based on credit score
        var interestRate = bank.CalculateInterestRate(playerWorld.CreditScore);

        // Apply world multiplier
        interestRate *= playerWorld.World.LoanInterestMultiplier;

        // Create the loan
        var loan = Loan.CreateLoan(
            playerWorld.WorldId,
            playerWorldId,
            bank.Id,
            loanType,
            amount,
            termMonths,
            interestRate,
            purpose,
            collateralAircraftId);

        // Approve and disburse
        loan.Approve();

        _context.Loans.Add(loan);

        // Disburse funds
        playerWorld.Balance += amount;

        // Record credit score event
        await _creditScoreService.RecordLoanOpenedAsync(playerWorldId, loan.Id, ct);

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Loan {LoanId} ({LoanType}) for ${Amount} approved for player {PlayerWorldId}",
            loan.Id, loanType, amount, playerWorldId);

        return new LoanApplicationResult
        {
            Success = true,
            Message = "Loan approved! Funds have been deposited to your account.",
            Loan = loan
        };
    }

    public async Task<LoanPaymentResult> MakePaymentAsync(Guid loanId, decimal amount, CancellationToken ct = default)
    {
        var loan = await _context.Loans
            .Include(l => l.PlayerWorld)
            .Include(l => l.Bank)
            .FirstOrDefaultAsync(l => l.Id == loanId, ct);

        if (loan == null)
        {
            return new LoanPaymentResult
            {
                Success = false,
                Message = "Loan not found"
            };
        }

        if (loan.Status != LoanStatus.Active)
        {
            return new LoanPaymentResult
            {
                Success = false,
                Message = $"Cannot make payment on a loan with status: {loan.Status}"
            };
        }

        if (amount <= 0)
        {
            return new LoanPaymentResult
            {
                Success = false,
                Message = "Payment amount must be greater than zero"
            };
        }

        if (loan.PlayerWorld.Balance < amount)
        {
            return new LoanPaymentResult
            {
                Success = false,
                Message = $"Insufficient funds. Your balance is ${loan.PlayerWorld.Balance:N2}"
            };
        }

        // Check if payment is late
        var isLate = loan.NextPaymentDue.HasValue && DateTimeOffset.UtcNow > loan.NextPaymentDue.Value;

        // Make the payment
        var payment = loan.MakePayment(amount, isLate);
        payment.WorldId = loan.WorldId;

        // Deduct from player balance
        loan.PlayerWorld.Balance -= amount;

        _context.LoanPayments.Add(payment);

        // Record credit score event
        if (isLate)
        {
            await _creditScoreService.RecordLatePaymentAsync(loan.PlayerWorldId, loan.Id, payment.DaysLate, ct);
        }
        else
        {
            await _creditScoreService.RecordOnTimePaymentAsync(loan.PlayerWorldId, loan.Id, ct);
        }

        // Check if loan is paid off
        if (loan.Status == LoanStatus.PaidOff)
        {
            await _creditScoreService.RecordLoanPaidOffAsync(loan.PlayerWorldId, loan.Id, ct);
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Payment of ${Amount} made on loan {LoanId}. Remaining balance: ${Remaining}",
            amount, loanId, loan.RemainingPrincipal);

        return new LoanPaymentResult
        {
            Success = true,
            Message = loan.Status == LoanStatus.PaidOff
                ? "Congratulations! Your loan has been paid off!"
                : $"Payment successful. Remaining balance: ${loan.RemainingPrincipal:N2}",
            Payment = payment,
            LoanPaidOff = loan.Status == LoanStatus.PaidOff
        };
    }

    public async Task<LoanPaymentResult> PayOffLoanAsync(Guid loanId, CancellationToken ct = default)
    {
        var loan = await _context.Loans
            .Include(l => l.PlayerWorld)
            .FirstOrDefaultAsync(l => l.Id == loanId, ct);

        if (loan == null)
        {
            return new LoanPaymentResult
            {
                Success = false,
                Message = "Loan not found"
            };
        }

        var payoffAmount = loan.GetPayoffAmount();

        if (loan.PlayerWorld.Balance < payoffAmount)
        {
            return new LoanPaymentResult
            {
                Success = false,
                Message = $"Insufficient funds. Payoff amount is ${payoffAmount:N2}, your balance is ${loan.PlayerWorld.Balance:N2}"
            };
        }

        return await MakePaymentAsync(loanId, payoffAmount, ct);
    }

    public async Task<IEnumerable<Loan>> GetPlayerLoansAsync(Guid playerWorldId, CancellationToken ct = default)
    {
        return await _context.Loans
            .Include(l => l.Bank)
            .Include(l => l.CollateralAircraft)
                .ThenInclude(a => a!.Aircraft)
            .Where(l => l.PlayerWorldId == playerWorldId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Loan?> GetLoanAsync(Guid loanId, CancellationToken ct = default)
    {
        return await _context.Loans
            .Include(l => l.Bank)
            .Include(l => l.Payments.OrderByDescending(p => p.PaymentNumber))
            .Include(l => l.CollateralAircraft)
                .ThenInclude(a => a!.Aircraft)
            .FirstOrDefaultAsync(l => l.Id == loanId, ct);
    }

    public async Task<LoanSummary> GetLoanSummaryAsync(Guid playerWorldId, CancellationToken ct = default)
    {
        var loans = await _context.Loans
            .Where(l => l.PlayerWorldId == playerWorldId)
            .ToListAsync(ct);

        var activeLoans = loans.Where(l => l.Status == LoanStatus.Active).ToList();

        return new LoanSummary
        {
            TotalLoans = loans.Count,
            ActiveLoans = activeLoans.Count,
            PaidOffLoans = loans.Count(l => l.Status == LoanStatus.PaidOff),
            DefaultedLoans = loans.Count(l => l.Status == LoanStatus.Defaulted),
            TotalDebt = activeLoans.Sum(l => l.RemainingPrincipal),
            TotalMonthlyPayment = activeLoans.Sum(l => l.MonthlyPayment),
            NextPaymentDue = activeLoans.Where(l => l.NextPaymentDue.HasValue)
                                        .OrderBy(l => l.NextPaymentDue)
                                        .FirstOrDefault()?.NextPaymentDue,
            HasStarterLoan = loans.Any(l => l.LoanType == LoanType.StarterLoan)
        };
    }

    public async Task ProcessOverdueLoansAsync(Guid worldId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Get all active loans with overdue payments
        var overdueLoans = await _context.Loans
            .Include(l => l.PlayerWorld)
            .Include(l => l.Bank)
            .Where(l => l.WorldId == worldId &&
                       l.Status == LoanStatus.Active &&
                       l.NextPaymentDue.HasValue &&
                       l.NextPaymentDue.Value < now)
            .ToListAsync(ct);

        foreach (var loan in overdueLoans)
        {
            var daysOverdue = (now - loan.NextPaymentDue!.Value).Days;

            // Check if loan should default
            if (daysOverdue >= loan.Bank.DaysToDefault)
            {
                loan.Default();
                await _creditScoreService.RecordLoanDefaultedAsync(loan.PlayerWorldId, loan.Id, ct);

                _logger.LogWarning(
                    "Loan {LoanId} defaulted after {Days} days overdue",
                    loan.Id, daysOverdue);
            }
            else
            {
                // Record missed payment
                loan.MissedPaymentCount++;
                await _creditScoreService.RecordMissedPaymentAsync(loan.PlayerWorldId, loan.Id, ct);

                _logger.LogInformation(
                    "Loan {LoanId} has missed payment. {Days} days overdue, {MissedCount} missed payments",
                    loan.Id, daysOverdue, loan.MissedPaymentCount);
            }
        }

        if (overdueLoans.Any())
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}

// DTOs for loan operations
public class StarterLoanEligibility
{
    public bool IsEligible { get; set; }
    public string? Reason { get; set; }
    public bool RequiresCpl { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? InterestRate { get; set; }
    public int[]? AvailableTerms { get; set; }
    public decimal? CurrentBalance { get; set; }
    public decimal? CurrentNetWorth { get; set; }
    public Guid? BankId { get; set; }
    public string? BankName { get; set; }
}

public class LoanApplicationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Loan? Loan { get; set; }
}

public class LoanPaymentResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public LoanPayment? Payment { get; set; }
    public bool LoanPaidOff { get; set; }
}

public class LoanSummary
{
    public int TotalLoans { get; set; }
    public int ActiveLoans { get; set; }
    public int PaidOffLoans { get; set; }
    public int DefaultedLoans { get; set; }
    public decimal TotalDebt { get; set; }
    public decimal TotalMonthlyPayment { get; set; }
    public DateTimeOffset? NextPaymentDue { get; set; }
    public bool HasStarterLoan { get; set; }
}
