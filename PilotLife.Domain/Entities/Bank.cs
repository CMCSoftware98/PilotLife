using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a bank that offers loans in a world.
/// Different banks have different terms and requirements.
/// </summary>
public class Bank : BaseEntity
{
    /// <summary>
    /// The world this bank operates in.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    /// <summary>
    /// Name of the bank.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the bank and its services.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this bank offers the starter loan program.
    /// </summary>
    public bool OffersStarterLoan { get; set; }

    /// <summary>
    /// Maximum starter loan amount offered.
    /// </summary>
    public decimal StarterLoanMaxAmount { get; set; } = 250000m;

    /// <summary>
    /// Monthly interest rate for starter loans (0.015 = 1.5%).
    /// </summary>
    public decimal StarterLoanInterestRate { get; set; } = 0.015m;

    /// <summary>
    /// Base monthly interest rate for regular loans (0.02 = 2%).
    /// </summary>
    public decimal BaseInterestRate { get; set; } = 0.02m;

    /// <summary>
    /// Maximum monthly interest rate (0.08 = 8%).
    /// </summary>
    public decimal MaxInterestRate { get; set; } = 0.08m;

    /// <summary>
    /// Minimum credit score required for regular loans.
    /// </summary>
    public int MinCreditScore { get; set; } = 500;

    /// <summary>
    /// Maximum loan amount multiplier based on net worth.
    /// </summary>
    public decimal MaxLoanToNetWorthRatio { get; set; } = 3.0m;

    /// <summary>
    /// Minimum down payment percentage required (0.05 = 5%).
    /// </summary>
    public decimal MinDownPaymentPercent { get; set; } = 0.05m;

    /// <summary>
    /// Maximum loan term in game months.
    /// </summary>
    public int MaxTermMonths { get; set; } = 24;

    /// <summary>
    /// Days after missed payment before default.
    /// </summary>
    public int DaysToDefault { get; set; } = 3;

    /// <summary>
    /// Late payment fee percentage.
    /// </summary>
    public decimal LatePaymentFeePercent { get; set; } = 0.05m;

    /// <summary>
    /// Whether this bank is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Loans issued by this bank.
    /// </summary>
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    /// <summary>
    /// Creates the default "PilotLife Bank" for a world.
    /// </summary>
    public static Bank CreateDefault(Guid worldId) => new()
    {
        WorldId = worldId,
        Name = "PilotLife Bank",
        Description = "The official bank of PilotLife. Offers competitive rates and the Starter Loan program for new commercial pilots.",
        OffersStarterLoan = true,
        StarterLoanMaxAmount = 250000m,
        StarterLoanInterestRate = 0.015m,
        BaseInterestRate = 0.02m,
        MaxInterestRate = 0.08m,
        MinCreditScore = 500,
        MaxLoanToNetWorthRatio = 3.0m,
        MinDownPaymentPercent = 0.05m,
        MaxTermMonths = 24,
        DaysToDefault = 3,
        LatePaymentFeePercent = 0.05m,
        IsActive = true
    };

    /// <summary>
    /// Calculates the interest rate for a loan based on credit score.
    /// </summary>
    public decimal CalculateInterestRate(int creditScore)
    {
        // Higher credit score = lower interest rate
        // 800+ = base rate
        // 500 = max rate
        // Linear interpolation between

        if (creditScore >= 800)
            return BaseInterestRate;

        if (creditScore <= MinCreditScore)
            return MaxInterestRate;

        var scoreRange = 800 - MinCreditScore;
        var rateRange = MaxInterestRate - BaseInterestRate;
        var scoreAboveMin = creditScore - MinCreditScore;

        return MaxInterestRate - (rateRange * scoreAboveMin / scoreRange);
    }
}
