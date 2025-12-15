using PilotLife.Domain.Common;
using PilotLife.Domain.Enums;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a loan issued to a player.
/// Tracks the loan lifecycle including payments, interest, and status.
/// </summary>
public class Loan : BaseEntity
{
    /// <summary>
    /// The world this loan exists in.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    /// <summary>
    /// The player who took out the loan.
    /// </summary>
    public Guid PlayerWorldId { get; set; }
    public PlayerWorld PlayerWorld { get; set; } = null!;

    /// <summary>
    /// The bank issuing this loan.
    /// </summary>
    public Guid BankId { get; set; }
    public Bank Bank { get; set; } = null!;

    /// <summary>
    /// Type of loan product.
    /// </summary>
    public LoanType LoanType { get; set; }

    /// <summary>
    /// Current status of the loan.
    /// </summary>
    public LoanStatus Status { get; set; } = LoanStatus.Pending;

    /// <summary>
    /// Original principal amount borrowed.
    /// </summary>
    public decimal PrincipalAmount { get; set; }

    /// <summary>
    /// Monthly interest rate (0.015 = 1.5% per month).
    /// </summary>
    public decimal InterestRatePerMonth { get; set; }

    /// <summary>
    /// Loan term in game months.
    /// </summary>
    public int TermMonths { get; set; }

    /// <summary>
    /// Calculated monthly payment amount.
    /// </summary>
    public decimal MonthlyPayment { get; set; }

    /// <summary>
    /// Total amount to be repaid (principal + interest).
    /// </summary>
    public decimal TotalRepaymentAmount { get; set; }

    /// <summary>
    /// Remaining principal balance.
    /// </summary>
    public decimal RemainingPrincipal { get; set; }

    /// <summary>
    /// Total interest accrued to date.
    /// </summary>
    public decimal AccruedInterest { get; set; }

    /// <summary>
    /// Total amount paid so far.
    /// </summary>
    public decimal TotalPaid { get; set; }

    /// <summary>
    /// Number of payments made.
    /// </summary>
    public int PaymentsMade { get; set; }

    /// <summary>
    /// Number of payments remaining.
    /// </summary>
    public int PaymentsRemaining { get; set; }

    /// <summary>
    /// Number of late payments.
    /// </summary>
    public int LatePaymentCount { get; set; }

    /// <summary>
    /// Number of missed payments before default.
    /// </summary>
    public int MissedPaymentCount { get; set; }

    /// <summary>
    /// When the next payment is due.
    /// </summary>
    public DateTimeOffset? NextPaymentDue { get; set; }

    /// <summary>
    /// When the loan was approved.
    /// </summary>
    public DateTimeOffset? ApprovedAt { get; set; }

    /// <summary>
    /// When funds were disbursed.
    /// </summary>
    public DateTimeOffset? DisbursedAt { get; set; }

    /// <summary>
    /// When the loan was fully paid off.
    /// </summary>
    public DateTimeOffset? PaidOffAt { get; set; }

    /// <summary>
    /// When the loan went into default.
    /// </summary>
    public DateTimeOffset? DefaultedAt { get; set; }

    // Collateral (optional)
    /// <summary>
    /// Aircraft used as collateral (if any).
    /// </summary>
    public Guid? CollateralAircraftId { get; set; }
    public OwnedAircraft? CollateralAircraft { get; set; }

    /// <summary>
    /// Purpose/description of the loan.
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Administrative notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Payment history for this loan.
    /// </summary>
    public ICollection<LoanPayment> Payments { get; set; } = new List<LoanPayment>();

    /// <summary>
    /// Calculates the monthly payment using standard amortization formula.
    /// </summary>
    public static decimal CalculateMonthlyPayment(decimal principal, decimal monthlyRate, int termMonths)
    {
        if (monthlyRate == 0)
            return principal / termMonths;

        // PMT = P * (r * (1 + r)^n) / ((1 + r)^n - 1)
        var ratePlusOne = 1 + monthlyRate;
        var power = (decimal)Math.Pow((double)ratePlusOne, termMonths);
        return principal * (monthlyRate * power) / (power - 1);
    }

    /// <summary>
    /// Calculates the total repayment amount.
    /// </summary>
    public static decimal CalculateTotalRepayment(decimal monthlyPayment, int termMonths)
    {
        return monthlyPayment * termMonths;
    }

    /// <summary>
    /// Creates a starter loan for a new commercial pilot.
    /// </summary>
    public static Loan CreateStarterLoan(
        Guid worldId,
        Guid playerWorldId,
        Guid bankId,
        decimal amount,
        int termMonths,
        decimal interestRatePerMonth = 0.015m)
    {
        var monthlyPayment = CalculateMonthlyPayment(amount, interestRatePerMonth, termMonths);
        var totalRepayment = CalculateTotalRepayment(monthlyPayment, termMonths);

        return new Loan
        {
            WorldId = worldId,
            PlayerWorldId = playerWorldId,
            BankId = bankId,
            LoanType = LoanType.StarterLoan,
            Status = LoanStatus.Pending,
            PrincipalAmount = amount,
            InterestRatePerMonth = interestRatePerMonth,
            TermMonths = termMonths,
            MonthlyPayment = Math.Round(monthlyPayment, 2),
            TotalRepaymentAmount = Math.Round(totalRepayment, 2),
            RemainingPrincipal = amount,
            AccruedInterest = 0,
            TotalPaid = 0,
            PaymentsMade = 0,
            PaymentsRemaining = termMonths,
            Purpose = "Starter Loan - First Aircraft Purchase"
        };
    }

    /// <summary>
    /// Creates a regular loan.
    /// </summary>
    public static Loan CreateLoan(
        Guid worldId,
        Guid playerWorldId,
        Guid bankId,
        LoanType loanType,
        decimal amount,
        int termMonths,
        decimal interestRatePerMonth,
        string? purpose = null,
        Guid? collateralAircraftId = null)
    {
        var monthlyPayment = CalculateMonthlyPayment(amount, interestRatePerMonth, termMonths);
        var totalRepayment = CalculateTotalRepayment(monthlyPayment, termMonths);

        return new Loan
        {
            WorldId = worldId,
            PlayerWorldId = playerWorldId,
            BankId = bankId,
            LoanType = loanType,
            Status = LoanStatus.Pending,
            PrincipalAmount = amount,
            InterestRatePerMonth = interestRatePerMonth,
            TermMonths = termMonths,
            MonthlyPayment = Math.Round(monthlyPayment, 2),
            TotalRepaymentAmount = Math.Round(totalRepayment, 2),
            RemainingPrincipal = amount,
            AccruedInterest = 0,
            TotalPaid = 0,
            PaymentsMade = 0,
            PaymentsRemaining = termMonths,
            Purpose = purpose,
            CollateralAircraftId = collateralAircraftId
        };
    }

    /// <summary>
    /// Approves the loan and sets the first payment date.
    /// </summary>
    public void Approve(DateTimeOffset? firstPaymentDate = null)
    {
        Status = LoanStatus.Active;
        ApprovedAt = DateTimeOffset.UtcNow;
        DisbursedAt = DateTimeOffset.UtcNow;
        // First payment due in 1 game month (approximately 2.5 real days)
        NextPaymentDue = firstPaymentDate ?? DateTimeOffset.UtcNow.AddDays(2.5);
    }

    /// <summary>
    /// Records a payment against the loan.
    /// </summary>
    public LoanPayment MakePayment(decimal amount, bool isLate = false)
    {
        var interestPortion = RemainingPrincipal * InterestRatePerMonth;
        var principalPortion = amount - interestPortion;

        if (principalPortion < 0)
        {
            // Payment doesn't cover interest - all goes to interest
            interestPortion = amount;
            principalPortion = 0;
        }

        var payment = new LoanPayment
        {
            LoanId = Id,
            WorldId = WorldId,
            PaymentNumber = PaymentsMade + 1,
            Amount = amount,
            PrincipalPortion = principalPortion,
            InterestPortion = interestPortion,
            LateFee = 0,
            RemainingBalanceAfter = RemainingPrincipal - principalPortion,
            DueDate = NextPaymentDue ?? DateTimeOffset.UtcNow,
            PaidAt = DateTimeOffset.UtcNow,
            IsLate = isLate
        };

        RemainingPrincipal -= principalPortion;
        AccruedInterest += interestPortion;
        TotalPaid += amount;
        PaymentsMade++;
        PaymentsRemaining--;

        if (isLate)
            LatePaymentCount++;

        // Set next payment date (1 game month = ~2.5 real days)
        NextPaymentDue = DateTimeOffset.UtcNow.AddDays(2.5);

        // Check if paid off
        if (RemainingPrincipal <= 0.01m || PaymentsRemaining <= 0)
        {
            Status = LoanStatus.PaidOff;
            PaidOffAt = DateTimeOffset.UtcNow;
            RemainingPrincipal = 0;
            PaymentsRemaining = 0;
            NextPaymentDue = null;
        }

        return payment;
    }

    /// <summary>
    /// Pays off the entire remaining balance.
    /// </summary>
    public LoanPayment PayOff()
    {
        var payoffAmount = RemainingPrincipal + (RemainingPrincipal * InterestRatePerMonth);
        return MakePayment(payoffAmount);
    }

    /// <summary>
    /// Marks the loan as defaulted.
    /// </summary>
    public void Default()
    {
        Status = LoanStatus.Defaulted;
        DefaultedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the current payoff amount including accrued interest.
    /// </summary>
    public decimal GetPayoffAmount()
    {
        return RemainingPrincipal + (RemainingPrincipal * InterestRatePerMonth);
    }
}
