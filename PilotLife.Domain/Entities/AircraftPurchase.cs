using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Records an aircraft purchase transaction.
/// </summary>
public class AircraftPurchase : BaseEntity
{
    /// <summary>
    /// The world this purchase occurred in.
    /// </summary>
    public Guid WorldId { get; set; }
    public World World { get; set; } = null!;

    /// <summary>
    /// The player who made the purchase.
    /// </summary>
    public Guid PlayerWorldId { get; set; }
    public PlayerWorld PlayerWorld { get; set; } = null!;

    /// <summary>
    /// The owned aircraft that was created from this purchase.
    /// </summary>
    public Guid OwnedAircraftId { get; set; }
    public OwnedAircraft OwnedAircraft { get; set; } = null!;

    /// <summary>
    /// The dealer the aircraft was purchased from (null if private sale).
    /// </summary>
    public Guid? DealerId { get; set; }
    public AircraftDealer? Dealer { get; set; }

    /// <summary>
    /// The inventory item that was purchased (null if private sale).
    /// </summary>
    public Guid? DealerInventoryId { get; set; }
    public DealerInventory? DealerInventory { get; set; }

    /// <summary>
    /// The seller player (for private sales).
    /// </summary>
    public Guid? SellerPlayerWorldId { get; set; }
    public PlayerWorld? SellerPlayerWorld { get; set; }

    /// <summary>
    /// Final purchase price.
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// Down payment amount (if financed).
    /// </summary>
    public decimal DownPayment { get; set; }

    /// <summary>
    /// Trade-in value received (if applicable).
    /// </summary>
    public decimal TradeInValue { get; set; }

    /// <summary>
    /// The owned aircraft traded in (if applicable).
    /// </summary>
    public Guid? TradeInAircraftId { get; set; }
    public OwnedAircraft? TradeInAircraft { get; set; }

    /// <summary>
    /// Whether this purchase was financed.
    /// </summary>
    public bool IsFinanced { get; set; }

    /// <summary>
    /// Loan ID if financed.
    /// </summary>
    public Guid? LoanId { get; set; }

    /// <summary>
    /// Interest rate if financed.
    /// </summary>
    public decimal? FinancingInterestRate { get; set; }

    /// <summary>
    /// Loan term in months if financed.
    /// </summary>
    public int? FinancingTermMonths { get; set; }

    /// <summary>
    /// Monthly payment if financed.
    /// </summary>
    public decimal? MonthlyPayment { get; set; }

    /// <summary>
    /// ICAO code where the purchase was made.
    /// </summary>
    public string PurchaseLocationIcao { get; set; } = string.Empty;

    /// <summary>
    /// When the purchase was completed.
    /// </summary>
    public DateTimeOffset PurchasedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Aircraft condition at time of purchase.
    /// </summary>
    public int ConditionAtPurchase { get; set; }

    /// <summary>
    /// Aircraft hours at time of purchase.
    /// </summary>
    public int FlightMinutesAtPurchase { get; set; }

    /// <summary>
    /// Whether warranty was included.
    /// </summary>
    public bool IncludedWarranty { get; set; }

    /// <summary>
    /// Warranty duration in months.
    /// </summary>
    public int? WarrantyMonths { get; set; }

    /// <summary>
    /// Total amount paid (purchase price - trade-in value).
    /// </summary>
    public decimal NetAmount => PurchasePrice - TradeInValue;

    /// <summary>
    /// Amount financed (if applicable).
    /// </summary>
    public decimal? AmountFinanced => IsFinanced ? PurchasePrice - DownPayment - TradeInValue : null;

    /// <summary>
    /// Creates a dealer purchase record.
    /// </summary>
    public static AircraftPurchase CreateDealerPurchase(
        Guid worldId,
        Guid playerWorldId,
        Guid ownedAircraftId,
        Guid dealerId,
        Guid dealerInventoryId,
        decimal purchasePrice,
        string locationIcao,
        int condition,
        int flightMinutes,
        bool hasWarranty,
        int? warrantyMonths)
    {
        return new AircraftPurchase
        {
            WorldId = worldId,
            PlayerWorldId = playerWorldId,
            OwnedAircraftId = ownedAircraftId,
            DealerId = dealerId,
            DealerInventoryId = dealerInventoryId,
            PurchasePrice = purchasePrice,
            DownPayment = purchasePrice,
            PurchaseLocationIcao = locationIcao,
            ConditionAtPurchase = condition,
            FlightMinutesAtPurchase = flightMinutes,
            IncludedWarranty = hasWarranty,
            WarrantyMonths = warrantyMonths,
            PurchasedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a financed dealer purchase record.
    /// </summary>
    public static AircraftPurchase CreateFinancedDealerPurchase(
        Guid worldId,
        Guid playerWorldId,
        Guid ownedAircraftId,
        Guid dealerId,
        Guid dealerInventoryId,
        decimal purchasePrice,
        decimal downPayment,
        decimal interestRate,
        int termMonths,
        decimal monthlyPayment,
        Guid loanId,
        string locationIcao,
        int condition,
        int flightMinutes,
        bool hasWarranty,
        int? warrantyMonths)
    {
        return new AircraftPurchase
        {
            WorldId = worldId,
            PlayerWorldId = playerWorldId,
            OwnedAircraftId = ownedAircraftId,
            DealerId = dealerId,
            DealerInventoryId = dealerInventoryId,
            PurchasePrice = purchasePrice,
            DownPayment = downPayment,
            IsFinanced = true,
            LoanId = loanId,
            FinancingInterestRate = interestRate,
            FinancingTermMonths = termMonths,
            MonthlyPayment = monthlyPayment,
            PurchaseLocationIcao = locationIcao,
            ConditionAtPurchase = condition,
            FlightMinutesAtPurchase = flightMinutes,
            IncludedWarranty = hasWarranty,
            WarrantyMonths = warrantyMonths,
            PurchasedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a private sale purchase record.
    /// </summary>
    public static AircraftPurchase CreatePrivatePurchase(
        Guid worldId,
        Guid buyerPlayerWorldId,
        Guid sellerPlayerWorldId,
        Guid ownedAircraftId,
        decimal purchasePrice,
        string locationIcao,
        int condition,
        int flightMinutes)
    {
        return new AircraftPurchase
        {
            WorldId = worldId,
            PlayerWorldId = buyerPlayerWorldId,
            SellerPlayerWorldId = sellerPlayerWorldId,
            OwnedAircraftId = ownedAircraftId,
            PurchasePrice = purchasePrice,
            DownPayment = purchasePrice,
            PurchaseLocationIcao = locationIcao,
            ConditionAtPurchase = condition,
            FlightMinutesAtPurchase = flightMinutes,
            IncludedWarranty = false,
            PurchasedAt = DateTimeOffset.UtcNow
        };
    }
}
