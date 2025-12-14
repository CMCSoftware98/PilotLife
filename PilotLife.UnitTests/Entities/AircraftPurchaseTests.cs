using PilotLife.Domain.Entities;

namespace PilotLife.UnitTests.Entities;

public class AircraftPurchaseTests
{
    [Fact]
    public void NewAircraftPurchase_HasValidId()
    {
        var purchase = new AircraftPurchase();

        Assert.NotEqual(Guid.Empty, purchase.Id);
    }

    [Fact]
    public void NewAircraftPurchase_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var purchase = new AircraftPurchase();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(purchase.CreatedAt, before, after);
    }

    [Fact]
    public void NewAircraftPurchase_HasDefaultValues()
    {
        var purchase = new AircraftPurchase();

        Assert.False(purchase.IsFinanced);
        Assert.False(purchase.IncludedWarranty);
    }

    [Fact]
    public void NetAmount_CalculatesCorrectly()
    {
        var purchase = new AircraftPurchase
        {
            PurchasePrice = 100000m,
            TradeInValue = 25000m
        };

        Assert.Equal(75000m, purchase.NetAmount);
    }

    [Fact]
    public void NetAmount_WhenNoTradeIn_ReturnsPurchasePrice()
    {
        var purchase = new AircraftPurchase
        {
            PurchasePrice = 100000m,
            TradeInValue = 0m
        };

        Assert.Equal(100000m, purchase.NetAmount);
    }

    [Fact]
    public void AmountFinanced_WhenFinanced_CalculatesCorrectly()
    {
        var purchase = new AircraftPurchase
        {
            IsFinanced = true,
            PurchasePrice = 100000m,
            DownPayment = 20000m,
            TradeInValue = 10000m
        };

        Assert.Equal(70000m, purchase.AmountFinanced);
    }

    [Fact]
    public void AmountFinanced_WhenNotFinanced_ReturnsNull()
    {
        var purchase = new AircraftPurchase
        {
            IsFinanced = false,
            PurchasePrice = 100000m,
            DownPayment = 100000m
        };

        Assert.Null(purchase.AmountFinanced);
    }

    [Fact]
    public void CreateDealerPurchase_SetsCorrectValues()
    {
        var worldId = Guid.NewGuid();
        var playerWorldId = Guid.NewGuid();
        var ownedAircraftId = Guid.NewGuid();
        var dealerId = Guid.NewGuid();
        var inventoryId = Guid.NewGuid();

        var purchase = AircraftPurchase.CreateDealerPurchase(
            worldId, playerWorldId, ownedAircraftId,
            dealerId, inventoryId,
            150000m, "EGLL", 100, 0,
            true, 12);

        Assert.Equal(worldId, purchase.WorldId);
        Assert.Equal(playerWorldId, purchase.PlayerWorldId);
        Assert.Equal(ownedAircraftId, purchase.OwnedAircraftId);
        Assert.Equal(dealerId, purchase.DealerId);
        Assert.Equal(inventoryId, purchase.DealerInventoryId);
        Assert.Equal(150000m, purchase.PurchasePrice);
        Assert.Equal(150000m, purchase.DownPayment);
        Assert.Equal("EGLL", purchase.PurchaseLocationIcao);
        Assert.Equal(100, purchase.ConditionAtPurchase);
        Assert.Equal(0, purchase.FlightMinutesAtPurchase);
        Assert.True(purchase.IncludedWarranty);
        Assert.Equal(12, purchase.WarrantyMonths);
        Assert.False(purchase.IsFinanced);
    }

    [Fact]
    public void CreateFinancedDealerPurchase_SetsCorrectValues()
    {
        var worldId = Guid.NewGuid();
        var playerWorldId = Guid.NewGuid();
        var ownedAircraftId = Guid.NewGuid();
        var dealerId = Guid.NewGuid();
        var inventoryId = Guid.NewGuid();
        var loanId = Guid.NewGuid();

        var purchase = AircraftPurchase.CreateFinancedDealerPurchase(
            worldId, playerWorldId, ownedAircraftId,
            dealerId, inventoryId,
            150000m, 30000m, 0.045m, 60, 2500m, loanId,
            "KJFK", 95, 3000,
            true, 6);

        Assert.Equal(worldId, purchase.WorldId);
        Assert.Equal(playerWorldId, purchase.PlayerWorldId);
        Assert.Equal(ownedAircraftId, purchase.OwnedAircraftId);
        Assert.Equal(dealerId, purchase.DealerId);
        Assert.Equal(inventoryId, purchase.DealerInventoryId);
        Assert.Equal(150000m, purchase.PurchasePrice);
        Assert.Equal(30000m, purchase.DownPayment);
        Assert.True(purchase.IsFinanced);
        Assert.Equal(loanId, purchase.LoanId);
        Assert.Equal(0.045m, purchase.FinancingInterestRate);
        Assert.Equal(60, purchase.FinancingTermMonths);
        Assert.Equal(2500m, purchase.MonthlyPayment);
        Assert.Equal("KJFK", purchase.PurchaseLocationIcao);
        Assert.Equal(95, purchase.ConditionAtPurchase);
        Assert.Equal(3000, purchase.FlightMinutesAtPurchase);
        Assert.True(purchase.IncludedWarranty);
        Assert.Equal(6, purchase.WarrantyMonths);
    }

    [Fact]
    public void CreateFinancedDealerPurchase_CalculatesAmountFinanced()
    {
        var worldId = Guid.NewGuid();
        var playerWorldId = Guid.NewGuid();
        var ownedAircraftId = Guid.NewGuid();
        var dealerId = Guid.NewGuid();
        var inventoryId = Guid.NewGuid();
        var loanId = Guid.NewGuid();

        var purchase = AircraftPurchase.CreateFinancedDealerPurchase(
            worldId, playerWorldId, ownedAircraftId,
            dealerId, inventoryId,
            100000m, 20000m, 0.05m, 48, 1800m, loanId,
            "KLAX", 90, 5000,
            false, null);

        Assert.Equal(80000m, purchase.AmountFinanced);
    }

    [Fact]
    public void CreatePrivatePurchase_SetsCorrectValues()
    {
        var worldId = Guid.NewGuid();
        var buyerPlayerWorldId = Guid.NewGuid();
        var sellerPlayerWorldId = Guid.NewGuid();
        var ownedAircraftId = Guid.NewGuid();

        var purchase = AircraftPurchase.CreatePrivatePurchase(
            worldId, buyerPlayerWorldId, sellerPlayerWorldId,
            ownedAircraftId,
            85000m, "KORD", 80, 8000);

        Assert.Equal(worldId, purchase.WorldId);
        Assert.Equal(buyerPlayerWorldId, purchase.PlayerWorldId);
        Assert.Equal(sellerPlayerWorldId, purchase.SellerPlayerWorldId);
        Assert.Equal(ownedAircraftId, purchase.OwnedAircraftId);
        Assert.Equal(85000m, purchase.PurchasePrice);
        Assert.Equal(85000m, purchase.DownPayment);
        Assert.Equal("KORD", purchase.PurchaseLocationIcao);
        Assert.Equal(80, purchase.ConditionAtPurchase);
        Assert.Equal(8000, purchase.FlightMinutesAtPurchase);
        Assert.False(purchase.IncludedWarranty);
        Assert.Null(purchase.DealerId);
        Assert.Null(purchase.DealerInventoryId);
    }

    [Fact]
    public void CreatePrivatePurchase_IsNotFinanced()
    {
        var worldId = Guid.NewGuid();
        var buyerPlayerWorldId = Guid.NewGuid();
        var sellerPlayerWorldId = Guid.NewGuid();
        var ownedAircraftId = Guid.NewGuid();

        var purchase = AircraftPurchase.CreatePrivatePurchase(
            worldId, buyerPlayerWorldId, sellerPlayerWorldId,
            ownedAircraftId,
            50000m, "KSFO", 75, 10000);

        Assert.False(purchase.IsFinanced);
        Assert.Null(purchase.AmountFinanced);
    }
}
