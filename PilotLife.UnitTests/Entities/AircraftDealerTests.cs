using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class AircraftDealerTests
{
    [Fact]
    public void NewAircraftDealer_HasValidId()
    {
        var dealer = new AircraftDealer();

        Assert.NotEqual(Guid.Empty, dealer.Id);
    }

    [Fact]
    public void NewAircraftDealer_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var dealer = new AircraftDealer();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(dealer.CreatedAt, before, after);
    }

    [Fact]
    public void NewAircraftDealer_HasDefaultValues()
    {
        var dealer = new AircraftDealer();

        Assert.Equal(5, dealer.MinInventory);
        Assert.Equal(20, dealer.MaxInventory);
        Assert.Equal(7, dealer.InventoryRefreshDays);
        Assert.Equal(1.0m, dealer.PriceMultiplier);
        Assert.False(dealer.OffersFinancing);
        Assert.Equal(60, dealer.MinCondition);
        Assert.Equal(100, dealer.MaxCondition);
        Assert.Equal(0, dealer.MinHours);
        Assert.Equal(10000, dealer.MaxHours);
        Assert.Equal(3.0, dealer.ReputationScore);
        Assert.Equal(0, dealer.TotalSales);
        Assert.True(dealer.IsActive);
    }

    [Fact]
    public void CreateManufacturerShowroom_HasCorrectType()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateManufacturerShowroom(worldId, "EGLL", "Cessna");

        Assert.Equal(DealerType.ManufacturerShowroom, dealer.DealerType);
    }

    [Fact]
    public void CreateManufacturerShowroom_HasCorrectConditionRange()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateManufacturerShowroom(worldId, "EGLL", "Cessna");

        Assert.Equal(100, dealer.MinCondition);
        Assert.Equal(100, dealer.MaxCondition);
        Assert.Equal(0, dealer.MinHours);
        Assert.Equal(0, dealer.MaxHours);
    }

    [Fact]
    public void CreateManufacturerShowroom_OffersFinancing()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateManufacturerShowroom(worldId, "EGLL", "Cessna");

        Assert.True(dealer.OffersFinancing);
        Assert.Equal(0.10m, dealer.FinancingDownPaymentPercent);
        Assert.Equal(0.035m, dealer.FinancingInterestRate);
    }

    [Fact]
    public void CreateManufacturerShowroom_HasHighReputation()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateManufacturerShowroom(worldId, "EGLL", "Cessna");

        Assert.Equal(5.0, dealer.ReputationScore);
    }

    [Fact]
    public void CreateManufacturerShowroom_SetsCorrectIds()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateManufacturerShowroom(worldId, "EGLL", "Cessna");

        Assert.Equal(worldId, dealer.WorldId);
        Assert.Equal("EGLL", dealer.AirportIcao);
    }

    [Fact]
    public void CreateCertifiedPreOwned_HasCorrectType()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateCertifiedPreOwned(worldId, "KJFK", "Premium Aircraft");

        Assert.Equal(DealerType.CertifiedPreOwned, dealer.DealerType);
    }

    [Fact]
    public void CreateCertifiedPreOwned_HasCorrectConditionRange()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateCertifiedPreOwned(worldId, "KJFK", "Premium Aircraft");

        Assert.Equal(80, dealer.MinCondition);
        Assert.Equal(95, dealer.MaxCondition);
        Assert.Equal(500, dealer.MinHours);
        Assert.Equal(3000, dealer.MaxHours);
    }

    [Fact]
    public void CreateCertifiedPreOwned_HasReducedPriceMultiplier()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateCertifiedPreOwned(worldId, "KJFK", "Premium Aircraft");

        Assert.Equal(0.80m, dealer.PriceMultiplier);
    }

    [Fact]
    public void CreateBudgetLot_HasCorrectType()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateBudgetLot(worldId, "KORD", "Budget Wings");

        Assert.Equal(DealerType.BudgetLot, dealer.DealerType);
    }

    [Fact]
    public void CreateBudgetLot_HasLowerConditionRange()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateBudgetLot(worldId, "KORD", "Budget Wings");

        Assert.Equal(60, dealer.MinCondition);
        Assert.Equal(80, dealer.MaxCondition);
        Assert.Equal(3000, dealer.MinHours);
        Assert.Equal(15000, dealer.MaxHours);
    }

    [Fact]
    public void CreateBudgetLot_HasLowestPriceMultiplier()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateBudgetLot(worldId, "KORD", "Budget Wings");

        Assert.Equal(0.55m, dealer.PriceMultiplier);
    }

    [Fact]
    public void CreateBudgetLot_HasHigherInterestRate()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateBudgetLot(worldId, "KORD", "Budget Wings");

        Assert.True(dealer.OffersFinancing);
        Assert.Equal(0.08m, dealer.FinancingInterestRate);
        Assert.Equal(0.25m, dealer.FinancingDownPaymentPercent);
    }

    [Fact]
    public void CreateFlightSchool_HasCorrectType()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateFlightSchool(worldId, "KLAX", "Wings Academy");

        Assert.Equal(DealerType.FlightSchool, dealer.DealerType);
    }

    [Fact]
    public void CreateFlightSchool_HasLowestDownPayment()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateFlightSchool(worldId, "KLAX", "Wings Academy");

        Assert.True(dealer.OffersFinancing);
        Assert.Equal(0.05m, dealer.FinancingDownPaymentPercent);
    }

    [Fact]
    public void CreateFlightSchool_HasHighReputation()
    {
        var worldId = Guid.NewGuid();

        var dealer = AircraftDealer.CreateFlightSchool(worldId, "KLAX", "Wings Academy");

        Assert.Equal(4.5, dealer.ReputationScore);
    }

    [Fact]
    public void Inventory_DefaultsToEmpty()
    {
        var dealer = new AircraftDealer();

        Assert.NotNull(dealer.Inventory);
        Assert.Empty(dealer.Inventory);
    }
}
