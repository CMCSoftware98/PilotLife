namespace PilotLife.Domain.Enums;

/// <summary>
/// Type of loan product.
/// </summary>
public enum LoanType
{
    /// <summary>
    /// Special starter loan for new CPL holders - once per world, favorable terms.
    /// </summary>
    StarterLoan,

    /// <summary>
    /// Standard aircraft financing loan.
    /// </summary>
    AircraftFinancing,

    /// <summary>
    /// Personal loan not tied to specific collateral.
    /// </summary>
    Personal,

    /// <summary>
    /// Business expansion loan.
    /// </summary>
    Business,

    /// <summary>
    /// Emergency loan with higher rates but quick approval.
    /// </summary>
    Emergency
}
