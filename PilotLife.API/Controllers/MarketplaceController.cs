using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PilotLife.API.DTOs;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarketplaceController : ControllerBase
{
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<MarketplaceController> _logger;

    public MarketplaceController(PilotLifeDbContext context, ILogger<MarketplaceController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET /api/marketplace/dealers
    [HttpGet("dealers")]
    public async Task<ActionResult<IEnumerable<DealerResponse>>> GetDealers(
        [FromQuery] string? airportIcao,
        [FromQuery] string? dealerType)
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        var query = _context.AircraftDealers
            .Where(d => d.WorldId == playerWorld.WorldId && d.IsActive);

        if (!string.IsNullOrEmpty(airportIcao))
        {
            query = query.Where(d => d.AirportIcao == airportIcao);
        }

        if (!string.IsNullOrEmpty(dealerType) && Enum.TryParse<DealerType>(dealerType, out var type))
        {
            query = query.Where(d => d.DealerType == type);
        }

        var dealers = await query.ToListAsync();

        // Get airport info for dealers
        var airportIcaos = dealers.Select(d => d.AirportIcao).Distinct().ToList();
        var airports = await _context.Airports
            .Where(a => airportIcaos.Contains(a.Ident))
            .ToDictionaryAsync(a => a.Ident);

        var response = dealers.Select(d => MapToResponse(d, airports.GetValueOrDefault(d.AirportIcao)));

        return Ok(response);
    }

    // GET /api/marketplace/dealers/{id}
    [HttpGet("dealers/{id:guid}")]
    public async Task<ActionResult<DealerResponse>> GetDealer(Guid id)
    {
        var dealer = await _context.AircraftDealers.FindAsync(id);

        if (dealer == null)
        {
            return NotFound(new { message = "Dealer not found" });
        }

        var airport = await _context.Airports.FirstOrDefaultAsync(a => a.Ident == dealer.AirportIcao);

        return Ok(MapToResponse(dealer, airport));
    }

    // GET /api/marketplace/inventory
    [HttpGet("inventory")]
    public async Task<ActionResult<IEnumerable<InventoryResponse>>> GetInventory(
        [FromQuery] string? dealerId,
        [FromQuery] string? aircraftId,
        [FromQuery] int? minCondition,
        [FromQuery] decimal? maxPrice)
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        var query = _context.DealerInventory
            .Include(i => i.Dealer)
            .Include(i => i.Aircraft)
            .Where(i => i.WorldId == playerWorld.WorldId && !i.IsSold && i.Dealer.IsActive);

        if (Guid.TryParse(dealerId, out var dId))
        {
            query = query.Where(i => i.DealerId == dId);
        }

        if (Guid.TryParse(aircraftId, out var aId))
        {
            query = query.Where(i => i.AircraftId == aId);
        }

        if (minCondition.HasValue)
        {
            query = query.Where(i => i.Condition >= minCondition.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(i => i.ListPrice <= maxPrice.Value);
        }

        var inventory = await query
            .OrderByDescending(i => i.ListedAt)
            .Take(100)
            .ToListAsync();

        // Get airport info for dealers
        var airportIcaos = inventory.Select(i => i.Dealer.AirportIcao).Distinct().ToList();
        var airports = await _context.Airports
            .Where(a => airportIcaos.Contains(a.Ident))
            .ToDictionaryAsync(a => a.Ident);

        var response = inventory.Select(i => MapToInventoryResponse(i, airports.GetValueOrDefault(i.Dealer.AirportIcao)));

        return Ok(response);
    }

    // GET /api/marketplace/inventory/{id}
    [HttpGet("inventory/{id:guid}")]
    public async Task<ActionResult<InventoryResponse>> GetInventoryItem(Guid id)
    {
        var item = await _context.DealerInventory
            .Include(i => i.Dealer)
            .Include(i => i.Aircraft)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
        {
            return NotFound(new { message = "Inventory item not found" });
        }

        var airport = await _context.Airports.FirstOrDefaultAsync(a => a.Ident == item.Dealer.AirportIcao);

        return Ok(MapToInventoryResponse(item, airport));
    }

    // GET /api/marketplace/inventory/local/{airportIcao}
    [HttpGet("inventory/local/{airportIcao}")]
    public async Task<ActionResult<IEnumerable<InventoryResponse>>> GetLocalInventory(string airportIcao)
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        var airport = await _context.Airports.FirstOrDefaultAsync(a => a.Ident == airportIcao);
        if (airport == null)
        {
            return NotFound(new { message = "Airport not found" });
        }

        var inventory = await _context.DealerInventory
            .Include(i => i.Dealer)
            .Include(i => i.Aircraft)
            .Where(i => i.WorldId == playerWorld.WorldId &&
                        i.Dealer.AirportIcao == airportIcao &&
                        !i.IsSold &&
                        i.Dealer.IsActive)
            .OrderByDescending(i => i.ListedAt)
            .ToListAsync();

        var response = inventory.Select(i => MapToInventoryResponse(i, airport));

        return Ok(response);
    }

    // GET /api/marketplace/search
    [HttpGet("search")]
    public async Task<ActionResult<MarketplaceSearchResponse>> SearchMarketplace(
        [FromQuery] string fromAirportIcao,
        [FromQuery] string? aircraftType,
        [FromQuery] double? maxDistance,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? minCondition,
        [FromQuery] int? limit)
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        var fromAirport = await _context.Airports.FirstOrDefaultAsync(a => a.Ident == fromAirportIcao);
        if (fromAirport == null)
        {
            return BadRequest(new { message = "Origin airport not found" });
        }

        // Default limit to 20, max 100
        var resultLimit = Math.Min(limit ?? 20, 100);

        // Start with base query
        var query = _context.DealerInventory
            .Include(i => i.Dealer)
            .Include(i => i.Aircraft)
            .Where(i => i.WorldId == playerWorld.WorldId && !i.IsSold && i.Dealer.IsActive);

        // Filter by aircraft type (title match)
        if (!string.IsNullOrEmpty(aircraftType))
        {
            var searchTerm = aircraftType.ToLower();
            query = query.Where(i => i.Aircraft.Title.ToLower().Contains(searchTerm) ||
                                      (i.Aircraft.AtcType != null && i.Aircraft.AtcType.ToLower().Contains(searchTerm)) ||
                                      (i.Aircraft.AtcModel != null && i.Aircraft.AtcModel.ToLower().Contains(searchTerm)));
        }

        if (minCondition.HasValue)
        {
            query = query.Where(i => i.Condition >= minCondition.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(i => i.ListPrice <= maxPrice.Value);
        }

        // Get candidate inventory items
        var candidates = await query
            .OrderByDescending(i => i.Condition)
            .ThenBy(i => i.ListPrice)
            .Take(1000) // Pre-filter to reasonable number
            .ToListAsync();

        // Get unique airports from candidates
        var airportIcaos = candidates.Select(i => i.Dealer.AirportIcao).Distinct().ToList();
        var airports = await _context.Airports
            .Where(a => airportIcaos.Contains(a.Ident))
            .ToDictionaryAsync(a => a.Ident);

        // Filter by distance if specified
        var results = new List<(DealerInventory Item, Airport? Airport, double Distance)>();

        foreach (var item in candidates)
        {
            if (!airports.TryGetValue(item.Dealer.AirportIcao, out var airport))
                continue;

            var distance = CalculateDistanceNm(
                fromAirport.Latitude, fromAirport.Longitude,
                airport.Latitude, airport.Longitude);

            if (maxDistance.HasValue && distance > maxDistance.Value)
                continue;

            results.Add((item, airport, distance));
        }

        // Sort by distance and take limit
        var orderedResults = results
            .OrderBy(r => r.Distance)
            .Take(resultLimit)
            .ToList();

        var response = new MarketplaceSearchResponse
        {
            Inventory = orderedResults.Select(r => MapToInventoryResponse(r.Item, r.Airport)).ToList(),
            TotalCount = results.Count,
            SearchedAirports = airports.Count
        };

        return Ok(response);
    }

    // POST /api/marketplace/purchase
    [HttpPost("purchase")]
    public async Task<ActionResult<PurchaseResponse>> PurchaseAircraft([FromBody] PurchaseRequest request)
    {
        var userId = GetUserId();
        var playerWorld = await GetCurrentPlayerWorld(userId);

        if (playerWorld == null)
        {
            return BadRequest(new { message = "No active world selected" });
        }

        var inventory = await _context.DealerInventory
            .Include(i => i.Dealer)
            .Include(i => i.Aircraft)
            .FirstOrDefaultAsync(i => i.Id == request.InventoryId);

        if (inventory == null)
        {
            return NotFound(new { message = "Aircraft not found" });
        }

        if (inventory.IsSold)
        {
            return BadRequest(new { message = "Aircraft has already been sold" });
        }

        if (inventory.WorldId != playerWorld.WorldId)
        {
            return BadRequest(new { message = "Aircraft is not available in your world" });
        }

        // Check financing
        decimal amountDue;
        if (request.UseFinancing && inventory.Dealer.OffersFinancing)
        {
            var downPayment = request.DownPayment ?? (inventory.ListPrice * inventory.Dealer.FinancingDownPaymentPercent ?? 0.20m);
            amountDue = downPayment;
            // TODO: Create loan record for remaining balance
        }
        else
        {
            amountDue = inventory.ListPrice;
        }

        // Check balance
        if (playerWorld.Balance < amountDue)
        {
            return BadRequest(new { message = "Insufficient funds" });
        }

        // Generate registration
        var registration = GenerateRegistration();

        // Create owned aircraft
        var ownedAircraft = new OwnedAircraft
        {
            WorldId = playerWorld.WorldId,
            PlayerWorldId = playerWorld.Id,
            AircraftId = inventory.AircraftId,
            Registration = registration,
            Condition = inventory.Condition,
            TotalFlightMinutes = inventory.TotalFlightMinutes,
            TotalCycles = inventory.TotalCycles,
            CurrentLocationIcao = inventory.Dealer.AirportIcao,
            HasWarranty = inventory.HasWarranty,
            WarrantyExpiresAt = inventory.HasWarranty && inventory.WarrantyMonths.HasValue
                ? DateTimeOffset.UtcNow.AddMonths(inventory.WarrantyMonths.Value)
                : null,
            PurchasePrice = inventory.ListPrice,
            PurchasedAt = DateTimeOffset.UtcNow
        };

        // Update inventory and player balance
        inventory.MarkAsSold();
        playerWorld.Balance -= amountDue;
        inventory.Dealer.TotalSales++;

        _context.OwnedAircraft.Add(ownedAircraft);
        await _context.SaveChangesAsync();

        // Load aircraft for response
        await _context.Entry(ownedAircraft).Reference(o => o.Aircraft).LoadAsync();

        _logger.LogInformation("User {UserId} purchased aircraft {AircraftId} for {Price}",
            userId, inventory.AircraftId, amountDue);

        return Ok(new PurchaseResponse
        {
            Success = true,
            Message = "Aircraft purchased successfully",
            OwnedAircraft = MapToOwnedAircraftResponse(ownedAircraft),
            NewBalance = playerWorld.Balance
        });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private async Task<PlayerWorld?> GetCurrentPlayerWorld(Guid userId)
    {
        // Get the most recently active player world for this user
        return await _context.PlayerWorlds
            .Where(pw => pw.UserId == userId)
            .OrderByDescending(pw => pw.LastActiveAt)
            .FirstOrDefaultAsync();
    }

    private string GenerateRegistration()
    {
        var random = new Random();
        var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var reg = "N" + random.Next(1, 999).ToString();
        reg += letters[random.Next(letters.Length)];
        reg += letters[random.Next(letters.Length)];
        return reg;
    }

    private static double CalculateDistanceNm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusNm = 3440.065;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusNm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    private static DealerResponse MapToResponse(AircraftDealer dealer, Airport? airport)
    {
        return new DealerResponse
        {
            Id = dealer.Id.ToString(),
            AirportIcao = dealer.AirportIcao,
            Airport = airport != null ? new DealerAirportResponse
            {
                Icao = airport.Ident,
                Name = airport.Name,
                Latitude = airport.Latitude,
                Longitude = airport.Longitude
            } : null,
            DealerType = dealer.DealerType.ToString(),
            Name = dealer.Name,
            Description = dealer.Description,
            PriceMultiplier = dealer.PriceMultiplier,
            OffersFinancing = dealer.OffersFinancing,
            FinancingDownPaymentPercent = dealer.FinancingDownPaymentPercent,
            FinancingInterestRate = dealer.FinancingInterestRate,
            MinCondition = dealer.MinCondition,
            MaxCondition = dealer.MaxCondition,
            ReputationScore = dealer.ReputationScore,
            IsActive = dealer.IsActive
        };
    }

    private static InventoryResponse MapToInventoryResponse(DealerInventory inventory, Airport? airport)
    {
        return new InventoryResponse
        {
            Id = inventory.Id.ToString(),
            DealerId = inventory.DealerId.ToString(),
            Dealer = inventory.Dealer != null ? MapToResponse(inventory.Dealer, airport) : null,
            AircraftId = inventory.AircraftId.ToString(),
            Aircraft = inventory.Aircraft != null ? new MarketplaceAircraftResponse
            {
                Id = inventory.Aircraft.Id.ToString(),
                Title = inventory.Aircraft.Title,
                AtcType = inventory.Aircraft.AtcType,
                AtcModel = inventory.Aircraft.AtcModel,
                Category = inventory.Aircraft.Category,
                EngineType = inventory.Aircraft.EngineType,
                EngineTypeStr = inventory.Aircraft.EngineTypeStr,
                NumberOfEngines = inventory.Aircraft.NumberOfEngines,
                MaxGrossWeightLbs = inventory.Aircraft.MaxGrossWeightLbs,
                EmptyWeightLbs = inventory.Aircraft.EmptyWeightLbs,
                CruiseSpeedKts = inventory.Aircraft.CruiseSpeedKts,
                SimulatorVersion = inventory.Aircraft.SimulatorVersion,
                IsApproved = inventory.Aircraft.IsApproved
            } : null,
            Registration = inventory.Registration,
            Condition = inventory.Condition,
            TotalFlightMinutes = inventory.TotalFlightMinutes,
            TotalFlightHours = inventory.TotalFlightHours,
            BasePrice = inventory.BasePrice,
            ListPrice = inventory.ListPrice,
            DiscountPercent = inventory.DiscountPercent,
            IsNew = inventory.IsNew,
            HasWarranty = inventory.HasWarranty,
            WarrantyMonths = inventory.WarrantyMonths,
            AvionicsPackage = inventory.AvionicsPackage,
            Notes = inventory.Notes,
            ListedAt = inventory.ListedAt.ToString("O")
        };
    }

    private static OwnedAircraftResponse MapToOwnedAircraftResponse(OwnedAircraft aircraft)
    {
        return new OwnedAircraftResponse
        {
            Id = aircraft.Id.ToString(),
            WorldId = aircraft.WorldId.ToString(),
            PlayerWorldId = aircraft.PlayerWorldId.ToString(),
            AircraftId = aircraft.AircraftId.ToString(),
            Aircraft = aircraft.Aircraft != null ? new MarketplaceAircraftResponse
            {
                Id = aircraft.Aircraft.Id.ToString(),
                Title = aircraft.Aircraft.Title,
                AtcType = aircraft.Aircraft.AtcType,
                AtcModel = aircraft.Aircraft.AtcModel,
                Category = aircraft.Aircraft.Category,
                EngineType = aircraft.Aircraft.EngineType,
                EngineTypeStr = aircraft.Aircraft.EngineTypeStr,
                NumberOfEngines = aircraft.Aircraft.NumberOfEngines,
                MaxGrossWeightLbs = aircraft.Aircraft.MaxGrossWeightLbs,
                EmptyWeightLbs = aircraft.Aircraft.EmptyWeightLbs,
                CruiseSpeedKts = aircraft.Aircraft.CruiseSpeedKts,
                SimulatorVersion = aircraft.Aircraft.SimulatorVersion,
                IsApproved = aircraft.Aircraft.IsApproved
            } : null,
            Registration = aircraft.Registration,
            Nickname = aircraft.Nickname,
            Condition = aircraft.Condition,
            TotalFlightMinutes = aircraft.TotalFlightMinutes,
            TotalFlightHours = aircraft.TotalFlightMinutes / 60.0,
            TotalCycles = aircraft.TotalCycles,
            HoursSinceLastInspection = aircraft.HoursSinceLastInspection,
            CurrentLocationIcao = aircraft.CurrentLocationIcao,
            IsAirworthy = aircraft.IsAirworthy,
            IsInMaintenance = aircraft.IsInMaintenance,
            IsInUse = aircraft.IsInUse,
            IsListedForSale = aircraft.IsListedForSale,
            HasWarranty = aircraft.HasWarranty,
            WarrantyExpiresAt = aircraft.WarrantyExpiresAt?.ToString("O"),
            HasInsurance = aircraft.HasInsurance,
            InsuranceExpiresAt = aircraft.InsuranceExpiresAt?.ToString("O"),
            PurchasePrice = aircraft.PurchasePrice,
            PurchasedAt = aircraft.PurchasedAt.ToString("O"),
            EstimatedValue = aircraft.EstimatedValue(aircraft.PurchasePrice)
        };
    }
}
