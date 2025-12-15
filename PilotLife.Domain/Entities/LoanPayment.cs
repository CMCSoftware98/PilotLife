using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Represents a payment made against a loan.
/// Tracks principal, interest, and late fees for each payment.
/// </summary>
public class LoanPayment : BaseEntity
{
    /// <summary>
    /// The loan this payment is for.
    /// </summary>
    public Guid LoanId { get; set; }
    public Loan Loan { get; set; } = null!;

    /// <summary>
    /// The world this payment exists in.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    /// <summary>
    /// Sequential payment number (1, 2, 3, etc.).
    /// </summary>
    public int PaymentNumber { get; set; }

    /// <summary>
    /// Total amount of this payment.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Portion of payment applied to principal.
    /// </summary>
    public decimal PrincipalPortion { get; set; }

    /// <summary>
    /// Portion of payment applied to interest.
    /// </summary>
    public decimal InterestPortion { get; set; }

    /// <summary>
    /// Late fee charged (if any).
    /// </summary>
    public decimal LateFee { get; set; }

    /// <summary>
    /// Remaining loan balance after this payment.
    /// </summary>
    public decimal RemainingBalanceAfter { get; set; }

    /// <summary>
    /// When this payment was due.
    /// </summary>
    public DateTimeOffset DueDate { get; set; }

    /// <summary>
    /// When this payment was actually made.
    /// </summary>
    public DateTimeOffset PaidAt { get; set; }

    /// <summary>
    /// Whether this payment was late.
    /// </summary>
    public bool IsLate { get; set; }

    /// <summary>
    /// Days late (if applicable).
    /// </summary>
    public int DaysLate => IsLate ? (int)(PaidAt - DueDate).TotalDays : 0;

    /// <summary>
    /// Notes about this payment.
    /// </summary>
    public string? Notes { get; set; }
}
