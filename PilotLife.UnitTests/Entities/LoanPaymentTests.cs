using PilotLife.Domain.Entities;

namespace PilotLife.UnitTests.Entities;

public class LoanPaymentTests
{
    [Fact]
    public void NewLoanPayment_HasValidId()
    {
        var payment = new LoanPayment();

        Assert.NotEqual(Guid.Empty, payment.Id);
    }

    [Fact]
    public void NewLoanPayment_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var payment = new LoanPayment();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(payment.CreatedAt, before, after);
    }

    [Fact]
    public void NewLoanPayment_IsNotLateByDefault()
    {
        var payment = new LoanPayment();

        Assert.False(payment.IsLate);
    }

    [Fact]
    public void DaysLate_ReturnsZero_WhenNotLate()
    {
        var payment = new LoanPayment
        {
            IsLate = false,
            DueDate = DateTimeOffset.UtcNow.AddDays(-5),
            PaidAt = DateTimeOffset.UtcNow
        };

        Assert.Equal(0, payment.DaysLate);
    }

    [Fact]
    public void DaysLate_ReturnsCorrectDays_WhenLate()
    {
        var dueDate = DateTimeOffset.UtcNow.AddDays(-5);
        var paidDate = DateTimeOffset.UtcNow;

        var payment = new LoanPayment
        {
            IsLate = true,
            DueDate = dueDate,
            PaidAt = paidDate
        };

        Assert.Equal(5, payment.DaysLate);
    }

    [Fact]
    public void Payment_TracksBreakdownCorrectly()
    {
        var payment = new LoanPayment
        {
            Amount = 1000m,
            PrincipalPortion = 800m,
            InterestPortion = 200m,
            LateFee = 0m
        };

        Assert.Equal(1000m, payment.Amount);
        Assert.Equal(800m, payment.PrincipalPortion);
        Assert.Equal(200m, payment.InterestPortion);
    }

    [Fact]
    public void Payment_TracksRemainingBalance()
    {
        var payment = new LoanPayment
        {
            RemainingBalanceAfter = 50000m
        };

        Assert.Equal(50000m, payment.RemainingBalanceAfter);
    }

    [Fact]
    public void PaymentNumber_TracksSequence()
    {
        var payment1 = new LoanPayment { PaymentNumber = 1 };
        var payment2 = new LoanPayment { PaymentNumber = 2 };
        var payment3 = new LoanPayment { PaymentNumber = 3 };

        Assert.Equal(1, payment1.PaymentNumber);
        Assert.Equal(2, payment2.PaymentNumber);
        Assert.Equal(3, payment3.PaymentNumber);
    }
}
