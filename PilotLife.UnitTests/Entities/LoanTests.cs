using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class LoanTests
{
    [Fact]
    public void NewLoan_HasValidId()
    {
        var loan = new Loan();

        Assert.NotEqual(Guid.Empty, loan.Id);
    }

    [Fact]
    public void NewLoan_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var loan = new Loan();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(loan.CreatedAt, before, after);
    }

    [Fact]
    public void NewLoan_HasDefaultStatus()
    {
        var loan = new Loan();

        Assert.Equal(LoanStatus.Pending, loan.Status);
    }

    [Fact]
    public void CalculateMonthlyPayment_WithZeroInterest_ReturnsPrincipalDividedByTerm()
    {
        var principal = 12000m;
        var term = 12;

        var payment = Loan.CalculateMonthlyPayment(principal, 0, term);

        Assert.Equal(1000m, payment);
    }

    [Fact]
    public void CalculateMonthlyPayment_WithInterest_ReturnsCorrectPayment()
    {
        var principal = 100000m;
        var monthlyRate = 0.015m; // 1.5% per month
        var term = 12;

        var payment = Loan.CalculateMonthlyPayment(principal, monthlyRate, term);

        // Expected payment calculated using standard amortization formula
        // Should be approximately $9,168
        Assert.True(payment > 9100m && payment < 9200m);
    }

    [Fact]
    public void CalculateTotalRepayment_ReturnsPaymentTimesTerm()
    {
        var monthlyPayment = 1000m;
        var term = 12;

        var total = Loan.CalculateTotalRepayment(monthlyPayment, term);

        Assert.Equal(12000m, total);
    }

    [Fact]
    public void CreateStarterLoan_SetsCorrectLoanType()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);

        Assert.Equal(LoanType.StarterLoan, loan.LoanType);
    }

    [Fact]
    public void CreateStarterLoan_SetsPendingStatus()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);

        Assert.Equal(LoanStatus.Pending, loan.Status);
    }

    [Fact]
    public void CreateStarterLoan_SetsCorrectPrincipal()
    {
        var amount = 172500m;

        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            amount,
            12);

        Assert.Equal(amount, loan.PrincipalAmount);
        Assert.Equal(amount, loan.RemainingPrincipal);
    }

    [Fact]
    public void CreateStarterLoan_SetsDefaultInterestRate()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);

        Assert.Equal(0.015m, loan.InterestRatePerMonth);
    }

    [Fact]
    public void CreateStarterLoan_CalculatesPayments()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);

        Assert.True(loan.MonthlyPayment > 0);
        Assert.True(loan.TotalRepaymentAmount > loan.PrincipalAmount);
        Assert.Equal(12, loan.PaymentsRemaining);
        Assert.Equal(0, loan.PaymentsMade);
    }

    [Fact]
    public void CreateStarterLoan_SetsPurpose()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);

        Assert.Equal("Starter Loan - First Aircraft Purchase", loan.Purpose);
    }

    [Fact]
    public void CreateLoan_WithCustomInterestRate_SetsCorrectRate()
    {
        var customRate = 0.03m;

        var loan = Loan.CreateLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            LoanType.AircraftFinancing,
            100000m,
            12,
            customRate);

        Assert.Equal(customRate, loan.InterestRatePerMonth);
    }

    [Fact]
    public void CreateLoan_WithCollateral_SetsCollateralId()
    {
        var collateralId = Guid.NewGuid();

        var loan = Loan.CreateLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            LoanType.AircraftFinancing,
            100000m,
            12,
            0.02m,
            "Aircraft purchase",
            collateralId);

        Assert.Equal(collateralId, loan.CollateralAircraftId);
    }

    [Fact]
    public void Approve_SetsActiveStatus()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);

        loan.Approve();

        Assert.Equal(LoanStatus.Active, loan.Status);
    }

    [Fact]
    public void Approve_SetsApprovedAt()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);

        var before = DateTimeOffset.UtcNow;
        loan.Approve();
        var after = DateTimeOffset.UtcNow;

        Assert.NotNull(loan.ApprovedAt);
        Assert.InRange(loan.ApprovedAt.Value, before, after);
    }

    [Fact]
    public void Approve_SetsDisbursedAt()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);

        loan.Approve();

        Assert.NotNull(loan.DisbursedAt);
    }

    [Fact]
    public void Approve_SetsNextPaymentDue()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);

        loan.Approve();

        Assert.NotNull(loan.NextPaymentDue);
        Assert.True(loan.NextPaymentDue > DateTimeOffset.UtcNow);
    }

    [Fact]
    public void MakePayment_ReducesRemainingPrincipal()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);
        loan.Approve();

        var principalBefore = loan.RemainingPrincipal;

        loan.MakePayment(loan.MonthlyPayment);

        Assert.True(loan.RemainingPrincipal < principalBefore);
    }

    [Fact]
    public void MakePayment_IncrementsTotalPaid()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);
        loan.Approve();

        loan.MakePayment(loan.MonthlyPayment);

        Assert.Equal(loan.MonthlyPayment, loan.TotalPaid);
    }

    [Fact]
    public void MakePayment_IncrementsPaymentsMade()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);
        loan.Approve();

        loan.MakePayment(loan.MonthlyPayment);

        Assert.Equal(1, loan.PaymentsMade);
    }

    [Fact]
    public void MakePayment_DecrementsPaymentsRemaining()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);
        loan.Approve();

        loan.MakePayment(loan.MonthlyPayment);

        Assert.Equal(11, loan.PaymentsRemaining);
    }

    [Fact]
    public void MakePayment_WhenLate_IncrementsLatePaymentCount()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);
        loan.Approve();

        loan.MakePayment(loan.MonthlyPayment, isLate: true);

        Assert.Equal(1, loan.LatePaymentCount);
    }

    [Fact]
    public void MakePayment_ReturnsLoanPayment()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);
        loan.Approve();

        var payment = loan.MakePayment(loan.MonthlyPayment);

        Assert.NotNull(payment);
        Assert.Equal(loan.MonthlyPayment, payment.Amount);
        Assert.Equal(1, payment.PaymentNumber);
        Assert.True(payment.PrincipalPortion > 0);
        Assert.True(payment.InterestPortion > 0);
    }

    [Fact]
    public void MakePayment_WhenFullyPaid_SetsPaidOffStatus()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1000m, // Small amount for quick payoff
            2);
        loan.Approve();

        // Make all payments
        for (int i = 0; i < 2; i++)
        {
            loan.MakePayment(loan.MonthlyPayment);
        }

        Assert.Equal(LoanStatus.PaidOff, loan.Status);
        Assert.NotNull(loan.PaidOffAt);
    }

    [Fact]
    public void PayOff_PaysEntireRemainingBalance()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            10000m,
            12);
        loan.Approve();

        // Make a few payments first
        loan.MakePayment(loan.MonthlyPayment);
        loan.MakePayment(loan.MonthlyPayment);

        var payment = loan.PayOff();

        Assert.Equal(LoanStatus.PaidOff, loan.Status);
        Assert.Equal(0, loan.RemainingPrincipal);
    }

    [Fact]
    public void Default_SetsDefaultedStatus()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);
        loan.Approve();

        loan.Default();

        Assert.Equal(LoanStatus.Defaulted, loan.Status);
    }

    [Fact]
    public void Default_SetsDefaultedAt()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);
        loan.Approve();

        var before = DateTimeOffset.UtcNow;
        loan.Default();
        var after = DateTimeOffset.UtcNow;

        Assert.NotNull(loan.DefaultedAt);
        Assert.InRange(loan.DefaultedAt.Value, before, after);
    }

    [Fact]
    public void GetPayoffAmount_IncludesAccruedInterest()
    {
        var loan = Loan.CreateStarterLoan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            12);
        loan.Approve();

        var payoffAmount = loan.GetPayoffAmount();

        // Payoff should be principal plus one month's interest
        var expectedMinimum = loan.RemainingPrincipal;
        Assert.True(payoffAmount > expectedMinimum);
    }

    [Fact]
    public void Payments_DefaultsToEmpty()
    {
        var loan = new Loan();

        Assert.NotNull(loan.Payments);
        Assert.Empty(loan.Payments);
    }
}
