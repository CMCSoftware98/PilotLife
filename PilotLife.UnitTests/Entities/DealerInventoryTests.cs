using PilotLife.Domain.Entities;

namespace PilotLife.UnitTests.Entities;

public class DealerInventoryTests
{
    [Fact]
    public void NewDealerInventory_HasValidId()
    {
        var inventory = new DealerInventory();

        Assert.NotEqual(Guid.Empty, inventory.Id);
    }

    [Fact]
    public void NewDealerInventory_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var inventory = new DealerInventory();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(inventory.CreatedAt, before, after);
    }

    [Fact]
    public void NewDealerInventory_HasDefaultValues()
    {
        var inventory = new DealerInventory();

        Assert.Equal(100, inventory.Condition);
        Assert.Equal(0, inventory.TotalFlightMinutes);
        Assert.Equal(0, inventory.TotalCycles);
        Assert.False(inventory.IsNew);
        Assert.False(inventory.HasWarranty);
        Assert.False(inventory.IsSold);
    }

    [Fact]
    public void TotalFlightHours_ConvertsMinutesToHours()
    {
        var inventory = new DealerInventory { TotalFlightMinutes = 180 };

        Assert.Equal(3.0, inventory.TotalFlightHours);
    }

    [Fact]
    public void DiscountPercent_CalculatesCorrectly()
    {
        var inventory = new DealerInventory
        {
            BasePrice = 100000m,
            ListPrice = 80000m
        };

        Assert.Equal(20.0m, inventory.DiscountPercent);
    }

    [Fact]
    public void DiscountPercent_WhenNoDiscount_ReturnsZero()
    {
        var inventory = new DealerInventory
        {
            BasePrice = 100000m,
            ListPrice = 100000m
        };

        Assert.Equal(0m, inventory.DiscountPercent);
    }

    [Fact]
    public void DiscountPercent_WhenBasePriceZero_ReturnsZero()
    {
        var inventory = new DealerInventory
        {
            BasePrice = 0m,
            ListPrice = 50000m
        };

        Assert.Equal(0m, inventory.DiscountPercent);
    }

    [Fact]
    public void IsActive_WhenNotSoldAndNotExpired_ReturnsTrue()
    {
        var inventory = new DealerInventory
        {
            IsSold = false,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        Assert.True(inventory.IsActive);
    }

    [Fact]
    public void IsActive_WhenSold_ReturnsFalse()
    {
        var inventory = new DealerInventory
        {
            IsSold = true,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        Assert.False(inventory.IsActive);
    }

    [Fact]
    public void IsActive_WhenExpired_ReturnsFalse()
    {
        var inventory = new DealerInventory
        {
            IsSold = false,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        Assert.False(inventory.IsActive);
    }

    [Fact]
    public void IsActive_WhenNoExpiry_ReturnsTrue()
    {
        var inventory = new DealerInventory
        {
            IsSold = false,
            ExpiresAt = null
        };

        Assert.True(inventory.IsActive);
    }

    [Fact]
    public void MarkAsSold_SetsIsSoldAndSoldAt()
    {
        var before = DateTimeOffset.UtcNow;
        var inventory = new DealerInventory();

        inventory.MarkAsSold();
        var after = DateTimeOffset.UtcNow;

        Assert.True(inventory.IsSold);
        Assert.NotNull(inventory.SoldAt);
        Assert.InRange(inventory.SoldAt.Value, before, after);
    }

    [Fact]
    public void CreateNew_SetsCorrectValues()
    {
        var worldId = Guid.NewGuid();
        var dealerId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var inventory = DealerInventory.CreateNew(
            worldId, dealerId, aircraftId,
            150000m, 150000m, 12);

        Assert.Equal(worldId, inventory.WorldId);
        Assert.Equal(dealerId, inventory.DealerId);
        Assert.Equal(aircraftId, inventory.AircraftId);
        Assert.Equal(100, inventory.Condition);
        Assert.Equal(0, inventory.TotalFlightMinutes);
        Assert.Equal(0, inventory.TotalCycles);
        Assert.Equal(150000m, inventory.BasePrice);
        Assert.Equal(150000m, inventory.ListPrice);
        Assert.True(inventory.IsNew);
        Assert.True(inventory.HasWarranty);
        Assert.Equal(12, inventory.WarrantyMonths);
    }

    [Fact]
    public void CreateUsed_SetsCorrectValues()
    {
        var worldId = Guid.NewGuid();
        var dealerId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var inventory = DealerInventory.CreateUsed(
            worldId, dealerId, aircraftId,
            150000m, 100000m,
            85, 6000, 200,
            true, 6);

        Assert.Equal(worldId, inventory.WorldId);
        Assert.Equal(dealerId, inventory.DealerId);
        Assert.Equal(aircraftId, inventory.AircraftId);
        Assert.Equal(85, inventory.Condition);
        Assert.Equal(6000, inventory.TotalFlightMinutes);
        Assert.Equal(200, inventory.TotalCycles);
        Assert.Equal(150000m, inventory.BasePrice);
        Assert.Equal(100000m, inventory.ListPrice);
        Assert.False(inventory.IsNew);
        Assert.True(inventory.HasWarranty);
        Assert.Equal(6, inventory.WarrantyMonths);
    }

    [Fact]
    public void CreateUsed_WithoutWarranty_HasNoWarranty()
    {
        var worldId = Guid.NewGuid();
        var dealerId = Guid.NewGuid();
        var aircraftId = Guid.NewGuid();

        var inventory = DealerInventory.CreateUsed(
            worldId, dealerId, aircraftId,
            150000m, 80000m,
            70, 12000, 500);

        Assert.False(inventory.HasWarranty);
        Assert.Null(inventory.WarrantyMonths);
    }
}
