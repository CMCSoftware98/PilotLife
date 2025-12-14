using PilotLife.Domain.Entities;

namespace PilotLife.UnitTests.Entities;

public class FlightFinancialsTests
{
    [Fact]
    public void TotalRevenue_SumsAllRevenueComponents()
    {
        var financials = new FlightFinancials
        {
            JobRevenue = 1000m,
            OnTimeBonus = 100m,
            LandingBonus = 50m,
            FuelEfficiencyBonus = 25m,
            OtherBonuses = 10m
        };

        Assert.Equal(1185m, financials.TotalRevenue);
    }

    [Fact]
    public void TotalCosts_SumsAllCostComponents()
    {
        var financials = new FlightFinancials
        {
            FuelCost = 200m,
            LandingFees = 50m,
            HandlingFees = 30m,
            NavigationFees = 20m,
            MaintenanceCost = 100m,
            InsuranceCost = 25m,
            CrewCost = 75m
        };

        Assert.Equal(500m, financials.TotalCosts);
    }

    [Fact]
    public void TotalPenalties_SumsAllPenaltyComponents()
    {
        var financials = new FlightFinancials
        {
            LatePenalty = 50m,
            DamagePenalty = 100m,
            IncidentPenalty = 25m
        };

        Assert.Equal(175m, financials.TotalPenalties);
    }

    [Fact]
    public void NetProfit_CalculatesCorrectly_WhenProfitable()
    {
        var financials = new FlightFinancials
        {
            JobRevenue = 1000m,
            OnTimeBonus = 100m,
            FuelCost = 200m,
            LandingFees = 50m,
            LatePenalty = 0m
        };

        // Revenue: 1100, Costs: 250, Penalties: 0
        Assert.Equal(850m, financials.NetProfit);
        Assert.True(financials.IsProfitable);
    }

    [Fact]
    public void NetProfit_CalculatesCorrectly_WhenUnprofitable()
    {
        var financials = new FlightFinancials
        {
            JobRevenue = 500m,
            FuelCost = 400m,
            LandingFees = 100m,
            LatePenalty = 100m
        };

        // Revenue: 500, Costs: 500, Penalties: 100
        Assert.Equal(-100m, financials.NetProfit);
        Assert.False(financials.IsProfitable);
    }

    [Fact]
    public void NetProfit_IsZero_WhenBreakEven()
    {
        var financials = new FlightFinancials
        {
            JobRevenue = 500m,
            FuelCost = 500m
        };

        Assert.Equal(0m, financials.NetProfit);
        Assert.False(financials.IsProfitable);
    }

    [Fact]
    public void NewFlightFinancials_HasValidId()
    {
        var financials = new FlightFinancials();

        Assert.NotEqual(Guid.Empty, financials.Id);
    }

    [Fact]
    public void NewFlightFinancials_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var financials = new FlightFinancials();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(financials.CreatedAt, before, after);
    }

    [Fact]
    public void AllDecimalProperties_DefaultToZero()
    {
        var financials = new FlightFinancials();

        Assert.Equal(0m, financials.JobRevenue);
        Assert.Equal(0m, financials.OnTimeBonus);
        Assert.Equal(0m, financials.LandingBonus);
        Assert.Equal(0m, financials.FuelEfficiencyBonus);
        Assert.Equal(0m, financials.OtherBonuses);
        Assert.Equal(0m, financials.FuelCost);
        Assert.Equal(0m, financials.LandingFees);
        Assert.Equal(0m, financials.HandlingFees);
        Assert.Equal(0m, financials.NavigationFees);
        Assert.Equal(0m, financials.MaintenanceCost);
        Assert.Equal(0m, financials.InsuranceCost);
        Assert.Equal(0m, financials.CrewCost);
        Assert.Equal(0m, financials.LatePenalty);
        Assert.Equal(0m, financials.DamagePenalty);
        Assert.Equal(0m, financials.IncidentPenalty);
    }
}
