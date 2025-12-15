namespace PilotLife.Domain.Enums;

/// <summary>
/// Status of a loan throughout its lifecycle.
/// </summary>
public enum LoanStatus
{
    /// <summary>
    /// Loan application pending approval.
    /// </summary>
    Pending,

    /// <summary>
    /// Loan has been approved and is active.
    /// </summary>
    Active,

    /// <summary>
    /// Loan has been fully paid off.
    /// </summary>
    PaidOff,

    /// <summary>
    /// Loan is in default due to missed payments.
    /// </summary>
    Defaulted,

    /// <summary>
    /// Loan application was rejected.
    /// </summary>
    Rejected,

    /// <summary>
    /// Loan was cancelled before disbursement.
    /// </summary>
    Cancelled
}
