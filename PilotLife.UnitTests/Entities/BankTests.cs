using PilotLife.Domain.Entities;

namespace PilotLife.UnitTests.Entities;

public class BankTests
{
    [Fact]
    public void NewBank_HasValidId()
    {
        var bank = new Bank();

        Assert.NotEqual(Guid.Empty, bank.Id);
    }

    [Fact]
    public void NewBank_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var bank = new Bank();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(bank.CreatedAt, before, after);
    }

    [Fact]
    public void NewBank_HasDefaultValues()
    {
        var bank = new Bank();

        Assert.Equal(250000m, bank.StarterLoanMaxAmount);
        Assert.Equal(0.015m, bank.StarterLoanInterestRate);
        Assert.Equal(0.02m, bank.BaseInterestRate);
        Assert.Equal(0.08m, bank.MaxInterestRate);
        Assert.Equal(500, bank.MinCreditScore);
        Assert.Equal(3.0m, bank.MaxLoanToNetWorthRatio);
        Assert.Equal(0.05m, bank.MinDownPaymentPercent);
        Assert.Equal(24, bank.MaxTermMonths);
        Assert.Equal(3, bank.DaysToDefault);
        Assert.Equal(0.05m, bank.LatePaymentFeePercent);
        Assert.True(bank.IsActive);
    }

    [Fact]
    public void CreateDefault_SetsWorldId()
    {
        var worldId = Guid.NewGuid();

        var bank = Bank.CreateDefault(worldId);

        Assert.Equal(worldId, bank.WorldId);
    }

    [Fact]
    public void CreateDefault_OffersStarterLoan()
    {
        var worldId = Guid.NewGuid();

        var bank = Bank.CreateDefault(worldId);

        Assert.True(bank.OffersStarterLoan);
    }

    [Fact]
    public void CreateDefault_HasCorrectName()
    {
        var worldId = Guid.NewGuid();

        var bank = Bank.CreateDefault(worldId);

        Assert.Equal("PilotLife Bank", bank.Name);
    }

    [Fact]
    public void CreateDefault_HasDescription()
    {
        var worldId = Guid.NewGuid();

        var bank = Bank.CreateDefault(worldId);

        Assert.NotNull(bank.Description);
        Assert.Contains("Starter Loan", bank.Description);
    }

    [Fact]
    public void CalculateInterestRate_Returns_BaseRate_For_HighCreditScore()
    {
        var bank = Bank.CreateDefault(Guid.NewGuid());

        var rate = bank.CalculateInterestRate(800);

        Assert.Equal(bank.BaseInterestRate, rate);
    }

    [Fact]
    public void CalculateInterestRate_Returns_BaseRate_For_ExcellentCreditScore()
    {
        var bank = Bank.CreateDefault(Guid.NewGuid());

        var rate = bank.CalculateInterestRate(850);

        Assert.Equal(bank.BaseInterestRate, rate);
    }

    [Fact]
    public void CalculateInterestRate_Returns_MaxRate_For_LowCreditScore()
    {
        var bank = Bank.CreateDefault(Guid.NewGuid());

        var rate = bank.CalculateInterestRate(500);

        Assert.Equal(bank.MaxInterestRate, rate);
    }

    [Fact]
    public void CalculateInterestRate_Returns_MaxRate_For_VeryLowCreditScore()
    {
        var bank = Bank.CreateDefault(Guid.NewGuid());

        var rate = bank.CalculateInterestRate(400);

        Assert.Equal(bank.MaxInterestRate, rate);
    }

    [Fact]
    public void CalculateInterestRate_Returns_IntermediateRate_For_MidCreditScore()
    {
        var bank = Bank.CreateDefault(Guid.NewGuid());

        var rate = bank.CalculateInterestRate(650);

        // Should be between base and max rate
        Assert.True(rate > bank.BaseInterestRate);
        Assert.True(rate < bank.MaxInterestRate);
    }

    [Fact]
    public void CalculateInterestRate_ReturnsHigherRate_ForLowerScore()
    {
        var bank = Bank.CreateDefault(Guid.NewGuid());

        var rate600 = bank.CalculateInterestRate(600);
        var rate700 = bank.CalculateInterestRate(700);

        Assert.True(rate600 > rate700);
    }

    [Fact]
    public void Loans_DefaultsToEmpty()
    {
        var bank = new Bank();

        Assert.NotNull(bank.Loans);
        Assert.Empty(bank.Loans);
    }
}
