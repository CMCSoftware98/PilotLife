namespace PilotLife.API.DTOs;

// Marketplace-specific response DTOs
public class DealerResponse
{
    public required string Id { get; set; }
    public required string AirportIcao { get; set; }
    public DealerAirportResponse? Airport { get; set; }
    public required string DealerType { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal PriceMultiplier { get; set; }
    public bool OffersFinancing { get; set; }
    public decimal? FinancingDownPaymentPercent { get; set; }
    public decimal? FinancingInterestRate { get; set; }
    public int MinCondition { get; set; }
    public int MaxCondition { get; set; }
    public double ReputationScore { get; set; }
    public bool IsActive { get; set; }
}

public class DealerAirportResponse
{
    public required string Icao { get; set; }
    public required string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class InventoryResponse
{
    public required string Id { get; set; }
    public required string DealerId { get; set; }
    public DealerResponse? Dealer { get; set; }
    public required string AircraftId { get; set; }
    public MarketplaceAircraftResponse? Aircraft { get; set; }
    public string? Registration { get; set; }
    public int Condition { get; set; }
    public int TotalFlightMinutes { get; set; }
    public double TotalFlightHours { get; set; }
    public decimal BasePrice { get; set; }
    public decimal ListPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public bool IsNew { get; set; }
    public bool HasWarranty { get; set; }
    public int? WarrantyMonths { get; set; }
    public string? AvionicsPackage { get; set; }
    public string? Notes { get; set; }
    public required string ListedAt { get; set; }
}

public class MarketplaceAircraftResponse
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public string? AtcType { get; set; }
    public string? AtcModel { get; set; }
    public string? Category { get; set; }
    public int EngineType { get; set; }
    public string? EngineTypeStr { get; set; }
    public int NumberOfEngines { get; set; }
    public double MaxGrossWeightLbs { get; set; }
    public double EmptyWeightLbs { get; set; }
    public double CruiseSpeedKts { get; set; }
    public string? SimulatorVersion { get; set; }
    public bool IsApproved { get; set; }
}

public class MarketplaceSearchResponse
{
    public required List<InventoryResponse> Inventory { get; set; }
    public int TotalCount { get; set; }
    public int SearchedAirports { get; set; }
}

public class PurchaseRequest
{
    public Guid InventoryId { get; set; }
    public bool UseFinancing { get; set; }
    public decimal? DownPayment { get; set; }
    public int? LoanTermMonths { get; set; }
}

public class PurchaseResponse
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public OwnedAircraftResponse? OwnedAircraft { get; set; }
    public decimal NewBalance { get; set; }
}

public class OwnedAircraftResponse
{
    public required string Id { get; set; }
    public required string WorldId { get; set; }
    public required string PlayerWorldId { get; set; }
    public required string AircraftId { get; set; }
    public MarketplaceAircraftResponse? Aircraft { get; set; }
    public required string Registration { get; set; }
    public string? Nickname { get; set; }
    public int Condition { get; set; }
    public int TotalFlightMinutes { get; set; }
    public double TotalFlightHours { get; set; }
    public int TotalCycles { get; set; }
    public double HoursSinceLastInspection { get; set; }
    public required string CurrentLocationIcao { get; set; }
    public bool IsAirworthy { get; set; }
    public bool IsInMaintenance { get; set; }
    public bool IsInUse { get; set; }
    public bool IsListedForSale { get; set; }
    public bool HasWarranty { get; set; }
    public string? WarrantyExpiresAt { get; set; }
    public bool HasInsurance { get; set; }
    public string? InsuranceExpiresAt { get; set; }
    public decimal PurchasePrice { get; set; }
    public required string PurchasedAt { get; set; }
    public decimal EstimatedValue { get; set; }
}

public class UpdateNicknameRequest
{
    public string? Nickname { get; set; }
}

public class ListForSaleRequest
{
    public decimal AskingPrice { get; set; }
}
