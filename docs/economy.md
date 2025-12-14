# PilotLife Complete Economy System - Implementation Plan

## Overview

A comprehensive flight simulation economy with realistic business mechanics, compressed game time, multiple difficulty worlds, and engaging progression systems.

### Core Systems
- **Multi-World Architecture** - Easy, Medium, Hard worlds with separate economies
- **Flight Connector Integration** - Real-time flight tracking, job completion, exam monitoring
- **License Exam System** - Practical flight tests tracked by connector
- **IAM (Identity & Access Management)** - Role-based access control
- Aircraft ownership, repair, and modifications
- Dynamic job system with expiry times and risk levels
- License system with shop and renewals
- Aircraft marketplace with dealers at airports
- Player-to-player auctions
- Banking with loans and credit scores
- AI crew for passive income
- Worker management
- Risk/consequences for illegal activities
- **Future: SimBrief Integration** - Flight plan import and validation

---

## Multi-World Architecture

### Concept
Players can join multiple worlds, each with isolated progress. A player's User account is global, but all game state (money, aircraft, licenses, jobs, etc.) is per-world.

### World Types
| World | Difficulty | Description |
|-------|------------|-------------|
| Easy | Casual | Higher payouts, cheaper prices, forgiving penalties |
| Medium | Standard | Balanced baseline experience |
| Hard | Realistic | Lower payouts, expensive, harsh consequences |
| Custom | Variable | Server-defined modifiers (future) |

### World Difficulty Modifiers

| Modifier | Easy | Medium | Hard |
|----------|------|--------|------|
| Starting Capital | $100,000 | $50,000 | $25,000 |
| Job Payout | ×1.5 | ×1.0 | ×0.7 |
| Aircraft Prices | ×0.7 | ×1.0 | ×1.3 |
| Maintenance Costs | ×0.5 | ×1.0 | ×1.5 |
| License Costs | ×0.5 | ×1.0 | ×1.5 |
| Loan Interest | ×0.5 | ×1.0 | ×1.5 |
| Detection Risk | ×0.5 | ×1.0 | ×1.5 |
| Fines | ×0.5 | ×1.0 | ×2.0 |
| Job Expiry Time | ×2.0 | ×1.0 | ×0.5 |
| Credit Recovery | ×2.0 | ×1.0 | ×0.5 |
| Worker Salaries | ×0.7 | ×1.0 | ×1.3 |

### Entity Classification

**Global Entities (Shared across all worlds):**
- User - Player account (authentication, profile)
- World - World definitions and modifiers
- WorldSettings - Configurable world parameters
- Airport - Shared geography
- AircraftTemplate - Master aircraft specifications
- CargoCategory, CargoSubcategory, CargoType - Cargo definitions
- LicenseType - License definitions (base costs, requirements)
- Bank - Bank definitions (base rates, requirements)
- Role, RolePermission - IAM role definitions
- UserRole - User role assignments (can be global or per-world)

**Per-World Entities (Isolated per world, all have WorldId):**
- PlayerWorld - Player's state in specific world
- OwnedAircraft, AircraftComponent, AircraftModification, MaintenanceLog
- **AircraftRentalListing** - Player aircraft available for rent
- **RentalTransaction** - Rental history and payouts
- Job, FlightJob, **QuickJob** - Including rental jobs
- UserLicense
- LicenseExam, ExamManeuver, ExamCheckpoint - Practical flight exams
- Loan, LoanPayment, CreditScoreEvent
- AircraftDealer, DealerInventory, DealerDiscount
- Auction, AuctionBid
- Worker
- InspectionEvent, ViolationRecord
- TrackedFlight, FlightFinancials

### Database Architecture

**Current Design: Single Database with WorldId**
```
All per-world entities include:
- WorldId (required FK to World)
- Filtered in all queries by WorldId
- Enables easy future sharding
```

**Future: Separate Databases per World**
```
Master Database:
├── User (accounts)
├── World (definitions)
├── Global Templates/Definitions
└── Airport data

World Database (per world):
├── PlayerWorld
├── All per-world entities
└── No WorldId needed (implicit)

Connection routing by WorldId
```

### Key New Entities

**World Entity:**
```csharp
public class World
{
    public Guid Id { get; set; }
    public string Name { get; set; }              // "Easy", "Medium", "Hard"
    public string Slug { get; set; }              // "easy", "medium", "hard"
    public string Description { get; set; }
    public WorldDifficulty Difficulty { get; set; }

    // Economy Modifiers
    public decimal StartingCapital { get; set; }
    public decimal JobPayoutMultiplier { get; set; }
    public decimal AircraftPriceMultiplier { get; set; }
    public decimal MaintenanceCostMultiplier { get; set; }
    public decimal LicenseCostMultiplier { get; set; }
    public decimal LoanInterestMultiplier { get; set; }
    public decimal DetectionRiskMultiplier { get; set; }
    public decimal FineMultiplier { get; set; }
    public decimal JobExpiryMultiplier { get; set; }
    public decimal CreditRecoveryMultiplier { get; set; }
    public decimal WorkerSalaryMultiplier { get; set; }

    // Status
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int MaxPlayers { get; set; }           // 0 = unlimited
    public DateTime CreatedAt { get; set; }
}

public enum WorldDifficulty { Easy, Medium, Hard, Custom }
```

**PlayerWorld Entity (Player's state in a world):**
```csharp
public class PlayerWorld
{
    public Guid Id { get; set; }

    // Links
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid WorldId { get; set; }
    public World World { get; set; }

    // Financial State
    public decimal Balance { get; set; }
    public int CreditScore { get; set; }

    // Experience
    public int TotalFlightMinutes { get; set; }
    public int TotalFlights { get; set; }
    public int TotalJobsCompleted { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalSpent { get; set; }

    // Reputation
    public decimal ReputationScore { get; set; }  // 0.0-5.0
    public int OnTimeDeliveries { get; set; }
    public int LateDeliveries { get; set; }
    public int FailedDeliveries { get; set; }

    // Violations
    public int ViolationPoints { get; set; }
    public DateTime? LastViolationAt { get; set; }

    // Status
    public bool IsActive { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime LastActiveAt { get; set; }

    // Navigation (all per-world)
    public ICollection<OwnedAircraft> OwnedAircraft { get; set; }
    public ICollection<UserLicense> Licenses { get; set; }
    public ICollection<Loan> Loans { get; set; }
    public ICollection<Worker> Workers { get; set; }
    public ICollection<TrackedFlight> Flights { get; set; }
}
```

### Query Pattern

All per-world queries must filter by WorldId:
```csharp
// Get player's aircraft in a specific world
var aircraft = await _context.OwnedAircraft
    .Where(a => a.WorldId == worldId && a.OwnerId == userId)
    .ToListAsync();

// Get available jobs in a world
var jobs = await _context.Jobs
    .Where(j => j.WorldId == worldId && j.Status == JobStatus.Available)
    .ToListAsync();
```

### API Structure

```
/api/worlds                     - List available worlds
/api/worlds/{worldId}           - Get world details
/api/worlds/{worldId}/join      - Join a world (creates PlayerWorld)

/api/worlds/{worldId}/my-profile    - Player's state in world
/api/worlds/{worldId}/aircraft      - Player's aircraft in world
/api/worlds/{worldId}/jobs          - Available jobs in world
/api/worlds/{worldId}/licenses      - Player's licenses in world
/api/worlds/{worldId}/loans         - Player's loans in world
/api/worlds/{worldId}/auctions      - Auctions in world
...
```

---

## Flight Connector & Job Tracking

### Overview
The PilotLife.Connector (C++ SimConnect client) tracks real flights and communicates with the API to complete jobs and monitor license exams.

### Flight Tracking Flow
```
1. Player accepts job(s) in web app
2. Player starts flight sim, connector authenticates
3. Connector receives active jobs/exams from API
4. Player flies - connector tracks in real-time:
   - Current position (lat/lon/alt)
   - Departure airport detection
   - Arrival airport detection
   - Flight time, distance
   - Aircraft state (gear, flaps, engines)
5. On landing at destination:
   - Connector validates job completion criteria
   - Sends flight data to API
   - API processes payout & updates job status
```

### Job Completion Validation
```csharp
public class JobCompletionRequest
{
    public Guid JobId { get; set; }
    public Guid FlightId { get; set; }

    // Departure validation
    public string DepartureIcao { get; set; }
    public DateTime DepartureTime { get; set; }

    // Arrival validation
    public string ArrivalIcao { get; set; }
    public DateTime ArrivalTime { get; set; }

    // Flight data
    public int FlightTimeMinutes { get; set; }
    public double DistanceNm { get; set; }
    public string AircraftUsed { get; set; }  // ICAO type code

    // Cargo condition (for fragile/perishable)
    public decimal CargoConditionPercent { get; set; }  // 0-100%
    public int HardLandingCount { get; set; }
    public int OverspeedCount { get; set; }
}
```

### Connector State Machine
```
States:
- Idle: Not flying, waiting
- PreFlight: In cockpit, engines off
- Taxiing: Moving on ground
- Departing: Takeoff roll / initial climb
- EnRoute: Cruise flight
- Arriving: Approach / landing
- Arrived: Landed, engines running
- Shutdown: Parked, engines off

Job Transitions:
- Job.InTransit when: State changes to Departing from correct airport
- Job.Delivered when: State becomes Shutdown at destination
```

### Multi-Job Flight Support
```
Player can accept multiple jobs to same destination:
- Connector tracks array of active jobs
- On departure: Mark all matching jobs as InTransit
- On arrival: Complete all jobs with same destination
- Partial completion: If player lands elsewhere, jobs remain InTransit
```

### Future: SimBrief Integration
```
Integration Points:
1. Import flight plan from SimBrief API
   - Route (waypoints, airways)
   - Fuel calculations
   - Weights (ZFW, payload)
   - Alternate airports

2. Auto-match jobs to flight plan
   - Find jobs matching origin/destination
   - Suggest optimal cargo based on payload capacity

3. Validate flight against plan
   - Route deviation detection
   - Fuel usage vs planned
   - Time comparison

4. OFP (Operational Flight Plan) parsing
   - SimBrief XML/JSON API
   - Store flight plans in database

Entity: FlightPlan
- SimBriefId
- Origin, Destination, Alternate
- Route (waypoints JSON)
- PlannedFuelKg, PlannedTimeMinutes
- CreatedAt, UsedAt
```

---

## Connector Weight & Fuel Management

### Overview
The connector can read AND write aircraft weight and fuel states via SimConnect SDK. This enables automatic fuel loading and cargo placement when players accept jobs.

### SimConnect Variables Used

**Fuel Tanks (Read/Write):**
```cpp
// Fuel quantities in gallons
FUEL_TANK_LEFT_MAIN_QUANTITY
FUEL_TANK_RIGHT_MAIN_QUANTITY
FUEL_TANK_CENTER_QUANTITY
FUEL_TANK_LEFT_AUX_QUANTITY
FUEL_TANK_RIGHT_AUX_QUANTITY
FUEL_TANK_EXTERNAL1_QUANTITY
FUEL_TANK_EXTERNAL2_QUANTITY

// Total fuel
FUEL_TOTAL_QUANTITY_WEIGHT  // lbs
```

**Payload Stations (Read/Write):**
```cpp
// Payload weights in lbs
PAYLOAD_STATION_WEIGHT:1   // Pilot
PAYLOAD_STATION_WEIGHT:2   // Co-pilot/Passenger
PAYLOAD_STATION_WEIGHT:3   // Rear passengers
PAYLOAD_STATION_WEIGHT:4   // Baggage/Cargo
// ... varies by aircraft
```

**Aircraft Limits (Read):**
```cpp
DESIGN_SPEED_VS0          // Stall speed
MAX_GROSS_WEIGHT          // MTOW
EMPTY_WEIGHT              // OEW
TOTAL_WEIGHT              // Current weight
CG_PERCENT                // Center of gravity
```

### Auto-Fill Feature

When a player accepts a job and clicks "Auto-Fill", the system:

```
┌─────────────────────────────────────────────────────────────────┐
│                    AUTO-FILL CALCULATION                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. FUEL CALCULATION                                            │
│     ├── Route distance: 250nm                                   │
│     ├── Fuel burn rate: 12 gal/hr (from aircraft template)      │
│     ├── Estimated flight time: 1.5 hrs                          │
│     ├── Trip fuel: 18 gal                                       │
│     ├── Alternate fuel (+45 min): 9 gal                         │
│     ├── Reserve (+30 min): 6 gal                                │
│     ├── Contingency (+10%): 3.3 gal                             │
│     └── TOTAL FUEL REQUIRED: 36.3 gal                           │
│                                                                 │
│  2. PAYLOAD CALCULATION                                         │
│     ├── Cargo weight: 450 lbs                                   │
│     ├── Pilot weight: 180 lbs (configurable)                    │
│     └── Distribute to payload stations                          │
│                                                                 │
│  3. WEIGHT CHECK                                                │
│     ├── Empty weight: 1,670 lbs                                 │
│     ├── Fuel weight: 218 lbs (36.3 gal × 6 lbs)                 │
│     ├── Payload: 630 lbs                                        │
│     ├── Total: 2,518 lbs                                        │
│     ├── MTOW: 2,550 lbs                                         │
│     └── ✓ WITHIN LIMITS                                         │
│                                                                 │
│  4. FUEL COST                                                   │
│     ├── Current fuel: 10 gal                                    │
│     ├── Fuel to add: 26.3 gal                                   │
│     ├── Price at EGLL: $7.50/gal (AVGAS)                        │
│     └── FUEL COST: $197.25                                      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Auto-Fill Flow

```
1. Player in Web App:
   ├── Accepts job (cargo: 450 lbs, EGLL → EGCC)
   ├── Clicks "Auto-Fill Aircraft"
   └── Confirms fuel purchase ($197.25)

2. API Processing:
   ├── Calculate fuel requirements
   ├── Check aircraft limits (MTOW, fuel capacity)
   ├── Deduct fuel cost from player balance
   ├── Record fuel purchase at airport
   └── Send payload config to connector

3. Connector (SimConnect):
   ├── Receive auto-fill command
   ├── Set fuel tank quantities
   ├── Set payload station weights
   └── Confirm completion to API

4. Player sees:
   └── Aircraft loaded, ready to fly
```

### Connector Auto-Fill Implementation

```cpp
struct AutoFillCommand {
    // Fuel (gallons per tank)
    float leftMainFuel;
    float rightMainFuel;
    float centerFuel;
    float leftAuxFuel;
    float rightAuxFuel;

    // Payload (lbs per station)
    std::vector<float> payloadStations;

    // Validation
    float expectedTotalWeight;
    float mtow;
};

class FuelPayloadManager {
public:
    bool executeAutoFill(const AutoFillCommand& cmd) {
        // Validate within limits
        if (cmd.expectedTotalWeight > cmd.mtow) {
            return false; // Overweight
        }

        // Set fuel tanks via SimConnect
        SimConnect_SetDataOnSimObject(
            hSimConnect,
            FUEL_TANK_LEFT_MAIN,
            SIMCONNECT_OBJECT_ID_USER,
            0, 0, sizeof(float), &cmd.leftMainFuel
        );
        // ... repeat for other tanks

        // Set payload stations
        for (int i = 0; i < cmd.payloadStations.size(); i++) {
            SimConnect_SetDataOnSimObject(
                hSimConnect,
                PAYLOAD_STATION_BASE + i,
                SIMCONNECT_OBJECT_ID_USER,
                0, 0, sizeof(float), &cmd.payloadStations[i]
            );
        }

        return true;
    }

    AircraftWeights getCurrentWeights() {
        // Read current state from sim
        AircraftWeights weights;
        // ... SimConnect read operations
        return weights;
    }
};
```

### Manual vs Auto-Fill

| Mode | Description | Use Case |
|------|-------------|----------|
| Auto-Fill | System calculates and loads | Quick start, optimal fuel |
| Manual | Player sets own fuel/cargo | Custom planning, tankering |
| Partial | Auto cargo, manual fuel | Experienced players |

### Weight Tracking During Flight

```cpp
// Connector monitors weight throughout flight
struct FlightWeightLog {
    float takeoffWeight;
    float landingWeight;
    float fuelUsed;
    float cargoDelivered;

    // For fuel efficiency stats
    float fuelEfficiencyNmPerGal;
};
```

---

## Airport Fuel System

### Overview
Airports store and sell fuel. Different airports have different fuel types, prices, and daily supply limits. Players must plan fuel stops strategically.

### Fuel Types

| Type | Aircraft | Density | Color |
|------|----------|---------|-------|
| AVGAS 100LL | Piston engines | 6.0 lbs/gal | Blue |
| Jet-A1 | Turboprops, Jets | 6.7 lbs/gal | Clear/Straw |

### Daily Fuel Supply by Airport Size

| Airport Size | Examples | AVGAS/day | Jet-A/day | Refill Rate |
|--------------|----------|-----------|-----------|-------------|
| Grass Strip | Farm strips, private | 500 gal | 0 | 100 gal/hr |
| Small Paved | Local airports | 2,000 gal | 1,500 gal | 250 gal/hr |
| Medium | Regional airports | 15,000 gal | 40,000 gal | 2,000 gal/hr |
| Large Hub | Major airports | Unlimited | Unlimited | N/A |
| Player-Owned | Varies | Set by owner | Set by owner | Based on upgrades |

### Fuel Pricing (Base Rates)

| Location Type | AVGAS/gal | Jet-A/gal |
|---------------|-----------|-----------|
| Major Hub | $6.00 | $4.50 |
| Regional Airport | $7.00 | $5.50 |
| Small Airport | $8.00 | $6.50 |
| Remote/Island | $10.00 | $8.50 |
| Player-Owned | $5.00-$15.00 | $4.00-$12.00 |

**Price Modifiers:**
- High demand (>80% daily usage): +20%
- Low supply (<20% remaining): +30%
- World difficulty: × modifier
- Remote location bonus: +25-50%
- Player-owned markup: Set by owner

### Fuel Purchase Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                 FUEL PURCHASE - EGLL                            │
├─────────────────────────────────────────────────────────────────┤
│  Aircraft: Cessna 172 (N12345)                                  │
│  Current Fuel: 12.5 gal / 56 gal capacity                       │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  AVGAS 100LL                                            │    │
│  │  Price: $6.50/gal                                       │    │
│  │  Available: 1,847 gal (of 2,000 daily)                  │    │
│  │                                                         │    │
│  │  [Fill to ____] gal    or    [Top Off (43.5 gal)]       │    │
│  │                                                         │    │
│  │  Cost: $282.75                                          │    │
│  │                                                         │    │
│  │  [ ] Auto-fill to aircraft via connector                │    │
│  │                                                         │    │
│  │  [Purchase Fuel]                                        │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  💡 Tip: EGGP has cheaper fuel ($6.20/gal) - 150nm away         │
└─────────────────────────────────────────────────────────────────┘
```

### Fuel Exhaustion Scenarios

When an airport runs low on fuel:

```
Low Supply (<500 gal remaining):
├── Warning shown to players
├── Price increases +30%
├── Max purchase limited to 50 gal per aircraft
└── Refill countdown displayed

Out of Stock (0 gal):
├── "NO FUEL AVAILABLE" displayed
├── Players must plan alternate fuel stops
├── Creates strategic gameplay
└── Refills over time based on airport rate
```

### Airport Fuel Entity

```csharp
public class AirportFuelStock
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }
    public string AirportIcao { get; set; }

    // Current stock
    public decimal AvgasQuantityGal { get; set; }
    public decimal JetAQuantityGal { get; set; }

    // Daily limits
    public decimal AvgasDailyCapacityGal { get; set; }
    public decimal JetADailyCapacityGal { get; set; }

    // Refill rates (gal per real hour)
    public decimal AvgasRefillRatePerHour { get; set; }
    public decimal JetARefillRatePerHour { get; set; }

    // Pricing
    public decimal AvgasPricePerGal { get; set; }
    public decimal JetAPricePerGal { get; set; }

    // Ownership (null = system-owned)
    public Guid? OwnerPlayerWorldId { get; set; }
    public decimal? OwnerFuelMarkup { get; set; }  // 0-100% markup

    public DateTime LastRefillAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

public class FuelPurchase
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }
    public Guid PlayerWorldId { get; set; }
    public string AirportIcao { get; set; }

    public FuelType FuelType { get; set; }
    public decimal QuantityGal { get; set; }
    public decimal PricePerGal { get; set; }
    public decimal TotalCost { get; set; }

    public Guid? AircraftId { get; set; }  // If loaded directly
    public bool AutoFilled { get; set; }

    // Revenue split (if player-owned airport)
    public Guid? AirportOwnerId { get; set; }
    public decimal? OwnerRevenue { get; set; }

    public DateTime PurchasedAt { get; set; }
}

public enum FuelType { Avgas100LL, JetA1 }
```

### Strategic Fuel Planning

Players should consider:
- **Tankering**: Buy cheap fuel, carry extra to avoid expensive stops
- **Fuel Stops**: Plan routes through affordable fuel airports
- **Reserve Airports**: Know which airports have fuel for emergencies
- **Player-Owned Savings**: Frequent your own airport for cheaper fuel

---

## Player-Owned Airports

### Overview
Players can purchase airports to earn passive income from landing fees, fuel sales, and services. Airport ownership creates a player-driven economy layer.

### Buyable Airport Types

| Type | Examples | Price Range | Landing Traffic |
|------|----------|-------------|-----------------|
| Grass Strip | Farm fields, private | $50,000 - $250,000 | Low |
| Small Paved | Local GA airports | $250,000 - $1,500,000 | Low-Medium |
| Medium Regional | Regional hubs | $2,000,000 - $15,000,000 | Medium-High |
| Small Island | Remote destinations | $500,000 - $3,000,000 | Low (premium) |

**NOT Buyable:** Major international hubs (system-owned to ensure availability)

### Airport Purchase Flow

```
┌─────────────────────────────────────────────────────────────────┐
│              AIRPORT FOR SALE: EGBJ (Gloucestershire)           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Type: Small Paved Regional                                     │
│  Runway: 09/27 - 4,500ft asphalt                                │
│  Location: Gloucestershire, England                             │
│                                                                 │
│  📊 TRAFFIC STATS (Last 30 game days):                          │
│  ├── Total landings: 847                                        │
│  ├── Average per day: 28                                        │
│  ├── Aircraft mix: 65% light, 25% twin, 10% turboprop           │
│  └── Fuel sold: 12,400 gal AVGAS, 3,200 gal Jet-A               │
│                                                                 │
│  💰 ESTIMATED MONTHLY REVENUE:                                  │
│  ├── Landing fees: $18,500                                      │
│  ├── Fuel profit: $8,200                                        │
│  ├── Operating costs: -$5,000                                   │
│  └── NET PROFIT: ~$21,700/month                                 │
│                                                                 │
│  🏷️ ASKING PRICE: $850,000                                      │
│                                                                 │
│  [ Purchase Outright ]  [ Finance (20% down) ]                  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Revenue Streams

**1. Landing Fees**
| Aircraft Class | Default Fee | Owner Can Set |
|----------------|-------------|---------------|
| Light Single (<6,000 lbs) | $50 | $25 - $300 |
| Light Twin (<12,000 lbs) | $100 | $50 - $500 |
| Turboprop (<20,000 lbs) | $300 | $150 - $1,500 |
| Regional Jet (<50,000 lbs) | $800 | $400 - $3,000 |
| Narrow Body (<180,000 lbs) | $2,000 | $1,000 - $8,000 |
| Wide Body (>180,000 lbs) | $5,000 | $2,500 - $15,000 |

**2. Fuel Sales**
- Owner sets markup: 0-100% above base cost
- Base cost is wholesale price (owner pays this)
- Profit = (Sell price - Base cost) × gallons sold

**3. Parking Fees (Optional)**
| Duration | Default | Owner Can Set |
|----------|---------|---------------|
| Per hour | $10 | $5 - $50 |
| Per day | $50 | $25 - $200 |
| Per week | $200 | $100 - $800 |

### Operating Costs (Per Game Month / ~2.5 Real Days)

| Airport Type | Monthly Maintenance | Staff (Optional) | Fuel Storage |
|--------------|--------------------|--------------------|--------------|
| Grass Strip | $500 | $1,000 | $200 |
| Small Paved | $2,000 | $3,000 | $500 |
| Medium Regional | $8,000 | $12,000 | $2,000 |
| Small Island | $3,000 | $2,000 | $1,000 |

*Costs deducted every game month (~2.5 real days)*

### Airport Upgrades

Owners can invest to improve their airport:

| Upgrade | Cost | Benefit |
|---------|------|---------|
| Larger Fuel Storage | $25,000-$100,000 | +50-200% daily fuel capacity |
| Faster Fuel Trucks | $50,000 | +100% refill rate |
| Better Runway | $100,000-$500,000 | Attract larger aircraft |
| Lighting (Night Ops) | $75,000 | Allow night landings |
| ILS/Approach Aids | $200,000 | Attract IFR traffic |
| Hangar Space | $50,000-$200,000 | Parking revenue, storage |
| FBO Services | $150,000 | Premium fees, pilot amenities |

### Competitive Dynamics

**Pricing Strategy:**
- Set fees too high → pilots avoid your airport
- Set fees too low → less profit per landing
- Balance with local competition

**Fuel Competition:**
```
EGBJ (Player A): AVGAS $7.00/gal
EGBK (Player B): AVGAS $6.50/gal (25nm away)
EGBB (System): AVGAS $6.80/gal (40nm away)

→ Pilots factor fuel price + distance into route planning
```

### Airport Entity

```csharp
public class OwnedAirport
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }
    public string AirportIcao { get; set; }

    // Ownership
    public Guid OwnerPlayerWorldId { get; set; }
    public PlayerWorld Owner { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime PurchasedAt { get; set; }

    // Fee settings
    public decimal LandingFeeLightSingle { get; set; }
    public decimal LandingFeeLightTwin { get; set; }
    public decimal LandingFeeTurboprop { get; set; }
    public decimal LandingFeeRegionalJet { get; set; }
    public decimal LandingFeeNarrowBody { get; set; }
    public decimal LandingFeeWideBody { get; set; }

    // Fuel settings
    public decimal AvgasMarkupPercent { get; set; }  // 0-100
    public decimal JetAMarkupPercent { get; set; }

    // Parking
    public decimal ParkingFeePerHour { get; set; }
    public decimal ParkingFeePerDay { get; set; }

    // Upgrades
    public bool HasNightLighting { get; set; }
    public bool HasILS { get; set; }
    public bool HasFBO { get; set; }
    public int FuelStorageLevel { get; set; }  // 1-3
    public int HangarCapacity { get; set; }

    // Stats
    public int TotalLandings { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime LastLandingAt { get; set; }
}

public class LandingFeeTransaction
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }
    public Guid PilotPlayerWorldId { get; set; }
    public Guid? AirportOwnerId { get; set; }
    public string AirportIcao { get; set; }

    public string AircraftType { get; set; }
    public decimal AircraftWeightLbs { get; set; }
    public decimal FeeAmount { get; set; }

    public decimal OwnerRevenue { get; set; }  // 90% typically
    public decimal PlatformFee { get; set; }   // 10% to system

    public DateTime LandedAt { get; set; }
}
```

### Landing Fee Integration with Connector

```cpp
// Connector reports landing to API
struct LandingReport {
    string arrivalIcao;
    string aircraftType;
    float landingWeightLbs;
    DateTime landingTime;

    // API returns
    decimal landingFee;
    string airportOwner;  // null if system-owned
};

// Fee automatically deducted from pilot's balance
// Owner credited (minus platform fee)
```

### Player Considerations When Flying

```
┌─────────────────────────────────────────────────────────────────┐
│                  ROUTE PLANNING: EGLL → EGCC                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Direct Route: 160nm                                            │
│                                                                 │
│  DESTINATION OPTIONS:                                           │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │ EGCC (Manchester)          - System Owned                │    │
│  │   Landing Fee: $150 (turboprop)                         │    │
│  │   Fuel: $5.80/gal Jet-A                                 │    │
│  ├─────────────────────────────────────────────────────────┤    │
│  │ EGNM (Leeds Bradford)      - Player: SkyKing_Pete       │    │
│  │   Landing Fee: $200 (turboprop) ⚠️ Higher               │    │
│  │   Fuel: $5.20/gal Jet-A ✓ Cheaper                       │    │
│  │   Distance: +15nm                                        │    │
│  ├─────────────────────────────────────────────────────────┤    │
│  │ EGNH (Blackpool)           - Player: AviatorJane        │    │
│  │   Landing Fee: $120 (turboprop) ✓ Cheapest              │    │
│  │   Fuel: $6.00/gal Jet-A                                 │    │
│  │   Distance: +25nm                                        │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  💡 Consider: Landing at EGNH saves $30 in fees but uses       │
│     more fuel (+$15). Net savings: ~$15                         │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Starter Loan Program

### Overview
New players can access a special starter loan after obtaining their CPL to purchase their first aircraft. This enables aircraft ownership within the first 2 hours of gameplay.

### The First 2 Hours Problem

**Without Starter Loan:**
```
Hour 0:   Start with $50,000
Hour 1:   CPL obtained, $42,500 remaining
Hour 1-2: Quick Jobs earning ~$8,000-$12,000/flight
          Need 15-20 flights to afford $150,000 aircraft
          That's 10+ hours of gameplay!
```

**With Starter Loan:**
```
Hour 0:   Start with $50,000
Hour 1:   CPL obtained, $42,500 remaining
Hour 1-2: 3-4 Quick Jobs, earn ~$35,000
          Total: ~$77,500
          Apply for Starter Loan: $172,500
          Buy used Cessna 172: $250,000
          OWN FIRST AIRCRAFT! ✓
```

### Starter Loan Terms

| Feature | Starter Loan | Regular Loan |
|---------|--------------|--------------|
| Eligibility | CPL obtained, < $100k net worth | Credit score based |
| Max Amount | $250,000 | Based on credit |
| Interest Rate | 1.5% per game month | 2-8% per game month |
| Term | 6-12 game months | 1-24 game months |
| Down Payment | 0% | 5-25% |
| Collateral | Aircraft purchased | Varies |
| Approval | Instant | Credit check |
| Limit | Once per world | Unlimited |

### Starter Loan Flow

```
┌─────────────────────────────────────────────────────────────────┐
│              🎓 STARTER LOAN PROGRAM                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Congratulations on earning your CPL!                           │
│                                                                 │
│  You qualify for our Starter Loan Program - designed to help    │
│  new commercial pilots purchase their first aircraft.           │
│                                                                 │
│  YOUR ELIGIBILITY:                                              │
│  ✓ CPL License obtained                                         │
│  ✓ Net worth under $100,000 ($77,500 current)                   │
│  ✓ First loan in this world                                     │
│                                                                 │
│  STARTER LOAN TERMS:                                            │
│  ├── Maximum amount: $250,000                                   │
│  ├── Interest rate: 1.5% per game month                         │
│  ├── Term options: 6, 9, or 12 game months                      │
│  ├── Down payment: $0 required                                  │
│  └── Collateral: Aircraft purchased with loan                   │
│                                                                 │
│  💡 With your $77,500 + $250,000 loan, you can afford:          │
│     • Used Cessna 152 ($150,000) - keep $177,500 reserve        │
│     • Used Cessna 172 ($220,000) - keep $107,500 reserve        │
│     • Used Piper PA-28 ($200,000) - keep $127,500 reserve       │
│                                                                 │
│  [Apply for Starter Loan]                                       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### First Aircraft Options

| Aircraft | Price (Used) | Cargo Capacity | Range | Earnings/Flight |
|----------|--------------|----------------|-------|-----------------|
| Cessna 150/152 | $120,000-$160,000 | 200 lbs | 400nm | $6,000-$8,000 |
| Cessna 172 | $180,000-$280,000 | 400 lbs | 600nm | $8,000-$12,000 |
| Piper PA-28 | $160,000-$240,000 | 350 lbs | 500nm | $7,000-$10,000 |
| Beechcraft Musketeer | $140,000-$200,000 | 300 lbs | 550nm | $7,000-$9,000 |

### Loan Repayment Schedule

**Example: $172,500 loan for Cessna 172**

| Term | Monthly Payment | Total Interest | Total Repayment |
|------|-----------------|----------------|-----------------|
| 6 months | $29,831 | $6,488 | $178,988 |
| 9 months | $20,182 | $9,138 | $181,638 |
| 12 months | $15,349 | $11,688 | $184,188 |

**Can You Afford It?**
```
Monthly payment (12 mo): $15,349
Flights per game month needed: ~2 flights
(At $8,000 profit per flight)

Very manageable! Most players fly 10+ flights per game month.
```

### Alternative: Rental-Only Path

Some players may prefer to avoid loans:

```
RENTAL PATH TO OWNERSHIP:

Hour 0-1:   CPL obtained ($42,500)
Hour 1-5:   Quick Jobs only
            20 flights × $9,000 avg profit = $180,000
            Total: $222,500
Hour 5-6:   Buy first aircraft outright ($200,000)
            No debt, $22,500 reserve

Pros: No interest, no monthly payments
Cons: Takes 5-6 hours instead of 2 hours
      Lower profit per flight (rental cut)
```

### Starter Loan Entity

```csharp
public class StarterLoan
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }
    public Guid PlayerWorldId { get; set; }

    // Loan details
    public decimal Amount { get; set; }
    public decimal InterestRatePerMonth { get; set; }  // 0.015 = 1.5%
    public int TermMonths { get; set; }
    public decimal MonthlyPayment { get; set; }

    // Collateral
    public Guid PurchasedAircraftId { get; set; }

    // Status
    public LoanStatus Status { get; set; }
    public int PaymentsMade { get; set; }
    public decimal RemainingBalance { get; set; }
    public DateTime NextPaymentDue { get; set; }

    public DateTime ApprovedAt { get; set; }
    public DateTime? PaidOffAt { get; set; }
}
```

### Two Paths to Aircraft Ownership

```
┌─────────────────────────────────────────────────────────────────┐
│                PATH TO YOUR FIRST AIRCRAFT                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   PATH A: STARTER LOAN (Fast Track)          ⏱️ ~2 hours        │
│   ────────────────────────────────────                          │
│   1. Complete Discovery + PPL + CPL                             │
│   2. Do 3-4 Quick Jobs to build capital                         │
│   3. Apply for Starter Loan ($250k max)                         │
│   4. Buy your first aircraft!                                   │
│   5. Pay off loan while flying your own plane                   │
│                                                                 │
│   ✓ Own aircraft in 2 hours                                     │
│   ✓ Higher profit per flight (no rental cut)                    │
│   ✗ Monthly loan payments                                       │
│                                                                 │
│   ─────────────────── OR ───────────────────                    │
│                                                                 │
│   PATH B: RENTAL SAVINGS (Debt-Free)         ⏱️ ~6 hours        │
│   ────────────────────────────────────                          │
│   1. Complete Discovery + PPL + CPL                             │
│   2. Continue Quick Jobs with rentals                           │
│   3. Save up full aircraft purchase price                       │
│   4. Buy aircraft outright - no debt!                           │
│                                                                 │
│   ✓ No debt or monthly payments                                 │
│   ✓ Full profit immediately after purchase                      │
│   ✗ Takes longer to own aircraft                                │
│   ✗ Lower profit while renting (20%+ cut)                       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Experience & Skills System

### Overview
Players earn experience (XP) by completing activities. Leveling up grants skill points that can be spent on passive bonuses across four skill trees.

### XP Sources

| Activity | Base XP | Bonuses |
|----------|---------|---------|
| Job Completed | 100 × job tier | +50% on-time, +25% perfect cargo |
| Exam Passed | 500 × license tier | +100% first attempt, +50% score >90% |
| Flight Completed | 10 × flight hours | +25% for night, +25% for IFR |
| Landing | 25 per landing | +50% smooth (<-150fpm), +100% greaser (<-50fpm) |
| First Achievements | 1,000 (one-time) | First job, first exam, first aircraft, etc. |
| Milestone Flights | 2,500 (one-time) | 10/50/100/500/1000 hours milestones |

### Job Tier XP Multipliers

| Job Tier | XP Multiplier | Example |
|----------|---------------|---------|
| Standard (⭐) | ×1.0 | 100 XP |
| Priority (⭐⭐) | ×1.5 | 150 XP |
| Specialized (⭐⭐⭐) | ×2.0 | 200 XP |
| Complex (⭐⭐⭐⭐) | ×3.0 | 300 XP |
| Critical (⭐⭐⭐⭐⭐) | ×5.0 | 500 XP |

### Leveling Formula

```
XP Required for Level N = 1,000 × (N ^ 1.5)

Level 1 → 2:     1,000 XP
Level 5 → 6:     11,180 XP
Level 10 → 11:   31,623 XP
Level 25 → 26:   125,000 XP
Level 50 → 51:   353,553 XP
Level 100:       1,000,000 XP total
```

### Skill Points

| Source | Points Awarded |
|--------|----------------|
| Each level up | 1 point |
| Level 10 milestone | +2 bonus points |
| Level 25 milestone | +3 bonus points |
| Level 50 milestone | +5 bonus points |
| Level 75 milestone | +5 bonus points |
| Level 100 milestone | +10 bonus points |

**Max Points at Level 100:** ~125 skill points

### Skill Trees

Players can spend skill points across four trees. Each tree has tiers that unlock progressively.

```
┌─────────────────────────────────────────────────────────────────┐
│                        SKILL TREES                              │
├────────────────┬────────────────┬───────────────┬───────────────┤
│   💰 ECONOMY   │   ✈️ AVIATION   │  📈 BUSINESS  │  ⭐ REPUTATION │
├────────────────┼────────────────┼───────────────┼───────────────┤
│ Cost savings   │ Flight perks   │ More options  │ Rep bonuses   │
│ Better deals   │ XP bonuses     │ Better access │ Faster growth │
└────────────────┴────────────────┴───────────────┴───────────────┘
```

### Economy Tree (💰)

| Skill | Tier | Points | Effect | Prerequisite |
|-------|------|--------|--------|--------------|
| Fuel Saver I | 1 | 1 | -5% fuel costs | None |
| Fuel Saver II | 2 | 2 | -10% fuel costs | Fuel Saver I |
| Fuel Saver III | 3 | 3 | -15% fuel costs | Fuel Saver II |
| Maintenance Pro I | 1 | 1 | -5% maintenance costs | None |
| Maintenance Pro II | 2 | 2 | -10% maintenance costs | Maintenance Pro I |
| Maintenance Pro III | 3 | 3 | -15% maintenance costs | Maintenance Pro II |
| Smart Buyer I | 1 | 1 | -3% aircraft purchase price | None |
| Smart Buyer II | 2 | 2 | -5% aircraft purchase price | Smart Buyer I |
| Smart Buyer III | 3 | 3 | -8% aircraft purchase price | Smart Buyer II |
| Loan Negotiator I | 2 | 2 | -0.5% loan interest rate | Any Tier 1 |
| Loan Negotiator II | 3 | 3 | -1.0% loan interest rate | Loan Negotiator I |
| **Economy Master** | 4 | 5 | All economy bonuses +5% | All Tier 3 Economy |

### Aviation Tree (✈️)

| Skill | Tier | Points | Effect | Prerequisite |
|-------|------|--------|--------|--------------|
| Smooth Operator I | 1 | 1 | Landing tolerance +10% | None |
| Smooth Operator II | 2 | 2 | Landing tolerance +20% | Smooth Operator I |
| Smooth Operator III | 3 | 3 | Landing tolerance +30% | Smooth Operator II |
| Fuel Efficiency I | 1 | 1 | -5% fuel consumption | None |
| Fuel Efficiency II | 2 | 2 | -10% fuel consumption | Fuel Efficiency I |
| Fuel Efficiency III | 3 | 3 | -15% fuel consumption | Fuel Efficiency II |
| XP Boost I | 1 | 1 | +10% flight XP | None |
| XP Boost II | 2 | 2 | +20% flight XP | XP Boost I |
| XP Boost III | 3 | 3 | +30% flight XP | XP Boost II |
| Exam Expert I | 2 | 2 | +5 exam score bonus | Any Tier 1 |
| Exam Expert II | 3 | 3 | +10 exam score bonus | Exam Expert I |
| **Aviation Master** | 4 | 5 | All aviation bonuses +5% | All Tier 3 Aviation |

### Business Tree (📈)

| Skill | Tier | Points | Effect | Prerequisite |
|-------|------|--------|--------|--------------|
| Network I | 1 | 1 | +10% more jobs available | None |
| Network II | 2 | 2 | +20% more jobs available | Network I |
| Network III | 3 | 3 | +30% more jobs available | Network II |
| Negotiator I | 1 | 1 | +5% job payouts | None |
| Negotiator II | 2 | 2 | +10% job payouts | Negotiator I |
| Negotiator III | 3 | 3 | +15% job payouts | Negotiator II |
| Rental Discount I | 1 | 1 | -5% rental fees | None |
| Rental Discount II | 2 | 2 | -10% rental fees | Rental Discount I |
| Rental Discount III | 3 | 3 | -15% rental fees | Rental Discount II |
| Premium Access I | 2 | 2 | Unlock ⭐⭐⭐ jobs earlier | Any Tier 1 |
| Premium Access II | 3 | 3 | Unlock ⭐⭐⭐⭐ jobs earlier | Premium Access I |
| **Business Master** | 4 | 5 | All business bonuses +5% | All Tier 3 Business |

### Reputation Tree (⭐)

| Skill | Tier | Points | Effect | Prerequisite |
|-------|------|--------|--------|--------------|
| Fast Rep I | 1 | 1 | +10% reputation gain | None |
| Fast Rep II | 2 | 2 | +20% reputation gain | Fast Rep I |
| Fast Rep III | 3 | 3 | +30% reputation gain | Fast Rep II |
| Forgiveness I | 1 | 1 | -10% rep loss from failures | None |
| Forgiveness II | 2 | 2 | -20% rep loss from failures | Forgiveness I |
| Forgiveness III | 3 | 3 | -30% rep loss from failures | Forgiveness II |
| Credit Builder I | 1 | 1 | +1 credit score per payment | None |
| Credit Builder II | 2 | 2 | +2 credit score per payment | Credit Builder I |
| Credit Builder III | 3 | 3 | +3 credit score per payment | Credit Builder II |
| VIP Status I | 2 | 2 | Access exclusive contracts | Any Tier 1 |
| VIP Status II | 3 | 3 | Better exclusive contracts | VIP Status I |
| **Reputation Master** | 4 | 5 | All reputation bonuses +5% | All Tier 3 Reputation |

### Skill Application

Skills are applied automatically when their conditions are met:

```csharp
public class SkillService
{
    public decimal ApplyFuelCostModifier(Guid playerId, decimal baseCost)
    {
        var skills = GetPlayerSkills(playerId);
        decimal modifier = 1.0m;

        // Economy Tree - Fuel Saver
        if (skills.Has("FuelSaver3")) modifier -= 0.15m;
        else if (skills.Has("FuelSaver2")) modifier -= 0.10m;
        else if (skills.Has("FuelSaver1")) modifier -= 0.05m;

        // Mastery bonus
        if (skills.Has("EconomyMaster")) modifier -= 0.05m;

        return baseCost * modifier;
    }

    public decimal ApplyJobPayoutModifier(Guid playerId, decimal basePayout)
    {
        var skills = GetPlayerSkills(playerId);
        decimal modifier = 1.0m;

        // Business Tree - Negotiator
        if (skills.Has("Negotiator3")) modifier += 0.15m;
        else if (skills.Has("Negotiator2")) modifier += 0.10m;
        else if (skills.Has("Negotiator1")) modifier += 0.05m;

        // Mastery bonus
        if (skills.Has("BusinessMaster")) modifier += 0.05m;

        return basePayout * modifier;
    }
}
```

### Skill Entities

```csharp
public class PlayerExperience
{
    public Guid Id { get; set; }
    public Guid PlayerWorldId { get; set; }

    public int Level { get; set; }
    public long TotalXP { get; set; }
    public long XPToNextLevel { get; set; }

    public int AvailableSkillPoints { get; set; }
    public int TotalSkillPointsEarned { get; set; }

    public DateTime LastXPGainAt { get; set; }
}

public class PlayerSkill
{
    public Guid Id { get; set; }
    public Guid PlayerWorldId { get; set; }

    public string SkillId { get; set; }        // "FuelSaver1", "Negotiator2", etc.
    public string SkillTree { get; set; }      // "Economy", "Aviation", etc.
    public int Tier { get; set; }
    public int PointsSpent { get; set; }

    public DateTime UnlockedAt { get; set; }
}

public class XPTransaction
{
    public Guid Id { get; set; }
    public Guid PlayerWorldId { get; set; }

    public XPSource Source { get; set; }
    public long Amount { get; set; }
    public string Description { get; set; }

    public Guid? RelatedEntityId { get; set; }  // Job ID, Exam ID, etc.

    public DateTime EarnedAt { get; set; }
}

public enum XPSource
{
    JobCompletion,
    ExamPassed,
    FlightCompleted,
    Landing,
    Achievement,
    Milestone,
    Bonus
}
```

### Skill UI (Web App)

```
┌─────────────────────────────────────────────────────────────────┐
│  SKILLS & EXPERIENCE                           Level 27 ⭐      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  XP: 142,500 / 166,375                    [████████░░] 86%     │
│  Available Skill Points: 3                                      │
│                                                                 │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌────────────┐│
│  │ 💰 ECONOMY  │ │ ✈️ AVIATION │ │ 📈 BUSINESS │ │⭐ REPUTATION││
│  │  12 points  │ │  8 points   │ │  6 points   │ │  4 points  ││
│  └─────────────┘ └─────────────┘ └─────────────┘ └────────────┘│
│                                                                 │
│  ECONOMY TREE:                                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ Fuel Saver     [I ✓] [II ✓] [III ○]     -10% fuel costs │   │
│  │ Maintenance    [I ✓] [II ✓] [III ✓]     -15% maint      │   │
│  │ Smart Buyer    [I ✓] [II ○] [III ○]     -3% purchase    │   │
│  │ Loan Nego      [I ○] [II ○]             Locked          │   │
│  │ ────────────────────────────────────────────────────────│   │
│  │ 🏆 Economy Master [○]  Requires all Tier 3              │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  [Unlock Fuel Saver III - 3 points]                            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Community Aircraft Submission System

### Overview
In-game aircraft (add-ons, default aircraft) have values that differ from real-world specifications. Players can submit aircraft from their simulator with actual sim values, which admins review and map to our aircraft type database.

### The Problem

```
Real World Cessna 172S:          MSFS Default C172:
- Empty Weight: 1,680 lbs        - Empty Weight: 1,764 lbs
- Max Fuel: 53 gal               - Max Fuel: 53 gal
- MTOW: 2,550 lbs                - MTOW: 2,558 lbs

Third-Party A2A C172:            Carenado C172:
- Empty Weight: 1,672 lbs        - Empty Weight: 1,701 lbs
- Max Fuel: 53 gal               - Max Fuel: 53 gal
- MTOW: 2,550 lbs                - MTOW: 2,550 lbs
```

Each add-on has different values. We need to capture actual sim values, not assume real-world specs.

### How It Works

```
┌─────────────────────────────────────────────────────────────────┐
│                 AIRCRAFT SUBMISSION FLOW                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. PLAYER IN SIMULATOR                                         │
│     ├── Loads desired aircraft                                  │
│     ├── Connector reads SimConnect data automatically           │
│     └── Player clicks "Submit This Aircraft" in app             │
│                                                                 │
│  2. DATA CAPTURED (via SimConnect)                              │
│     ├── Aircraft title (sim internal name)                      │
│     ├── ATC model and type                                      │
│     ├── Empty weight                                            │
│     ├── Max gross weight                                        │
│     ├── Fuel tank capacities (per tank)                         │
│     ├── Number of engines                                       │
│     ├── Engine type (piston/turboprop/jet)                      │
│     ├── Number of payload stations                              │
│     └── Performance data (speeds, ceiling)                      │
│                                                                 │
│  3. PLAYER ADDS INFO                                            │
│     ├── Friendly name ("Carenado Cessna 172SP")                 │
│     ├── Developer/Publisher ("Carenado")                        │
│     ├── Category suggestion (SEP, MEP, Jet, etc.)               │
│     └── Optional: Screenshots, notes                            │
│                                                                 │
│  4. SUBMISSION CREATED                                          │
│     └── Status: "Pending Review"                                │
│     └── Player notified: "24-48 hours for review"               │
│                                                                 │
│  5. ADMIN REVIEW                                                │
│     ├── Verify data looks reasonable                            │
│     ├── Map to existing AircraftType in database                │
│     ├── Set category, license requirements                      │
│     └── Status: "Pending Approval"                              │
│                                                                 │
│  6. SUPER ADMIN APPROVAL                                        │
│     ├── Final review of mapping                                 │
│     ├── Approve or request changes                              │
│     └── Status: "Approved" or "Rejected"                        │
│                                                                 │
│  7. AVAILABLE FOR USE                                           │
│     ├── Aircraft appears in player's hangar                     │
│     ├── Other players can see and request same aircraft         │
│     └── Linked to our AircraftType for pricing/jobs             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### SimConnect Data Captured

```cpp
struct AircraftSubmissionData {
    // Identity (from sim)
    std::string simTitle;           // "Cessna Skyhawk G1000 Asobo"
    std::string atcType;            // "C172"
    std::string atcModel;           // "Cessna 172"

    // Weights (lbs)
    float emptyWeight;
    float maxGrossWeight;
    float maxPayload;

    // Fuel (gallons per tank)
    std::vector<FuelTank> fuelTanks;
    float totalFuelCapacity;

    // Engines
    int engineCount;
    EngineType engineType;          // Piston, Turboprop, Jet

    // Payload
    int payloadStationCount;
    std::vector<PayloadStation> payloadStations;

    // Performance (estimated from sim)
    float cruiseSpeedKts;
    float maxSpeedKts;
    float serviceCeilingFt;
    float rangeNm;

    // Metadata
    std::string simPlatform;        // "MSFS", "XPlane", "P3D"
    std::string capturedAt;
};

struct FuelTank {
    std::string name;               // "Left Main", "Right Aux"
    float capacityGal;
    float currentGal;
};

struct PayloadStation {
    int index;
    std::string name;               // "Pilot", "Cargo Aft"
    float maxWeightLbs;
};
```

### Submission Entity

```csharp
public class AircraftSubmission
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }
    public Guid SubmittedByPlayerId { get; set; }

    // Status
    public SubmissionStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Player-provided info
    public string FriendlyName { get; set; }        // "Carenado C172SP"
    public string DeveloperName { get; set; }       // "Carenado"
    public string CategorySuggestion { get; set; }  // "SEP"
    public string Notes { get; set; }
    public string ScreenshotUrls { get; set; }      // JSON array

    // SimConnect captured data (stored as JSON)
    public string SimTitle { get; set; }
    public string AtcType { get; set; }
    public string AtcModel { get; set; }
    public string SimPlatform { get; set; }         // MSFS, XPlane, P3D
    public string RawSimData { get; set; }          // Full JSON blob

    // Extracted values
    public float EmptyWeightLbs { get; set; }
    public float MaxGrossWeightLbs { get; set; }
    public float TotalFuelCapacityGal { get; set; }
    public int EngineCount { get; set; }
    public EngineType EngineType { get; set; }
    public int PayloadStationCount { get; set; }
    public float CruiseSpeedKts { get; set; }

    // Admin mapping (after review)
    public Guid? MappedAircraftTypeId { get; set; }
    public AircraftType MappedAircraftType { get; set; }
    public string AssignedCategory { get; set; }    // SEP, MEP, Turboprop, Jet
    public string LicenseRequired { get; set; }     // PPL, CPL, Type Rating

    // Review info
    public Guid? ReviewedByUserId { get; set; }
    public string ReviewerNotes { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public string RejectionReason { get; set; }
}

public enum SubmissionStatus
{
    Pending,            // Just submitted
    InReview,           // Admin picked it up
    PendingApproval,    // Admin done, awaiting Super Admin
    Approved,           // Ready for use
    Rejected,           // Not approved (with reason)
    NeedsInfo           // Admin needs more info from player
}

public enum EngineType
{
    Piston,
    Turboprop,
    Turbojet,
    Turbofan
}
```

### Approved Aircraft Entity

Once approved, creates a usable aircraft definition:

```csharp
public class CommunityAircraft
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }
    public Guid SubmissionId { get; set; }
    public Guid AircraftTypeId { get; set; }        // Our base type

    // Identity
    public string SimTitle { get; set; }            // Exact sim match
    public string FriendlyName { get; set; }
    public string DeveloperName { get; set; }
    public string SimPlatform { get; set; }

    // Actual sim values (for validation)
    public float EmptyWeightLbs { get; set; }
    public float MaxGrossWeightLbs { get; set; }
    public float TotalFuelCapacityGal { get; set; }
    public int EngineCount { get; set; }
    public EngineType EngineType { get; set; }

    // Classification
    public string Category { get; set; }            // SEP, MEP, etc.
    public string LicenseRequired { get; set; }

    // Stats
    public int TimesUsed { get; set; }
    public int UniqueUsers { get; set; }

    public bool IsActive { get; set; }
    public DateTime ApprovedAt { get; set; }
}
```

### Admin Review Interface

```
┌─────────────────────────────────────────────────────────────────┐
│  AIRCRAFT SUBMISSION REVIEW                    ID: SUB-4521     │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  SUBMITTED BY: SkyPilot_John           STATUS: Pending Review   │
│  SUBMITTED: 2 hours ago                PLATFORM: MSFS           │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  PLAYER PROVIDED:                                        │   │
│  │  Name: "Carenado Cessna 172SP G1000"                     │   │
│  │  Developer: Carenado                                     │   │
│  │  Suggested Category: SEP                                 │   │
│  │  Notes: "Great GA aircraft, very realistic"              │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  SIMCONNECT DATA:                                        │   │
│  │  Sim Title: "Carenado_C172SP_G1000"                      │   │
│  │  ATC Type: C172                                          │   │
│  │  ──────────────────────────────────────────────────────  │   │
│  │  Empty Weight:     1,701 lbs                             │   │
│  │  Max Gross Weight: 2,550 lbs                             │   │
│  │  Fuel Capacity:    53 gal (2 tanks)                      │   │
│  │  Engines:          1 × Piston                            │   │
│  │  Payload Stations: 4                                     │   │
│  │  Cruise Speed:     124 kts                               │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  MAP TO AIRCRAFT TYPE:                                          │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Search: [C172____________]                              │   │
│  │                                                          │   │
│  │  ○ Cessna 172 Skyhawk (SEP) - Base model                │   │
│  │  ● Cessna 172SP Skyhawk SP (SEP) - Best match           │   │
│  │  ○ Cessna 172RG Cutlass (SEP) - Retractable             │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  ASSIGNED CATEGORY: [SEP ▼]    LICENSE: [PPL ▼]                │
│                                                                 │
│  REVIEWER NOTES:                                                │
│  [Values match expected C172SP specs. Approved for mapping._]   │
│                                                                 │
│  [Approve & Send to Super Admin]  [Request More Info]  [Reject] │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Player Submission Interface

```
┌─────────────────────────────────────────────────────────────────┐
│  SUBMIT YOUR AIRCRAFT                                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Current Aircraft Detected:                                     │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  📍 "Carenado_C172SP_G1000"                              │   │
│  │                                                          │   │
│  │  Detected Values:                                        │   │
│  │  • Empty Weight: 1,701 lbs                               │   │
│  │  • Max Gross: 2,550 lbs                                  │   │
│  │  • Fuel: 53 gal                                          │   │
│  │  • Engines: 1 × Piston                                   │   │
│  │                                                          │   │
│  │  ✓ Data captured successfully                            │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  Please provide additional information:                         │
│                                                                 │
│  Friendly Name: [Carenado Cessna 172SP G1000_______________]   │
│  Developer:     [Carenado_________________________________]    │
│  Category:      [Single Engine Piston (SEP) ▼]                 │
│                                                                 │
│  Notes (optional):                                              │
│  [Great aircraft for GA flying, very realistic systems___]      │
│                                                                 │
│  ⚠️ Review typically takes 24-48 hours. You'll be notified     │
│     when your aircraft is approved and ready to use.            │
│                                                                 │
│  [Submit Aircraft for Review]                                   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Submission Status Tracking

```
┌─────────────────────────────────────────────────────────────────┐
│  MY AIRCRAFT SUBMISSIONS                                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ Carenado C172SP G1000                                    │   │
│  │ Submitted: 2 days ago                                    │   │
│  │ Status: ✅ APPROVED                                      │   │
│  │ → Now available in your aircraft list                    │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ PMDG 737-800                                             │   │
│  │ Submitted: 6 hours ago                                   │   │
│  │ Status: 🔄 IN REVIEW                                     │   │
│  │ → Admin is reviewing your submission                     │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ FlyByWire A320neo                                        │   │
│  │ Submitted: 1 hour ago                                    │   │
│  │ Status: ⏳ PENDING                                       │   │
│  │ → Waiting for admin review (typically 24-48 hrs)         │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Connector Implementation

```cpp
class AircraftSubmissionCapture {
public:
    AircraftSubmissionData captureCurrentAircraft() {
        AircraftSubmissionData data;

        // Read aircraft identity
        SimConnect_RequestDataOnSimObject(
            hSimConnect, REQUEST_AIRCRAFT_INFO,
            AIRCRAFT_INFO_DEFINITION, SIMCONNECT_OBJECT_ID_USER,
            SIMCONNECT_PERIOD_ONCE
        );

        // Data populated in callback:
        data.simTitle = aircraftInfo.title;
        data.atcType = aircraftInfo.atcType;
        data.atcModel = aircraftInfo.atcModel;

        // Read weights
        data.emptyWeight = getSimVar("EMPTY WEIGHT");
        data.maxGrossWeight = getSimVar("MAX GROSS WEIGHT");

        // Read fuel tanks
        data.fuelTanks = captureFuelTanks();
        data.totalFuelCapacity = calculateTotalFuel(data.fuelTanks);

        // Read engines
        data.engineCount = (int)getSimVar("NUMBER OF ENGINES");
        data.engineType = detectEngineType();

        // Read payload stations
        data.payloadStationCount = (int)getSimVar("PAYLOAD STATION COUNT");
        data.payloadStations = capturePayloadStations();

        // Read performance
        data.cruiseSpeedKts = getSimVar("CRUISE SPEED");
        data.serviceCeilingFt = getSimVar("SERVICE CEILING");

        data.simPlatform = "MSFS";
        data.capturedAt = getCurrentTimestamp();

        return data;
    }

private:
    EngineType detectEngineType() {
        int engType = (int)getSimVar("ENGINE TYPE");
        switch (engType) {
            case 0: return EngineType::Piston;
            case 1: return EngineType::Turboprop;
            case 2: return EngineType::Turbojet;
            case 5: return EngineType::Turbofan;
            default: return EngineType::Piston;
        }
    }

    std::vector<FuelTank> captureFuelTanks() {
        std::vector<FuelTank> tanks;

        // Check each possible tank
        const char* tankNames[] = {
            "FUEL TANK LEFT MAIN CAPACITY",
            "FUEL TANK RIGHT MAIN CAPACITY",
            "FUEL TANK CENTER CAPACITY",
            "FUEL TANK LEFT AUX CAPACITY",
            "FUEL TANK RIGHT AUX CAPACITY"
        };

        for (const char* tankName : tankNames) {
            float capacity = getSimVar(tankName);
            if (capacity > 0) {
                FuelTank tank;
                tank.name = tankName;
                tank.capacityGal = capacity;
                tanks.push_back(tank);
            }
        }

        return tanks;
    }
};
```

### Aircraft Matching During Flight

When a player starts flying, the connector matches their aircraft:

```cpp
bool matchAircraftToDatabase(const std::string& simTitle) {
    // First, check community aircraft (exact sim title match)
    auto communityMatch = api.findCommunityAircraft(simTitle);
    if (communityMatch.found) {
        currentAircraftId = communityMatch.id;
        return true;
    }

    // If no exact match, prompt player to submit
    ui.showMessage("This aircraft isn't in our database yet. "
                   "Would you like to submit it for approval?");

    return false;
}
```

---

## Flight Restrictions & Enforcement

### Overview
The connector enforces license-based restrictions in real-time. Players cannot fly outside their qualifications.

### Restriction Types

| Restriction | Required License | Enforcement |
|-------------|-----------------|-------------|
| Night Flying | Night Rating | Block engine start after sunset without rating |
| IMC/IFR Flight | Instrument Rating (IR) | Warning when entering clouds without IR |
| Multi-Engine | MEP Rating | Cannot start multi-engine aircraft |
| Turbine Aircraft | Type Rating | Cannot start turboprops/jets without rating |
| Specific Type | Type Rating (e.g., A320) | Cannot start specific aircraft without type |
| Commercial Ops | CPL minimum | Cannot accept paying jobs with PPL only |
| Airline Ops | ATPL | Cannot fly scheduled airline routes |

### Connector Enforcement Logic
```cpp
struct FlightRestrictions {
    // Pre-flight checks (before engine start)
    bool canStartAircraft(AircraftType type, PlayerLicenses licenses) {
        // Check aircraft category rating
        if (type.isMultiEngine && !licenses.hasMEP) return false;
        if (type.isTurbine && !licenses.hasTypeRating(type)) return false;
        return true;
    }

    // In-flight monitoring
    void monitorFlight(FlightState state, PlayerLicenses licenses) {
        // Night check
        if (state.isNightTime && !licenses.hasNightRating) {
            issueViolation("NIGHT_FLYING_WITHOUT_RATING");
        }

        // IMC check
        if (state.inClouds && !licenses.hasIR) {
            issueWarning("IMC_WITHOUT_IR");
        }

        // Speed restriction (250kts below 10,000ft)
        if (state.altitudeFt < 10000 && state.indicatedAirspeedKts > 250) {
            issueViolation("SPEED_VIOLATION_BELOW_10K");
        }
    }
};
```

### Violation Consequences
| Violation | First Offense | Repeat |
|-----------|--------------|--------|
| Night without rating | Warning + job cancelled | $5,000 fine + 24hr suspension |
| IMC without IR | Warning | $10,000 fine |
| Speed violation | -5 exam points | -10 exam points + $2,000 fine |
| Wrong aircraft type | Cannot start (blocked) | N/A |
| G-force excess (>2G) | Warning | -5 exam points per occurrence |

### Night Time Definition
```
Night = 30 minutes after sunset to 30 minutes before sunrise
Based on aircraft's current position (lat/lon)
Calculated using astronomical algorithms
```

---

## Aircraft Location & Transport System

### Overview
Aircraft exist at specific airports. Players must physically be at an aircraft's location to fly it, or pay for transport.

### Aircraft Location
```csharp
public class OwnedAircraft
{
    // ... other properties
    public string CurrentLocationIcao { get; set; }  // Where aircraft is parked
    public DateTime LastMovedAt { get; set; }
    public bool IsInMaintenance { get; set; }
    public bool IsInUse { get; set; }  // Currently being flown
}
```

### Player Location
```csharp
public class PlayerWorld
{
    // ... other properties
    public string CurrentLocationIcao { get; set; }  // Player's current airport
    public DateTime LastLocationUpdateAt { get; set; }
}
```

### Location Update Rules
```
1. Player completes flight A → B:
   - Player location updates to B
   - Aircraft location updates to B
   - Player MUST be at B for next flight

2. Player wants to fly aircraft at different airport:
   - Option A: Quick Transport (pay fee, instant)
   - Option B: Fly another aircraft there
   - Option C: Wait (future: commercial flight simulation)
```

### Quick Transport Pricing
```
TRANSPORT_COST = BASE_FEE + (DISTANCE_NM × RATE_PER_NM)

Distance Brackets:
- 0-100nm:    $500 base + $3/nm = $500-$800
- 100-500nm:  $500 base + $5/nm = $800-$3,000
- 500-1000nm: $500 base + $8/nm = $3,000-$8,500
- 1000nm+:    $500 base + $12/nm = $8,500+

Examples:
- EGLL to EGCC (160nm): $500 + (160 × $5) = $1,300
- KJFK to KLAX (2,150nm): $500 + (2150 × $12) = $26,300
- EGLL to LFPG (190nm): $500 + (190 × $5) = $1,450
```

### Transport Entity
```csharp
public class TransportRequest
{
    public Guid Id { get; set; }
    public Guid PlayerWorldId { get; set; }
    public string FromIcao { get; set; }
    public string ToIcao { get; set; }
    public double DistanceNm { get; set; }
    public decimal Cost { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime CompletedAt { get; set; }
}
```

### API Endpoints
```
GET  /api/worlds/{worldId}/my-location          - Get player's current location
GET  /api/worlds/{worldId}/aircraft/{id}/location - Get aircraft location
POST /api/worlds/{worldId}/transport/quote      - Get transport price quote
POST /api/worlds/{worldId}/transport/request    - Pay and transport player
```

---

## Quick Jobs & Aircraft Rental System

### Overview
New players can start earning money immediately through "Quick Jobs" - jobs that include aircraft rental. This allows players to take commercial flights without owning an aircraft, paying a rental fee that's deducted from the job payout.

### Rental Sources

**1. System Fleet (Always Available)**
- Standard aircraft at major airports
- Higher rental percentage (system takes larger cut)
- Guaranteed availability
- Basic insurance included
- No relationship building needed

**2. Player Fleet (Passive Income)**
- Other players list their aircraft for rent
- Owner sets rental percentage (within limits)
- Owner earns passive income when aircraft is rented
- Aircraft gains flight hours and wear
- Owner responsible for maintenance

### Rental Fee Structure (% of Job Payout)

| Aircraft Class | System Rental | Player Rental Range | Min Player Rate |
|----------------|---------------|---------------------|-----------------|
| Light Single (C172, PA28) | 20% | 10-25% | 10% |
| Light Twin (C421, PA34) | 25% | 15-30% | 15% |
| Turboprop Single (TBM, PC-12) | 28% | 18-35% | 18% |
| Turboprop Twin (King Air) | 30% | 20-35% | 20% |
| Regional Jet (CRJ, E-Jets) | 35% | 25-40% | 25% |
| Narrow Body (A320, B737) | 38% | 28-45% | 28% |
| Wide Body (A350, B777) | 40% | 30-50% | 30% |

### Quick Job Flow

```
1. Player browses available jobs
2. Player selects job (doesn't own suitable aircraft)
3. System shows "Quick Job" option with available rentals:
   ┌─────────────────────────────────────────────────────┐
   │  QUICK JOB: Cargo to EGCC                          │
   │  Job Payout: $12,000                               │
   │                                                     │
   │  Available Rentals at EGLL:                        │
   │  ┌─────────────────────────────────────────────┐   │
   │  │ [SYSTEM] Cessna 172 - 20% ($2,400)          │   │
   │  │          Your Profit: $9,600                │   │
   │  ├─────────────────────────────────────────────┤   │
   │  │ [PLAYER] JohnDoe's PA-28 - 15% ($1,800)     │   │
   │  │          Your Profit: $10,200               │   │
   │  └─────────────────────────────────────────────┘   │
   └─────────────────────────────────────────────────────┘
4. Player selects rental and accepts job
5. Player flies and completes job
6. Payout distributed automatically:
   - Rental fee → owner (system or player)
   - Insurance fee → system (if applicable)
   - Remainder → pilot
```

### Rental Payout Distribution

**System Rental Example:**
```
Job Payout:        $12,000
System Rental:     -$2,400 (20%) → System
Insurance:         -$200 (flat fee)
─────────────────────────────
Pilot Profit:      $9,400
```

**Player Rental Example:**
```
Job Payout:        $12,000
Owner Rental:      -$1,800 (15%) → Aircraft Owner
Platform Fee:      -$180 (10% of rental) → System
Insurance:         -$150 (lower for player rentals)
─────────────────────────────
Pilot Profit:      $9,870
Owner Profit:      $1,620 (after platform fee)
```

### Aircraft Rental Listing (For Owners)

Players can list their aircraft for rent:
```csharp
public class AircraftRentalListing
{
    public Guid Id { get; set; }
    public Guid OwnedAircraftId { get; set; }
    public Guid WorldId { get; set; }

    public decimal RentalPercentage { get; set; }  // 10-50%
    public bool IsActive { get; set; }

    // Restrictions
    public int? MinimumPilotRating { get; set; }  // Reputation score
    public bool RequireCPL { get; set; }
    public bool AllowInternational { get; set; }

    // Stats
    public int TotalRentals { get; set; }
    public decimal TotalEarnings { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Rental Insurance

| Coverage | Cost | Covers |
|----------|------|--------|
| Basic (System) | $200/job | Damage up to $50k |
| Standard (Player) | $150/job | Damage up to $25k |
| Premium | $500/job | Full replacement value |

**Damage Responsibility:**
- Hard landings, minor incidents: Insurance covers
- Pilot negligence (gear-up landing): Pilot pays deductible
- Total loss: Insurance + pilot deductible (10% of value, max $50k)

### Why Quick Jobs?

**For New Players:**
- Start earning immediately after getting CPL
- Learn different aircraft before buying
- Build reputation and capital
- No upfront aircraft investment

**For Aircraft Owners:**
- Passive income while not flying
- Aircraft stays active (good for economy)
- Builds fleet utilization stats
- Can set restrictions on who rents

---

## License Exam System

### Overview
Players must pass practical flight exams to earn licenses. Exams are started from the web app and tracked by the connector.

### Design Goal: CPL in ~1 Hour

A skilled player should be able to progress from zero to CPL (Commercial Pilot License) in approximately **1 hour of gameplay**. This allows new players to quickly start taking paid jobs and earning money.

**Fast Track to Commercial Operations:**
```
Discovery Flight (10 min) → PPL Exam (25 min) → CPL Exam (30 min)
Total: ~65 minutes of flight time
```

### Exam Qualification Path

```
                              ┌─────────────────────┐
                              │   START (New Player) │
                              └──────────┬──────────┘
                                         │
                                         ▼
                    ┌────────────────────────────────────────┐
                    │     DISCOVERY FLIGHT (10 min, $500)    │
                    │   Basic: Takeoff, fly, navigate, land  │
                    │         ✓ Pass → Unlocks PPL           │
                    └──────────────────┬─────────────────────┘
                                       │
                                       ▼
                    ┌────────────────────────────────────────┐
                    │       PPL EXAM (25 min, $2,000)        │
                    │   Maneuvers, navigation, 3 landings    │
                    │     ✓ Pass → Unlocks everything below  │
                    └──────────────────┬─────────────────────┘
                                       │
           ┌───────────────┬───────────┼───────────┬─────────────────┐
           │               │           │           │                 │
           ▼               ▼           ▼           ▼                 ▼
    ┌─────────────┐ ┌─────────────┐ ┌─────────┐ ┌─────────┐  ┌──────────────┐
    │ CPL EXAM    │ │NIGHT RATING │ │ IR EXAM │ │MEP EXAM │  │ BASIC TYPE   │
    │ (30 min)    │ │ (30 min)    │ │ (45 min)│ │ (45 min)│  │ RATINGS      │
    │ $5,000      │ │ $3,000      │ │ $5,000  │ │ $4,000  │  │ (SEP only)   │
    └──────┬──────┘ └─────────────┘ └────┬────┘ └────┬────┘  └──────────────┘
           │                              │          │
           │         ┌────────────────────┴──────────┘
           │         │
           ▼         ▼
    ┌─────────────────────────────────────────────────────┐
    │              TYPE RATINGS (45-90 min)               │
    │       Requires: CPL (or ATPL for airline types)     │
    │    Multi-airport circuit with touch-and-go's        │
    └──────────────────────────┬──────────────────────────┘
                               │
                               │ (Requires: CPL + IR + MEP)
                               ▼
                    ┌─────────────────────────────────────┐
                    │      ATPL EXAM (90 min, $10,000)    │
                    │    Multi-crew, airline procedures   │
                    │    ✓ Pass → Airline operations      │
                    └─────────────────────────────────────┘
```

### Exam Types (Updated for Fast Progression)

| Exam | Prerequisites | Duration | Cost | Unlocks |
|------|--------------|----------|------|---------|
| Discovery Flight | None | 10 min | $500 | PPL Exam |
| PPL | Discovery Flight ✓ | 25 min | $2,000 | CPL, Night, IR, MEP, Basic Types |
| Night Rating | PPL | 30 min | $3,000 | Night flying, night jobs |
| IR (Instrument) | PPL | 45 min | $5,000 | IFR flight, IFR jobs, ATPL |
| MEP (Multi-Engine) | PPL | 45 min | $4,000 | Multi-engine aircraft, ATPL |
| CPL | PPL | 30 min | $5,000 | Commercial jobs, Type Ratings |
| Type Rating | CPL (or ATPL) | 45-90 min | $5k-$20k | Specific aircraft operation |
| ATPL | CPL + IR + MEP | 90 min | $10,000 | Airline operations |

**No Hour Requirements**: Skill > time logged. Pass the practical test = earn the license.

### Discovery Flight (New Player Entry Point)

The Discovery Flight is designed to get new players into the action quickly while ensuring they have basic aircraft control.

**Requirements:**
- Duration: 10 minutes
- Aircraft: Any single-engine piston
- Cost: $500

**Tasks:**
1. Start at designated airport
2. Takeoff and climb to 2,000ft AGL
3. Fly to waypoint (5-10nm away)
4. Perform one 360° turn
5. Return to airport
6. Land safely

**Scoring:**
- Takeoff: 15 points
- Navigation: 20 points
- Turn (maintain altitude ±200ft): 15 points
- Return navigation: 20 points
- Landing: 30 points
- **Pass: 70/100**

**Why Discovery Flight?**
- Proves basic competency before PPL
- Quick filter for complete beginners
- Low cost, low risk entry point
- Unlocks the full exam system

### Scoring System

**Pass Threshold: 70%** (70/100 points minimum to pass)

| Score Range | Result | Outcome |
|-------------|--------|---------|
| 90-100% | Excellent | Pass + bonus (reduced renewal cost) |
| 80-89% | Good | Pass |
| 70-79% | Satisfactory | Pass (marginal) |
| 60-69% | Below Standard | Fail - minor cooldown |
| Below 60% | Unsatisfactory | Fail - longer cooldown + higher retake fee |

### Point Deductions

| Violation | Points Lost | Notes |
|-----------|-------------|-------|
| Speed >250kts below 10,000ft | -5 per occurrence | Max -20 |
| Altitude deviation >200ft | -3 per occurrence | Max -15 |
| Heading deviation >10° | -2 per occurrence | Max -10 |
| G-force >2G | -5 per occurrence | Max -15 |
| G-force >3G | -10 + exam fail | Immediate failure |
| Hard landing (>-600fpm) | -5 | Per landing |
| Very hard landing (>-900fpm) | -10 | Per landing |
| Centerline deviation >50ft | -3 | Per landing |
| Missed checkpoint | -10 | Per checkpoint |
| Time limit exceeded | -1 per minute | Max -20 |
| Crashed/gear up landing | -100 | Immediate failure |

### Failed Exam Consequences

| Failure Type | Cooldown (Real Time) | Retake Fee |
|--------------|----------------------|------------|
| First failure (60-69%) | 6 hours | 50% of original |
| First failure (<60%) | 12 hours | 75% of original |
| Second failure | 24 hours | 100% of original |
| Third+ failure | 48 hours | 150% of original |

*Cooldowns reduced to match faster game pace*

---

### Type Rating Exam Structure (Detailed)

Type rating exams follow a **Multi-Airport Circuit Pattern**:

```
EXAM FLOW:

1. DEPARTURE (Starting Airport)
   ├── Pre-flight at designated gate/ramp
   ├── Normal takeoff
   ├── Climb to assigned altitude (e.g., 3,000ft AGL)
   └── Turn to assigned heading

2. CRUISE LEG (15-30nm)
   ├── Maintain altitude ±200ft
   ├── Maintain heading ±5°
   ├── Maintain speed within limits
   └── Monitor for G-force violations

3. TOUCH-AND-GO #1 (Airport B)
   ├── Descend for pattern entry
   ├── Fly standard traffic pattern
   ├── Touch down in touchdown zone
   ├── Maintain centerline ±50ft
   └── Execute touch-and-go, climb out

4. CRUISE LEG (10-20nm)
   └── Same requirements as Leg 2

5. TOUCH-AND-GO #2 (Airport C)
   └── Same requirements as T&G #1

6. CRUISE LEG (10-20nm)
   └── Same requirements as Leg 2

7. TOUCH-AND-GO #3 (Airport D) - Optional for larger aircraft
   └── Same requirements as T&G #1

8. RETURN LEG (15-30nm)
   └── Navigate back to starting airport

9. FULL STOP LANDING (Starting Airport)
   ├── Final approach
   ├── Land and taxi to designated area
   └── Shutdown and end exam
```

### Exam Route Generation

The system automatically generates exam routes based on:
```csharp
public class ExamRouteGenerator
{
    public ExamRoute GenerateTypeRatingRoute(string startingAirport, AircraftCategory category)
    {
        // Find 3-4 airports within range
        var nearbyAirports = FindAirportsWithin(
            startingAirport,
            minDistance: 10, // nm
            maxDistance: category == AircraftCategory.Airliner ? 50 : 30,
            requiresRunwayLength: GetMinRunwayForCategory(category)
        );

        // Select diverse airports (different runway orientations)
        var selectedAirports = SelectDiverseAirports(nearbyAirports, count: 3);

        return new ExamRoute
        {
            StartAirport = startingAirport,
            Waypoints = selectedAirports,
            TotalDistance = CalculateTotalDistance(startingAirport, selectedAirports),
            AssignedAltitude = GetAltitudeForCategory(category),
            TimeLimit = GetTimeLimitForCategory(category)
        };
    }
}
```

### Altitude Assignments by Aircraft Category

| Category | Assigned Altitude | Speed Limit |
|----------|------------------|-------------|
| SEP (Single Piston) | 2,000-3,000ft AGL | 150 kts |
| MEP (Twin Piston) | 3,000-4,000ft AGL | 200 kts |
| Turboprop | 4,000-6,000ft AGL | 250 kts |
| Regional Jet | 5,000-8,000ft AGL | 250 kts below 10k |
| Narrow Body | 6,000-10,000ft AGL | 250 kts below 10k |
| Wide Body | 8,000-12,000ft AGL | 250 kts below 10k |

### G-Force Monitoring

```cpp
struct GForceMonitor {
    float currentG;
    float maxGAllowed = 2.0f;  // Normal ops
    float criticalG = 3.0f;    // Exam failure

    void update(float gForce) {
        currentG = gForce;

        if (currentG > criticalG) {
            triggerExamFailure("EXCESSIVE_G_FORCE");
        } else if (currentG > maxGAllowed) {
            recordViolation("G_FORCE_EXCEEDED", currentG);
        }
    }

    // Track sustained G
    bool isSustainedHighG(float duration) {
        return currentG > 1.5f && duration > 5.0f; // 5 seconds
    }
};
```

### Landing Quality Scoring

```cpp
struct LandingScore {
    float verticalSpeedFpm;
    float centerlineDeviationFt;
    float touchdownZoneDistanceFt;
    bool isOnRunway;

    int calculateScore() {
        int score = 20; // Base points per landing

        // Vertical speed scoring
        if (verticalSpeedFpm > -100) score += 5;      // Greaser
        else if (verticalSpeedFpm > -200) score += 3; // Smooth
        else if (verticalSpeedFpm > -400) score += 0; // Normal
        else if (verticalSpeedFpm > -600) score -= 5; // Firm
        else if (verticalSpeedFpm > -900) score -= 10; // Hard
        else score -= 20; // Very hard / potential damage

        // Centerline scoring
        if (centerlineDeviationFt < 10) score += 3;
        else if (centerlineDeviationFt < 25) score += 1;
        else if (centerlineDeviationFt < 50) score += 0;
        else score -= 3;

        // Touchdown zone
        if (touchdownZoneDistanceFt < 500) score += 2;
        else if (touchdownZoneDistanceFt < 1000) score += 0;
        else score -= 2;

        return std::max(0, score);
    }
};

### Exam Result Display

After completing an exam, the player sees a detailed scorecard:

```
╔══════════════════════════════════════════════════════════════╗
║                    TYPE RATING EXAM RESULTS                   ║
║                        Cessna 172 (C172)                      ║
╠══════════════════════════════════════════════════════════════╣
║  FINAL SCORE: 78/100                          PASSED ✓       ║
╠══════════════════════════════════════════════════════════════╣
║  FLIGHT PERFORMANCE                                          ║
║  ├── Altitude Adherence................ 18/20  (-2 deviations)║
║  ├── Heading Adherence................. 16/20  (-2 deviations)║
║  ├── Speed Compliance.................. 20/20  (no violations)║
║  └── G-Force Compliance................ 15/20  (-1 violation) ║
║                                                              ║
║  LANDINGS (3 Touch-and-Go + 1 Full Stop)                     ║
║  ├── Landing #1 (EGCC)................. 22/25  (firm, centered)║
║  ├── Landing #2 (EGGP)................. 18/25  (hard, -15ft)  ║
║  ├── Landing #3 (EGNM)................. 24/25  (smooth)       ║
║  └── Final Landing (EGLL).............. 20/25  (normal)       ║
║                                                              ║
║  TIME BONUS                                                  ║
║  └── Completed in 52 min (limit: 60)... +5 bonus             ║
╠══════════════════════════════════════════════════════════════╣
║  EXAMINER NOTES:                                             ║
║  - Good altitude control during cruise legs                  ║
║  - Landing #2 was hard (-650fpm), practice flare timing      ║
║  - Minor G-force spike during turn to final at EGNM          ║
║  - Overall satisfactory performance                          ║
╚══════════════════════════════════════════════════════════════╝
```

### Exam Entity
```csharp
public class LicenseExam
{
    public Guid Id { get; set; }
    public Guid PlayerWorldId { get; set; }
    public Guid WorldId { get; set; }
    public Guid LicenseTypeId { get; set; }

    // Exam configuration
    public ExamStatus Status { get; set; }  // Scheduled, InProgress, Passed, Failed, Abandoned
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TimeLimitMinutes { get; set; }

    // Requirements
    public string RequiredAircraftCategory { get; set; }  // "SEP", "MEP", "JET", specific type
    public string DepartureIcao { get; set; }
    public string RouteJson { get; set; }  // Generated exam route (waypoints)

    // Results
    public int Score { get; set; }  // 0-100
    public int PassingScore { get; set; }  // 70
    public string? FailureReason { get; set; }
    public string? ExaminerNotes { get; set; }  // Auto-generated feedback

    // Attempt tracking
    public int AttemptNumber { get; set; }  // 1, 2, 3...
    public decimal FeePaid { get; set; }
    public DateTime? EligibleForRetakeAt { get; set; }  // Cooldown

    // Tracking
    public ICollection<ExamManeuver> Maneuvers { get; set; }
    public ICollection<ExamCheckpoint> Checkpoints { get; set; }
    public ICollection<ExamLanding> Landings { get; set; }
    public ICollection<ExamViolation> Violations { get; set; }
}

public class ExamLanding
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public int Order { get; set; }  // 1, 2, 3, 4...
    public string AirportIcao { get; set; }
    public LandingType Type { get; set; }  // TouchAndGo, FullStop

    // Metrics
    public float VerticalSpeedFpm { get; set; }
    public float CenterlineDeviationFt { get; set; }
    public float TouchdownZoneDistanceFt { get; set; }
    public int PointsAwarded { get; set; }
    public string? Notes { get; set; }
}

public class ExamViolation
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public DateTime OccurredAt { get; set; }
    public ViolationType Type { get; set; }
    public float Value { get; set; }  // e.g., 2.3G, 280kts, etc.
    public int PointsDeducted { get; set; }
    public double LatitudeAtViolation { get; set; }
    public double LongitudeAtViolation { get; set; }
}

public enum ExamStatus { Scheduled, InProgress, Passed, Failed, Abandoned, Expired }
public enum LandingType { TouchAndGo, FullStop }
public enum ViolationType { SpeedExcess, AltitudeDeviation, HeadingDeviation, GForceExcess, HardLanding, CenterlineDeviation }
```

### Exam Maneuvers (Graded by Connector)
```csharp
public class ExamManeuver
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }

    public string ManeuverType { get; set; }  // "Takeoff", "Landing", "SteepTurn", "Stall", etc.
    public int Order { get; set; }
    public bool IsRequired { get; set; }

    // Scoring
    public int MaxPoints { get; set; }
    public int PointsAwarded { get; set; }
    public ManeuverResult Result { get; set; }  // Pass, Fail, NotAttempted

    // Tolerances
    public int? AltitudeToleranceFt { get; set; }
    public int? HeadingToleranceDeg { get; set; }
    public int? SpeedToleranceKts { get; set; }

    // Actual performance
    public int? AltitudeDeviationFt { get; set; }
    public int? HeadingDeviationDeg { get; set; }
    public int? SpeedDeviationKts { get; set; }
    public string? Notes { get; set; }
}
```

### Exam Flow
```
1. Web App: Player requests exam
   - Check prerequisites (licenses)
   - Pay exam fee
   - Select exam location (airport)
   - Schedule exam (immediate or future)

2. Connector: Player starts exam
   - Load exam requirements
   - Display exam briefing
   - Player spawns at exam airport

3. Connector: Track exam flight
   - Monitor all maneuvers
   - Grade each in real-time
   - Track checkpoints for nav exams
   - Enforce time limits

4. Connector: Complete exam
   - Calculate final score
   - Send results to API
   - API awards license if passed
   - Display debrief with feedback
```

### Maneuver Detection (Connector Logic)
```cpp
// Connector tracks and detects maneuvers:
struct ManeuverDetector {
    // Takeoff: Ground -> Airborne transition
    // Landing: Airborne -> Ground at airport
    // Steep Turn: Bank > 45° for > 360°
    // Stall: Airspeed drops below Vs, recovery
    // Engine Out: Engine failure simulation
    // Go-Around: Approach followed by climb
    // Holding: Racetrack pattern at fix
    // ILS Approach: GS/LOC tracking to minimums
};
```

### Exam Fees (Base, × World Modifier)
| Exam | Fee | Retake (First) | Retake (Second+) |
|------|-----|----------------|------------------|
| Discovery Flight | $500 | $250 | $375 |
| PPL | $2,000 | $1,000 | $1,500-$3,000 |
| CPL | $5,000 | $2,500 | $3,750-$7,500 |
| ATPL | $10,000 | $5,000 | $7,500-$15,000 |
| Night Rating | $3,000 | $1,500 | $2,250-$4,500 |
| IR | $5,000 | $2,500 | $3,750-$7,500 |
| MEP | $4,000 | $2,000 | $3,000-$6,000 |
| Type Rating | $5,000-$20,000 | 50% | 75%-150% |

### New Player Progression Cost Summary

**Minimum Cost to CPL (Fast Track):**
```
Discovery Flight:  $500
PPL Exam:          $2,000
CPL Exam:          $5,000
────────────────────────
Total:             $7,500 (assuming first-time passes)
```

**Starting Capital vs Requirements:**
- Starting Capital (Medium): $50,000
- Cost to CPL: $7,500
- Remaining after CPL: $42,500 (plenty for first aircraft down payment or rental jobs)

---

## IAM (Identity & Access Management)

### Overview
Role-based access control system to manage permissions across the platform.

### Role Hierarchy
```
SuperAdmin (Platform owner)
    └── Admin (Full world management)
        └── Moderator (Limited moderation)
            └── Support (Read-only + limited actions)
                └── Player (Standard user)
                    └── Restricted (Suspended/limited)
```

### Role Definitions
| Role | Scope | Description |
|------|-------|-------------|
| SuperAdmin | Global | Platform-wide, all permissions, IAM management |
| Admin | Per-World | Full control of a specific world |
| Moderator | Per-World | Moderate players, manage content |
| Support | Per-World | View data, respond to tickets |
| Player | Per-World | Standard gameplay permissions |
| Restricted | Per-World | Limited access (suspension) |

### Permission Categories
```csharp
public enum PermissionCategory
{
    // User Management
    Users_View,
    Users_Edit,
    Users_Ban,
    Users_Delete,

    // World Management
    Worlds_View,
    Worlds_Edit,
    Worlds_Create,
    Worlds_Delete,
    Worlds_Settings,

    // Economy
    Economy_View,
    Economy_Adjust,      // Modify balances, credits
    Economy_Audit,       // View transaction logs

    // Jobs
    Jobs_View,
    Jobs_Create,
    Jobs_Edit,
    Jobs_Delete,
    Jobs_ForceComplete,

    // Aircraft & Marketplace
    Aircraft_View,
    Aircraft_Spawn,      // Give aircraft to players
    Marketplace_Manage,
    Auctions_Moderate,

    // Licenses
    Licenses_View,
    Licenses_Grant,
    Licenses_Revoke,
    Exams_Override,

    // Moderation
    Chat_Moderate,
    Reports_View,
    Reports_Resolve,
    Violations_Issue,
    Violations_Clear,

    // System
    Settings_View,
    Settings_Edit,
    Logs_View,
    Analytics_View,
    IAM_Manage,          // SuperAdmin only
}
```

### IAM Entities
```csharp
public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; }           // "Admin", "Moderator", etc.
    public string Description { get; set; }
    public int Priority { get; set; }          // Higher = more authority
    public bool IsSystemRole { get; set; }     // Cannot be deleted
    public bool IsGlobal { get; set; }         // Applies to all worlds

    public ICollection<RolePermission> Permissions { get; set; }
}

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; }
    public PermissionCategory Permission { get; set; }
    public bool IsGranted { get; set; }        // Allow or deny
}

public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid RoleId { get; set; }
    public Role Role { get; set; }

    // Scope (null = global, set = specific world)
    public Guid? WorldId { get; set; }
    public World? World { get; set; }

    public DateTime GrantedAt { get; set; }
    public Guid GrantedByUserId { get; set; }
    public DateTime? ExpiresAt { get; set; }   // Temporary roles
}
```

### Permission Checking
```csharp
public interface IAuthorizationService
{
    // Check if user has permission (considers role hierarchy)
    Task<bool> HasPermissionAsync(Guid userId, PermissionCategory permission, Guid? worldId = null);

    // Get user's effective permissions for a world
    Task<IEnumerable<PermissionCategory>> GetPermissionsAsync(Guid userId, Guid? worldId = null);

    // Check if user can perform action on resource
    Task<bool> CanAccessResourceAsync(Guid userId, string resourceType, Guid resourceId, string action);
}

// Usage in controllers:
[HttpPost("worlds/{worldId}/settings")]
public async Task<IActionResult> UpdateWorldSettings(Guid worldId, WorldSettingsDto dto)
{
    if (!await _auth.HasPermissionAsync(UserId, PermissionCategory.Worlds_Settings, worldId))
        return Forbid();

    // ... update settings
}
```

### World Settings (Admin Adjustable)
```csharp
public class WorldSettings
{
    public Guid WorldId { get; set; }

    // All the modifiers from World entity, but editable:
    public decimal JobPayoutMultiplier { get; set; }
    public decimal AircraftPriceMultiplier { get; set; }
    public decimal MaintenanceCostMultiplier { get; set; }
    // ... etc

    // Additional settings
    public bool AllowNewPlayers { get; set; }
    public bool AllowIllegalCargo { get; set; }
    public bool EnableAuctions { get; set; }
    public bool EnableAICrews { get; set; }
    public int MaxAircraftPerPlayer { get; set; }
    public int MaxLoansPerPlayer { get; set; }
    public int MaxWorkersPerPlayer { get; set; }

    // Moderation
    public bool RequireApprovalToJoin { get; set; }
    public bool EnableChat { get; set; }
    public bool EnableReporting { get; set; }

    public DateTime LastModifiedAt { get; set; }
    public Guid LastModifiedByUserId { get; set; }
}
```

### Default World: "Global"
```csharp
// Seeded on first run
new World
{
    Name = "Global",
    Slug = "global",
    Difficulty = WorldDifficulty.Medium,
    IsDefault = true,
    IsActive = true,
    StartingCapital = 50000,
    JobPayoutMultiplier = 1.0m,
    AircraftPriceMultiplier = 1.0m,
    // ... standard multipliers
}
```

---

## Audit Logging System

### Overview
Every action that affects the economy, players, or game state is logged for accountability and investigation. Staff can view, filter, and export audit logs via the web app.

### What Gets Audited

**Player Actions:**
| Category | Events Logged |
|----------|---------------|
| Jobs | Accept, complete, fail, abandon, payout |
| Flights | Start, complete, violations detected |
| Aircraft | Purchase, sell, rent out, maintenance |
| Fuel | Purchase, auto-fill requests |
| Exams | Start, complete, pass, fail, score |
| Licenses | Acquire, renew, expire, revoke |
| Banking | Loan apply, payment, default, credit score change |
| Auctions | List, bid, win, cancel |
| Transport | Quick transport requests |

**Economy Events:**
| Category | Events Logged |
|----------|---------------|
| Balance Changes | Every credit/debit with reason and source |
| Airport Revenue | Landing fees, fuel sales, parking |
| Rental Income | Aircraft rental payouts |
| Market Transactions | Aircraft sales, auction completions |
| Loan Processing | Payments, interest, defaults |

**Staff Actions (Always Logged):**
| Category | Events Logged |
|----------|---------------|
| Player Modifications | Balance adjust, license grant/revoke, ban/unban |
| World Settings | Any multiplier or setting change |
| Airport Overrides | Fee changes, ownership transfers |
| Economy Adjustments | Item prices, job payouts, fuel prices |
| IAM Changes | Role grants, permission changes |

**System Events:**
| Category | Events Logged |
|----------|---------------|
| Job Generation | New jobs created, expired jobs removed |
| Fuel Restocking | Airport fuel refills |
| License Expiry | Auto-expired licenses |
| Loan Processing | Scheduled payments, auto-defaults |
| Maintenance | Scheduled aircraft maintenance |

### Audit Log Entity

```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }

    // Classification
    public AuditCategory Category { get; set; }
    public string Action { get; set; }           // "BALANCE_CREDIT", "LICENSE_GRANT", etc.
    public AuditSeverity Severity { get; set; }  // Info, Warning, Critical

    // Who performed the action
    public Guid? ActorUserId { get; set; }       // null = system
    public string ActorUsername { get; set; }
    public string ActorRole { get; set; }        // "Player", "Admin", "System"
    public string ActorIpAddress { get; set; }

    // What was affected
    public string TargetType { get; set; }       // "Player", "Airport", "Aircraft", etc.
    public Guid? TargetId { get; set; }
    public string TargetIdentifier { get; set; } // Username, ICAO, registration, etc.

    // Change details
    public string Description { get; set; }      // Human-readable description
    public string OldValue { get; set; }         // JSON of previous state
    public string NewValue { get; set; }         // JSON of new state
    public decimal? AmountChanged { get; set; }  // For financial transactions

    // Context
    public string RelatedEntityType { get; set; } // e.g., "Job" for job completion
    public Guid? RelatedEntityId { get; set; }
    public string Metadata { get; set; }         // Additional JSON context

    public DateTime Timestamp { get; set; }
}

public enum AuditCategory
{
    // Player actions
    PlayerJob,
    PlayerFlight,
    PlayerAircraft,
    PlayerFuel,
    PlayerExam,
    PlayerLicense,
    PlayerBanking,
    PlayerAuction,
    PlayerTransport,

    // Economy
    EconomyBalance,
    EconomyAirportRevenue,
    EconomyRental,
    EconomyMarket,
    EconomyLoan,

    // Staff actions
    StaffPlayerEdit,
    StaffWorldSettings,
    StaffAirportEdit,
    StaffEconomyAdjust,
    StaffIAM,

    // System
    SystemJobGeneration,
    SystemFuelRestock,
    SystemLicenseExpiry,
    SystemLoanProcess,
    SystemMaintenance
}

public enum AuditSeverity
{
    Info,      // Normal operations
    Warning,   // Unusual but valid (large transaction, multiple failures)
    Critical   // Requires attention (bans, large adjustments, system errors)
}
```

### Audit Log Examples

**Player Balance Change:**
```json
{
  "id": "...",
  "category": "EconomyBalance",
  "action": "BALANCE_CREDIT",
  "severity": "Info",
  "actorUserId": null,
  "actorRole": "System",
  "targetType": "Player",
  "targetIdentifier": "SkyPilot_John",
  "description": "Job completion payout",
  "oldValue": "{\"balance\": 45000}",
  "newValue": "{\"balance\": 57000}",
  "amountChanged": 12000,
  "relatedEntityType": "Job",
  "relatedEntityId": "job-guid-here",
  "timestamp": "2024-01-15T14:32:00Z"
}
```

**Admin Adjusts Player Balance:**
```json
{
  "id": "...",
  "category": "StaffPlayerEdit",
  "action": "BALANCE_ADJUST",
  "severity": "Warning",
  "actorUserId": "admin-guid",
  "actorUsername": "Admin_Mike",
  "actorRole": "Admin",
  "actorIpAddress": "192.168.1.100",
  "targetType": "Player",
  "targetIdentifier": "SkyPilot_John",
  "description": "Compensation for bug - lost cargo payout",
  "oldValue": "{\"balance\": 57000}",
  "newValue": "{\"balance\": 67000}",
  "amountChanged": 10000,
  "metadata": "{\"reason\": \"Bug compensation\", \"ticketId\": \"TICKET-1234\"}",
  "timestamp": "2024-01-15T15:00:00Z"
}
```

**World Setting Change:**
```json
{
  "id": "...",
  "category": "StaffWorldSettings",
  "action": "SETTING_CHANGE",
  "severity": "Critical",
  "actorUserId": "admin-guid",
  "actorUsername": "Admin_Mike",
  "actorRole": "Admin",
  "targetType": "World",
  "targetIdentifier": "Global",
  "description": "Changed job payout multiplier",
  "oldValue": "{\"jobPayoutMultiplier\": 1.0}",
  "newValue": "{\"jobPayoutMultiplier\": 1.2}",
  "timestamp": "2024-01-15T16:00:00Z"
}
```

### Staff Audit Log Access by Role

| Role | View Logs | Filter By | Export | See IP Addresses |
|------|-----------|-----------|--------|------------------|
| SuperAdmin | All | Everything | Yes | Yes |
| Admin | World-specific | Everything in world | Yes | Yes |
| Moderator | World-specific | Players, Economy | Yes | No |
| Support | World-specific | Players only (read) | No | No |

### Audit Dashboard (Web App)

```
┌─────────────────────────────────────────────────────────────────────────┐
│  AUDIT LOG VIEWER                                        [Export CSV]   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  FILTERS:                                                               │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────────┐   │
│  │ Category ▼  │ │ Severity ▼  │ │ Actor ▼     │ │ Date Range      │   │
│  │ All         │ │ All         │ │ All         │ │ Last 7 days   ▼ │   │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────────┘   │
│                                                                         │
│  ┌───────────────┐                                                      │
│  │ Search: player, airport, action...                              │   │
│  └───────────────┘                                                      │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│  TIME          │ CATEGORY      │ ACTION           │ DETAILS            │
├─────────────────────────────────────────────────────────────────────────┤
│  14:32:00      │ EconomyBalance│ BALANCE_CREDIT   │ SkyPilot_John      │
│  [Info]        │               │                  │ +$12,000 (Job)     │
├─────────────────────────────────────────────────────────────────────────┤
│  15:00:00      │ StaffPlayerEdit│ BALANCE_ADJUST  │ SkyPilot_John      │
│  [Warning]     │               │ by Admin_Mike    │ +$10,000 (Bug fix) │
├─────────────────────────────────────────────────────────────────────────┤
│  15:45:00      │ PlayerLicense │ LICENSE_EXPIRE   │ FlyHigh_Sarah      │
│  [Info]        │               │                  │ CPL expired        │
├─────────────────────────────────────────────────────────────────────────┤
│  16:00:00      │ StaffWorldSettings│ SETTING_CHANGE│ World: Global     │
│  [Critical]    │               │ by Admin_Mike    │ JobPayout 1.0→1.2  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Showing 1-50 of 2,847 entries                    [< Prev] [Next >]    │
└─────────────────────────────────────────────────────────────────────────┘
```

### Player-Specific Audit View

Staff can view a player's complete audit history:

```
┌─────────────────────────────────────────────────────────────────────────┐
│  PLAYER AUDIT: SkyPilot_John                                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  SUMMARY (Last 30 days):                                                │
│  ├── Jobs completed: 47                                                 │
│  ├── Total earned: $412,000                                             │
│  ├── Total spent: $385,000                                              │
│  ├── Exams taken: 3 (2 passed, 1 failed)                                │
│  ├── Violations: 2 (speed)                                              │
│  └── Staff interactions: 1 (balance adjustment)                         │
│                                                                         │
│  RECENT ACTIVITY:                                                       │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ 14:32 │ Job completed │ EGLL→EGCC │ +$12,000                    │    │
│  │ 13:15 │ Fuel purchase │ EGLL │ -$283 (42 gal AVGAS)             │    │
│  │ 12:00 │ Job accepted │ Cargo to EGCC │ ID: JOB-4521             │    │
│  │ 11:30 │ Landing fee │ EGLL │ -$50 (owner: System)               │    │
│  │ 10:45 │ Flight complete │ LFPG→EGLL │ 1.2hrs, 180nm             │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  [View Full History]  [Export]  [Flag for Review]                       │
└─────────────────────────────────────────────────────────────────────────┘
```

### Audit Retention Policy

| Log Type | Retention Period |
|----------|------------------|
| Player actions | 90 days (archive after) |
| Economy transactions | 1 year |
| Staff actions | 2 years (compliance) |
| System events | 30 days |
| Critical severity | 2 years |

### Automatic Alerts

Certain events trigger automatic alerts to staff:

| Trigger | Alert To | Severity |
|---------|----------|----------|
| Balance change > $1M | Admin | Warning |
| Multiple exam failures (5+) | Moderator | Info |
| Unusual login pattern | Support | Warning |
| Staff action on own account | SuperAdmin | Critical |
| World setting change | All Admins | Critical |
| Mass ban (5+ players) | SuperAdmin | Critical |

### Audit Service

```csharp
public interface IAuditService
{
    // Log an action
    Task LogAsync(AuditLog entry);

    // Convenience methods
    Task LogPlayerActionAsync(Guid playerId, string action, object oldValue, object newValue, decimal? amount = null);
    Task LogStaffActionAsync(Guid staffId, string action, string targetType, Guid targetId, object oldValue, object newValue);
    Task LogSystemEventAsync(string action, string description, object metadata = null);

    // Query
    Task<PagedResult<AuditLog>> SearchAsync(AuditSearchCriteria criteria);
    Task<IEnumerable<AuditLog>> GetPlayerHistoryAsync(Guid playerId, int days = 30);
    Task<IEnumerable<AuditLog>> GetStaffActionsAsync(Guid staffId, int days = 30);

    // Export
    Task<byte[]> ExportToCsvAsync(AuditSearchCriteria criteria);
}

// Usage example - automatically audit balance changes
public class PlayerBalanceService
{
    private readonly IAuditService _audit;

    public async Task AdjustBalanceAsync(Guid playerId, decimal amount, string reason, Guid? staffId = null)
    {
        var player = await _context.PlayerWorlds.FindAsync(playerId);
        var oldBalance = player.Balance;

        player.Balance += amount;
        await _context.SaveChangesAsync();

        // Audit the change
        await _audit.LogAsync(new AuditLog
        {
            Category = staffId.HasValue ? AuditCategory.StaffPlayerEdit : AuditCategory.EconomyBalance,
            Action = amount > 0 ? "BALANCE_CREDIT" : "BALANCE_DEBIT",
            Severity = Math.Abs(amount) > 100000 ? AuditSeverity.Warning : AuditSeverity.Info,
            ActorUserId = staffId,
            ActorRole = staffId.HasValue ? "Staff" : "System",
            TargetType = "Player",
            TargetId = playerId,
            Description = reason,
            OldValue = JsonSerializer.Serialize(new { balance = oldBalance }),
            NewValue = JsonSerializer.Serialize(new { balance = player.Balance }),
            AmountChanged = amount
        });
    }
}
```

---

## Game Time Conversion

**Critical Design Decision: 1 Game Year = 31 Real Days**

| Game Time | Real Time | Notes |
|-----------|-----------|-------|
| 1 game week | ~15 hours | ~0.6 real days |
| 1 game month | ~2.5 real days | 31÷12 = 2.58 days |
| 1 game quarter | ~7.75 real days | About 1 real week |
| 1 game year | 31 real days | About 1 real month |

**Conversion Formula:**
```
Real Days = Game Months × 2.58
Game Months = Real Days ÷ 2.58
```

All time-based systems use game time unless explicitly stated as "real time".

**Why This Pace?**
- Players experience a full game year in ~1 real month
- License renewals feel meaningful (every few real days)
- Loan payments are frequent but manageable
- Economy cycles feel dynamic without being overwhelming

### Time Impact Summary

| System | Game Time | Real Time |
|--------|-----------|-----------|
| License Validity (PPL, Night, MEP) | 6 months | ~15 days |
| License Validity (CPL, ATPL, IR) | 3 months | ~7.5 days |
| Loan Payment Due | Monthly | Every ~2.5 days |
| Loan Term (Aircraft) | 6-12 months | 15-31 days |
| Worker Salaries | Monthly | Every ~2.5 days |
| Airport Operating Costs | Monthly | Every ~2.5 days |
| Credit Score Recovery | +1/month | +1 per ~2.5 days |
| Violation Point Decay | 1/month | 1 per ~2.5 days |
| Exam Cooldown (First Fail) | - | 6-12 hours |
| Exam Cooldown (Multiple Fails) | - | 24-48 hours |

---

## Economy Scale

### Progression Philosophy
**Money + Skill = Access** — No artificial hour gates.

- Can afford the exam fee? Take the exam.
- Pass the practical test? Earn the license.
- Have the license + money? Buy the aircraft.
- Have the required licenses? Accept the job.

Hours flown are tracked for statistics and reputation display, but they NEVER gate content. A skilled player can theoretically go from SPL to ATPL in a single session if they have the money and can pass each exam.

### Starting Conditions
- **Starting Capital**: $50,000 (× world modifier)
- **Credit Score**: 650 (neutral)
- **Licenses**: None (must earn SPL first)
- **Aircraft**: None (rent or buy)

### Earnings by Aircraft Class

| Aircraft Class | Example | Purchase Price | Cargo/Flight | Passenger/Flight |
|----------------|---------|----------------|--------------|------------------|
| Light Single | C172 | $350,000 | $8,000-$12,000 | $4,000-$6,000 |
| Light Twin | C421C | $800,000-$1.5M | $40,000-$60,000 | $20,000-$35,000 |
| Turboprop Single | TBM 930 | $4M | $80,000-$120,000 | $50,000-$80,000 |
| Turboprop Twin | King Air 350 | $8M | $150,000-$250,000 | $100,000-$150,000 |
| Regional Jet | CRJ-700 | $25M | $400,000-$600,000 | $250,000-$400,000 |
| Narrow Body | A320 | $98M | $1.5M-$2.5M | $800,000-$1.2M |
| Wide Body | B777 | $350M | $5M-$8M | $3M-$5M |

### Aircraft Base Price Formula
```
BASE_PRICE = (
    (MTOW_kg × $400) +
    (CRUISE_TAS_kts × $40,000) +
    (RANGE_nm × $800) +
    (PASSENGER_CAPACITY × $80,000) +
    (ENGINE_COUNT × ENGINE_FACTOR)
) × CATEGORY_MULTIPLIER × MARKET_ADJUSTMENT

ENGINE_FACTOR: Piston($200k), Turboprop($1.5M), Turbofan($5M)
CATEGORY_MULTIPLIER: Single Piston(0.25), Twin Piston(0.40), Turboprop(0.85), Narrow Body(2.0), Wide Body(3.0)
```

---

## Phase 1: Core Database Entities

### Global Entities (Shared)

1. **World** - World definitions with difficulty modifiers
2. **AircraftTemplate** - Master aircraft data from Eurocontrol (GLOBAL)
3. **CargoCategory** - Top-level cargo categories (GLOBAL)
4. **CargoSubcategory** - Cargo subcategories (GLOBAL)
5. **CargoType** - Specific items with base pricing (GLOBAL)
6. **LicenseType** - License definitions with base costs (GLOBAL)
7. **Bank** - Bank definitions with base rates (GLOBAL)

### Per-World Entities (All have WorldId)

**Player State:**
8. **PlayerWorld** - Player's state in a world (balance, hours, reputation, credit score)

**Aircraft System:**
9. **OwnedAircraft** - Player-owned instances (WorldId + PlayerWorldId)
10. **AircraftComponent** - Engine1-6, Wings, Gear, Fuselage
11. **AircraftModification** - Cargo conversions (25%, 50%, 75%, 100%)
12. **MaintenanceLog** - Repair and service history

**Job System:**
13. **Job** - Dynamic cargo/passenger jobs with WorldId
    - Origin/destination airports
    - Cargo type, weight, volume
    - Expiry time (1-48 hours × world modifier)
    - Risk level (1-5 stars)
    - Base payout (× world modifier)
    - Status: Available → Accepted → InTransit → Delivered

14. **FlightJob** - Junction table for multi-job flights

**License System:**
15. **UserLicense** - Player's licenses in this world (cost × world modifier)

**Banking System:**
16. **Loan** - Active loans (interest × world modifier)
17. **LoanPayment** - Payment history
18. **CreditScoreEvent** - Credit score changes

**Marketplace System:**
19. **AircraftDealer** - Dealers at airports in this world
20. **DealerInventory** - Aircraft for sale (price × world modifier)
21. **DealerDiscount** - Active sales and promotions

**Auction System:**
22. **Auction** - Player aircraft listings in this world
23. **AuctionBid** - Bids with escrow tracking

**Worker System:**
24. **Worker** - Hired employees (salary × world modifier)

**Risk System:**
25. **InspectionEvent** - Inspection records (detection × world modifier)
26. **ViolationRecord** - Violations (fines × world modifier)

**Flight Tracking:**
27. **TrackedFlight** - Real flight data from SimConnect
28. **FlightFinancials** - Revenue/costs with world modifiers applied

---

## Phase 2: Banking System (Game Time Adjusted)

### Banks
| Bank | Interest/Month | Min Credit | Down Payment | Max Loan |
|------|---------------|------------|--------------|----------|
| First National | 1-2% | 750+ | 25% | $50M |
| Skyline Commercial | 2-3% | 650+ | 15% | $100M |
| Rapid Aviation | 3-5% | 550+ | 10% | $25M |
| Highwing Capital | 5-8% | 400+ | 5% | $10M |

### Loan Terms (Game Months → Real Days)
| Term Type | Game Months | Real Days |
|-----------|-------------|-----------|
| Short-term | 1-3 months | 2.5-7.5 days |
| Standard | 3-6 months | 7.5-15 days |
| Aircraft | 6-12 months | 15-31 days |
| Major | 12-24 months | 31-62 days |

### Credit Score (300-850)
- Starting: 650
- On-time payment: +5 (max +50/month)
- Late payment: -25 to -100
- Default: -200
- Recovery: +1 per game month (~2.5 real days) with no violations

### Payment Schedule
- Due: Every game month (~2.5 real days)
- Grace period: 12 real hours
- Late: Hours 12-24 (fee + credit hit)
- Very late: Hours 24-48 (major credit hit)
- Default: 48+ hours past due (asset seizure begins)

---

## Phase 3: License System (Game Time Adjusted)

### License Validity & Costs (Updated for Fast Progression)

**Core Licenses (Exam Required):**
| License | Exam Cost | Validity (Game) | Validity (Real) | Renewal |
|---------|-----------|-----------------|-----------------|---------|
| Discovery | $500 | Permanent | Permanent | N/A |
| PPL | $2,000 | 6 game months | ~15 real days | $500 |
| CPL | $5,000 | 3 game months | ~7.5 real days | $1,500 |
| ATPL | $10,000 | 3 game months | ~7.5 real days | $3,000 |

**Endorsements (Exam Required):**
| Rating | Exam Cost | Validity (Game) | Validity (Real) | Renewal |
|--------|-----------|-----------------|-----------------|---------|
| Night Rating | $3,000 | 6 game months | ~15 real days | $1,000 |
| IR (Instrument) | $5,000 | 3 game months | ~7.5 real days | $1,500 |
| MEP (Multi-Engine) | $4,000 | 6 game months | ~15 real days | $1,200 |
| Type Ratings | $5k-$20k | 3 game months | ~7.5 real days | 25% of cost |
| DG (Dangerous Goods) | $2,000 | 6 game months | ~15 real days | $600 |

### Fast Track Summary

**Time to CPL (Skilled Player):**
```
Discovery Flight:  10 min  │  $500
PPL Exam:          25 min  │  $2,000
CPL Exam:          30 min  │  $5,000
──────────────────────────────────────
Total:             65 min  │  $7,500
```

**After CPL, player can:**
- Take commercial jobs (with Quick Job rentals)
- Start earning $8,000-$12,000+ per flight
- Save for first aircraft purchase
- Add endorsements (Night, IR, MEP) as needed

### License Shop Features
- View all available licenses with prerequisites
- **Qualification status** shown (Discovery ✓, PPL exam unlocked, etc.)
- Real-time exam scheduling
- Renewal reminders (1 game week / ~15 real hours before expiry)
- Bulk discounts for multiple ratings (10% off 2+, 20% off 4+)

---

## Phase 4: Job Generation System

### Overview

The job generation system creates dynamic, realistic cargo and passenger jobs that spawn at airports based on multiple factors. Jobs provide variety through distance categories, cargo types, urgency levels, and route characteristics.

### Airport Classification

Every airport is classified to determine what jobs can spawn there:

```csharp
public enum AirportSize
{
    Major,      // EGLL, KJFK, KLAX - International hubs
    Large,      // EGCC, KORD, LFPG - Major domestic/regional hubs
    Medium,     // EGGP, KBOS, LFBO - Regional airports
    Small,      // EGNH, K1G4 - Small GA/regional
    Tiny,       // Grass strips, small islands
    Remote      // Bush strips, mountain airports, extreme locations
}

public enum AirportType
{
    Hub,            // Major passenger/cargo hub
    Cargo,          // Cargo-focused (KCVG, KLCK)
    Regional,       // Regional passenger service
    GeneralAviation,// GA-focused, flight schools
    Tourist,        // Resort destinations
    Industrial,     // Near factories, oil fields
    Island,         // Island destinations
    Bush,           // Remote bush/mountain
    Military        // Limited civilian access
}

public class AirportJobProfile
{
    public string Icao { get; set; }
    public AirportSize Size { get; set; }
    public AirportType PrimaryType { get; set; }
    public AirportType? SecondaryType { get; set; }

    public int BaseJobCount { get; set; }          // Jobs to maintain
    public double CargoRatio { get; set; }         // 0-1, cargo vs passenger
    public double InternationalRatio { get; set; } // 0-1, international routes
    public string[] PreferredDestinations { get; set; } // Common routes
    public string Region { get; set; }             // Geographic region
    public double EconomicActivity { get; set; }   // 0.5-2.0 multiplier
}
```

### Airport Size → Job Capacity

| Airport Size | Base Jobs | Refresh Rate | Max Distance | Job Variety |
|--------------|-----------|--------------|--------------|-------------|
| Major | 100-200 | Every 30 min | Unlimited | All categories |
| Large | 50-100 | Every 1 hour | 3000nm | Most categories |
| Medium | 25-50 | Every 2 hours | 1500nm | Regional + some long |
| Small | 10-25 | Every 4 hours | 500nm | Short/medium only |
| Tiny | 3-10 | Every 6 hours | 150nm | Very short/short |
| Remote | 1-5 | Every 8 hours | 300nm | Supply runs, medical |

### Distance Categories

Jobs are categorized by flight distance to ensure variety:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        DISTANCE DISTRIBUTION                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  VERY SHORT (0-50nm)     ████████████████████████  35%                  │
│  Training, island hop, urgent medical, local courier                     │
│                                                                          │
│  SHORT (50-150nm)        ███████████████████       28%                  │
│  Regional cargo, business passengers, mail                               │
│                                                                          │
│  MEDIUM (150-500nm)      ███████████████          22%                   │
│  General cargo, charter flights, livestock                               │
│                                                                          │
│  LONG (500-1500nm)       ████████                 12%                   │
│  High-value cargo, international, time-sensitive                         │
│                                                                          │
│  ULTRA LONG (1500nm+)    ███                       3%                   │
│  Intercontinental, airline contracts, specialty                          │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Distance Category Details

| Category | Range | Typical Aircraft | Base Payout/nm | Availability |
|----------|-------|------------------|----------------|--------------|
| Very Short | 0-50nm | C172, PA-28, C208 | $15-25 | Very common |
| Short | 50-150nm | C208, King Air, PC-12 | $12-20 | Common |
| Medium | 150-500nm | King Air, CRJ, E-Jets | $10-18 | Moderate |
| Long | 500-1500nm | A320, B737, B757 | $8-15 | Uncommon |
| Ultra Long | 1500nm+ | B777, A350, B747 | $6-12 | Rare |

### Job Generation Algorithm

```csharp
public class JobGenerationService
{
    /// <summary>
    /// Generates jobs for an airport based on its profile and current demand.
    /// Called periodically by background service.
    /// </summary>
    public async Task<List<Job>> GenerateJobsForAirportAsync(
        string icao,
        Guid worldId,
        CancellationToken ct = default)
    {
        var profile = await _airportService.GetJobProfileAsync(icao, ct);
        var currentJobs = await _jobRepository.CountActiveJobsAsync(icao, worldId, ct);
        var jobsToGenerate = Math.Max(0, profile.BaseJobCount - currentJobs);

        var jobs = new List<Job>();

        for (int i = 0; i < jobsToGenerate; i++)
        {
            // 1. Select distance category (weighted random)
            var distanceCategory = SelectDistanceCategory(profile);

            // 2. Find suitable destination
            var destination = await FindDestinationAsync(icao, distanceCategory, profile, ct);
            if (destination == null) continue;

            // 3. Determine job type (cargo vs passenger)
            var isCargo = _random.NextDouble() < profile.CargoRatio;

            // 4. Generate job details
            var job = isCargo
                ? GenerateCargoJob(icao, destination, distanceCategory, profile)
                : GeneratePassengerJob(icao, destination, distanceCategory, profile);

            // 5. Apply world modifiers
            job.Payout *= _worldService.GetPayoutMultiplier(worldId);
            job.ExpiresAt = CalculateExpiry(job.Urgency, worldId);

            jobs.Add(job);
        }

        return jobs;
    }

    private DistanceCategory SelectDistanceCategory(AirportJobProfile profile)
    {
        // Weighted selection based on airport size
        var weights = GetDistanceWeights(profile.Size);
        var roll = _random.NextDouble();
        var cumulative = 0.0;

        foreach (var (category, weight) in weights)
        {
            cumulative += weight;
            if (roll < cumulative)
                return category;
        }

        return DistanceCategory.Short;
    }

    private Dictionary<DistanceCategory, double> GetDistanceWeights(AirportSize size)
    {
        return size switch
        {
            AirportSize.Major => new()
            {
                [DistanceCategory.VeryShort] = 0.15,
                [DistanceCategory.Short] = 0.20,
                [DistanceCategory.Medium] = 0.30,
                [DistanceCategory.Long] = 0.25,
                [DistanceCategory.UltraLong] = 0.10
            },
            AirportSize.Large => new()
            {
                [DistanceCategory.VeryShort] = 0.20,
                [DistanceCategory.Short] = 0.25,
                [DistanceCategory.Medium] = 0.30,
                [DistanceCategory.Long] = 0.20,
                [DistanceCategory.UltraLong] = 0.05
            },
            AirportSize.Medium => new()
            {
                [DistanceCategory.VeryShort] = 0.30,
                [DistanceCategory.Short] = 0.35,
                [DistanceCategory.Medium] = 0.25,
                [DistanceCategory.Long] = 0.10,
                [DistanceCategory.UltraLong] = 0.00
            },
            AirportSize.Small => new()
            {
                [DistanceCategory.VeryShort] = 0.45,
                [DistanceCategory.Short] = 0.40,
                [DistanceCategory.Medium] = 0.15,
                [DistanceCategory.Long] = 0.00,
                [DistanceCategory.UltraLong] = 0.00
            },
            _ => new() // Tiny, Remote
            {
                [DistanceCategory.VeryShort] = 0.60,
                [DistanceCategory.Short] = 0.35,
                [DistanceCategory.Medium] = 0.05,
                [DistanceCategory.Long] = 0.00,
                [DistanceCategory.UltraLong] = 0.00
            }
        };
    }
}
```

### Destination Selection Algorithm

```csharp
public class DestinationSelector
{
    /// <summary>
    /// Finds a suitable destination airport for a job based on distance category
    /// and route characteristics.
    /// </summary>
    public async Task<string?> FindDestinationAsync(
        string origin,
        DistanceCategory distanceCategory,
        AirportJobProfile originProfile,
        CancellationToken ct)
    {
        var (minNm, maxNm) = GetDistanceRange(distanceCategory);

        // 1. Get candidate airports within distance range
        var candidates = await _airportRepository.GetAirportsInRangeAsync(
            origin, minNm, maxNm, ct);

        if (!candidates.Any())
            return null;

        // 2. Score and weight candidates
        var scoredCandidates = candidates
            .Select(dest => new
            {
                Icao = dest.Icao,
                Score = CalculateRouteScore(originProfile, dest, distanceCategory)
            })
            .Where(x => x.Score > 0)
            .ToList();

        // 3. Weighted random selection
        var totalScore = scoredCandidates.Sum(x => x.Score);
        var roll = _random.NextDouble() * totalScore;
        var cumulative = 0.0;

        foreach (var candidate in scoredCandidates)
        {
            cumulative += candidate.Score;
            if (roll < cumulative)
                return candidate.Icao;
        }

        return scoredCandidates.FirstOrDefault()?.Icao;
    }

    private (int min, int max) GetDistanceRange(DistanceCategory category)
    {
        return category switch
        {
            DistanceCategory.VeryShort => (5, 50),
            DistanceCategory.Short => (50, 150),
            DistanceCategory.Medium => (150, 500),
            DistanceCategory.Long => (500, 1500),
            DistanceCategory.UltraLong => (1500, 8000),
            _ => (50, 150)
        };
    }

    private double CalculateRouteScore(
        AirportJobProfile origin,
        AirportInfo destination,
        DistanceCategory distance)
    {
        var score = 1.0;

        // Preferred destinations get bonus
        if (origin.PreferredDestinations?.Contains(destination.Icao) == true)
            score *= 3.0;

        // Same region bonus for short flights
        if (distance <= DistanceCategory.Short &&
            origin.Region == destination.Region)
            score *= 1.5;

        // Different region bonus for long flights
        if (distance >= DistanceCategory.Long &&
            origin.Region != destination.Region)
            score *= 1.5;

        // Hub-to-hub routes more common for longer distances
        if (destination.Size <= AirportSize.Large && distance >= DistanceCategory.Medium)
            score *= 2.0;

        // Tourist destinations get passenger bonus
        if (destination.PrimaryType == AirportType.Tourist)
            score *= 1.3;

        // Cargo hubs get cargo bonus
        if (destination.PrimaryType == AirportType.Cargo)
            score *= 1.3;

        // Island destinations for short/very short
        if (destination.PrimaryType == AirportType.Island &&
            distance <= DistanceCategory.Short)
            score *= 2.0;

        return score;
    }
}
```

### Route Network Tiers

Jobs follow realistic route patterns:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         ROUTE NETWORK TIERS                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  TIER 1: HUB-TO-HUB                                                      │
│  ├── Major ↔ Major (KJFK ↔ EGLL, KLAX ↔ RJTT)                          │
│  ├── Major ↔ Large (EGLL ↔ EGCC, KJFK ↔ KBOS)                          │
│  └── Long/Ultra Long distances, high-value cargo, premium passengers     │
│                                                                          │
│  TIER 2: HUB-TO-REGIONAL                                                 │
│  ├── Large ↔ Medium (EGCC ↔ EGGP, KORD ↔ KSTL)                         │
│  ├── Medium ↔ Medium                                                     │
│  └── Medium distances, general cargo, charter flights                    │
│                                                                          │
│  TIER 3: REGIONAL-TO-LOCAL                                               │
│  ├── Medium ↔ Small (Regional hub ↔ local GA)                           │
│  ├── Small ↔ Small                                                       │
│  └── Short distances, mail, small cargo, business passengers             │
│                                                                          │
│  TIER 4: LOCAL OPERATIONS                                                │
│  ├── Small ↔ Tiny (GA airport ↔ grass strip)                            │
│  ├── Tiny ↔ Tiny                                                         │
│  └── Very short, aerial work, island hopping, medical                    │
│                                                                          │
│  TIER 5: SPECIALTY ROUTES                                                │
│  ├── Any ↔ Remote (Supply runs to bush strips)                          │
│  ├── Island chains (Caribbean, Pacific)                                  │
│  └── Unique cargo (oil rigs, mountain resorts, research stations)        │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Job Payout Formula

```csharp
public class JobPayoutCalculator
{
    /// <summary>
    /// Calculates job payout based on multiple factors.
    /// </summary>
    public decimal CalculatePayout(JobParameters parameters)
    {
        // Base rate per nautical mile (varies by distance category)
        decimal baseRatePerNm = GetBaseRate(parameters.DistanceCategory);

        // Distance component
        decimal distancePayout = parameters.DistanceNm * baseRatePerNm;

        // Weight/passenger component
        decimal capacityPayout = parameters.IsCargo
            ? parameters.WeightLbs * GetCargoRatePerLb(parameters.CargoType)
            : parameters.PassengerCount * GetPassengerRate(parameters.PassengerClass);

        // Base payout before modifiers
        decimal basePayout = distancePayout + capacityPayout;

        // Apply modifiers
        decimal finalPayout = basePayout
            * GetUrgencyMultiplier(parameters.Urgency)
            * GetRiskMultiplier(parameters.RiskLevel)
            * GetRouteMultiplier(parameters.RouteDifficulty)
            * GetDemandMultiplier(parameters.Destination)
            * GetTimeOfDayMultiplier(parameters.DepartureTime);

        // Minimum payout floor
        return Math.Max(finalPayout, GetMinimumPayout(parameters.DistanceCategory));
    }

    private decimal GetBaseRate(DistanceCategory category)
    {
        return category switch
        {
            DistanceCategory.VeryShort => 20m,  // $20/nm
            DistanceCategory.Short => 16m,      // $16/nm
            DistanceCategory.Medium => 14m,     // $14/nm
            DistanceCategory.Long => 11m,       // $11/nm
            DistanceCategory.UltraLong => 8m,   // $8/nm
            _ => 14m
        };
    }

    private decimal GetCargoRatePerLb(CargoType cargoType)
    {
        return cargoType.Category switch
        {
            CargoCategory.GeneralCargo => 0.50m,
            CargoCategory.Perishable => 0.80m,
            CargoCategory.Hazardous => 1.20m,
            CargoCategory.LiveAnimals => 1.50m,
            CargoCategory.Oversized => 0.70m,
            CargoCategory.Medical => 2.00m,
            CargoCategory.HighValue => 3.00m,
            CargoCategory.Fragile => 1.00m,
            _ => 0.50m
        };
    }

    private decimal GetPassengerRate(PassengerClass passengerClass)
    {
        return passengerClass switch
        {
            PassengerClass.Economy => 150m,     // $150/pax
            PassengerClass.Business => 400m,    // $400/pax
            PassengerClass.First => 800m,       // $800/pax
            PassengerClass.Charter => 600m,     // $600/pax
            PassengerClass.Medical => 1500m,    // $1500/pax
            PassengerClass.VIP => 2000m,        // $2000/pax
            _ => 150m
        };
    }

    private decimal GetUrgencyMultiplier(JobUrgency urgency)
    {
        return urgency switch
        {
            JobUrgency.Standard => 1.0m,    // 24-48 hours
            JobUrgency.Priority => 1.2m,    // 12-24 hours
            JobUrgency.Express => 1.5m,     // 6-12 hours
            JobUrgency.Urgent => 2.0m,      // 2-6 hours
            JobUrgency.Critical => 3.0m,    // 1-2 hours
            _ => 1.0m
        };
    }

    private decimal GetRiskMultiplier(int riskLevel)
    {
        return riskLevel switch
        {
            1 => 1.0m,
            2 => 1.3m,
            3 => 1.6m,
            4 => 2.0m,
            5 => 3.0m,
            _ => 1.0m
        };
    }

    private decimal GetRouteMultiplier(RouteDifficulty difficulty)
    {
        return difficulty switch
        {
            RouteDifficulty.Easy => 1.0m,       // Good weather, simple terrain
            RouteDifficulty.Moderate => 1.15m,  // Some challenges
            RouteDifficulty.Challenging => 1.3m,// Mountain/ocean crossings
            RouteDifficulty.Difficult => 1.5m,  // Remote, harsh conditions
            RouteDifficulty.Extreme => 2.0m,    // Bush strips, extreme terrain
            _ => 1.0m
        };
    }

    private decimal GetMinimumPayout(DistanceCategory category)
    {
        return category switch
        {
            DistanceCategory.VeryShort => 500m,
            DistanceCategory.Short => 2000m,
            DistanceCategory.Medium => 8000m,
            DistanceCategory.Long => 25000m,
            DistanceCategory.UltraLong => 80000m,
            _ => 2000m
        };
    }
}
```

### Payout Examples

| Route | Distance | Cargo | Urgency | Risk | Payout |
|-------|----------|-------|---------|------|--------|
| EGLL → EGCC | 150nm | 2000lb General | Standard | ⭐ | ~$4,400 |
| EGLL → EGCC | 150nm | 2000lb General | Urgent | ⭐⭐ | ~$11,400 |
| KJFK → KLAX | 2150nm | 8000lb High-Value | Express | ⭐⭐⭐ | ~$98,000 |
| KJFK → EGLL | 3000nm | 20,000lb General | Standard | ⭐⭐ | ~$62,000 |
| PHNL → PHOG | 80nm | 500lb Medical | Critical | ⭐ | ~$5,100 |
| PANC → Remote | 200nm | 3000lb Supply | Standard | ⭐⭐⭐ | ~$9,800 |

### Job Types by Distance Category

```
VERY SHORT (0-50nm):
├── Medical Emergency - Organs, blood, patients
├── Island Resupply - Food, fuel, mail
├── Urgent Documents - Legal, contracts
├── VIP Taxi - Executive short hops
├── Aerial Work - Survey, photography
├── Training Support - Flight school logistics
└── Local Courier - Parts, packages

SHORT (50-150nm):
├── Regional Mail - Postal service
├── Business Passengers - Day trips, meetings
├── Perishable Goods - Seafood, flowers, produce
├── Newspaper/Media - Time-sensitive publications
├── Medical Supplies - Non-emergency hospital supply
├── Small Cargo - General freight
└── Charter Flights - Tourism, events

MEDIUM (150-500nm):
├── General Cargo - Standard freight
├── Charter Service - Groups, sports teams
├── Livestock Transport - Animals, breeding stock
├── Automotive Parts - JIT delivery
├── Electronics - High-value tech cargo
├── E-commerce - Expedited packages
└── Executive Charter - Multi-leg business trips

LONG (500-1500nm):
├── Air Freight - Palletized cargo
├── Passenger Charter - Vacation groups
├── Time-Critical - Manufacturing parts
├── High-Value Cargo - Art, jewelry, equipment
├── Live Animals - Zoo transfers, breeding
├── Dangerous Goods - Hazmat with certification
└── Corporate Shuttle - Regular business routes

ULTRA LONG (1500nm+):
├── International Freight - Cross-continental
├── Airline Contract - Scheduled routes (ATPL)
├── Humanitarian - Aid delivery
├── Military Contract - Government cargo
├── Specialty Transport - Oversized, unusual
├── Premium Passenger - First class charter
└── Time-Sensitive International - Global express
```

### Risk Level Requirements (Updated)

| Level | Requirements | Jobs Available | Pay Multiplier |
|-------|-------------|----------------|----------------|
| ⭐ Standard | CPL + aircraft rating | 60% of jobs | ×1.0 |
| ⭐⭐ Priority | Reputation 3.0+ | 25% of jobs | ×1.3 |
| ⭐⭐⭐ Specialized | DG, Medical, or similar endorsement | 10% of jobs | ×1.6 |
| ⭐⭐⭐⭐ Complex | IR + Night + MEP ratings | 4% of jobs | ×2.0 |
| ⭐⭐⭐⭐⭐ Critical | ATPL + Reputation 4.5+ | 1% of jobs | ×3.0 |

### Job Refresh & Lifecycle

```csharp
public class JobLifecycleService
{
    /// <summary>
    /// Background service runs every 15 minutes.
    /// </summary>
    public async Task ProcessJobLifecycleAsync(Guid worldId, CancellationToken ct)
    {
        // 1. Expire old jobs
        await ExpireJobsAsync(worldId, ct);

        // 2. Generate new jobs for airports below capacity
        var airports = await _airportRepository.GetAllActiveAsync(worldId, ct);

        foreach (var airport in airports)
        {
            var currentCount = await _jobRepository.CountActiveJobsAsync(
                airport.Icao, worldId, ct);

            var profile = await _airportService.GetJobProfileAsync(airport.Icao, ct);

            // Only generate if below 70% capacity
            if (currentCount < profile.BaseJobCount * 0.7)
            {
                var newJobs = await _jobGenerator.GenerateJobsForAirportAsync(
                    airport.Icao, worldId, ct);

                await _jobRepository.AddRangeAsync(newJobs, ct);

                await _auditService.LogSystemEventAsync(
                    "JOB_GENERATION",
                    $"Generated {newJobs.Count} jobs at {airport.Icao}",
                    new { AirportIcao = airport.Icao, JobCount = newJobs.Count });
            }
        }

        // 3. Adjust demand multipliers based on supply/demand
        await UpdateDemandMultipliersAsync(worldId, ct);
    }

    private async Task ExpireJobsAsync(Guid worldId, CancellationToken ct)
    {
        var expiredCount = await _jobRepository.ExpireJobsAsync(
            worldId,
            DateTimeOffset.UtcNow,
            ct);

        if (expiredCount > 0)
        {
            await _auditService.LogSystemEventAsync(
                "JOBS_EXPIRED",
                $"Expired {expiredCount} jobs",
                new { WorldId = worldId, ExpiredCount = expiredCount });
        }
    }
}
```

### Job Entity (Complete)

```csharp
public class Job
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }

    // Route
    public string DepartureIcao { get; set; }
    public string ArrivalIcao { get; set; }
    public double DistanceNm { get; set; }
    public DistanceCategory DistanceCategory { get; set; }

    // Job details
    public JobType Type { get; set; }           // Cargo, Passenger
    public Guid? CargoTypeId { get; set; }
    public CargoType? CargoType { get; set; }
    public double? WeightLbs { get; set; }
    public double? VolumeCuFt { get; set; }
    public int? PassengerCount { get; set; }
    public PassengerClass? PassengerClass { get; set; }

    // Requirements
    public int RiskLevel { get; set; }          // 1-5
    public string[]? RequiredEndorsements { get; set; }
    public string? RequiredAircraftCategory { get; set; }
    public double? MinReputationScore { get; set; }

    // Timing
    public JobUrgency Urgency { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? PickupDeadline { get; set; }
    public DateTimeOffset? DeliveryDeadline { get; set; }

    // Status
    public JobStatus Status { get; set; }
    public Guid? AcceptedByPlayerId { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    // Payout
    public decimal BasePayout { get; set; }
    public decimal FinalPayout { get; set; }    // After modifiers
    public decimal? BonusPayout { get; set; }   // Early completion, etc.

    // Rental (if quick job)
    public bool IsQuickJob { get; set; }
    public Guid? RentalAircraftId { get; set; }
    public decimal? RentalFee { get; set; }

    // Metadata
    public string? Description { get; set; }
    public string? SpecialInstructions { get; set; }
    public RouteDifficulty RouteDifficulty { get; set; }
}

public enum DistanceCategory { VeryShort, Short, Medium, Long, UltraLong }
public enum JobUrgency { Standard, Priority, Express, Urgent, Critical }
public enum JobStatus { Available, Accepted, InProgress, Completed, Failed, Expired, Cancelled }
public enum RouteDifficulty { Easy, Moderate, Challenging, Difficult, Extreme }
```

---

## Phase 5: Aircraft Marketplace & Inventory System

### Overview

The aircraft marketplace provides dynamic, varied inventory at airports worldwide. Dealers spawn with contextually appropriate aircraft based on airport characteristics, and inventory refreshes regularly to maintain variety.

### Dealer Types (Detailed)

```csharp
public enum DealerType
{
    ManufacturerShowroom,  // New aircraft, full warranty, MSRP pricing
    CertifiedPreOwned,     // 80-95% condition, limited warranty
    RegionalDealer,        // Mix of new and used, regional aircraft focus
    BudgetLot,             // 60-80% condition, as-is, steep discounts
    SpecialtyDealer,       // Bush planes, floatplanes, warbirds
    ExecutiveDealer,       // Business jets and turboprops
    CargoSpecialist,       // Freighter aircraft, cargo conversions
    FlightSchool           // Trainers, affordable starter aircraft
}

public class AircraftDealer
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }
    public string AirportIcao { get; set; }

    public DealerType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Inventory settings
    public int MinInventory { get; set; }
    public int MaxInventory { get; set; }
    public int InventoryRefreshDays { get; set; }  // Game days

    // Pricing
    public decimal PriceMultiplier { get; set; }   // 0.6-1.2 typically
    public bool OffersFinancing { get; set; }
    public decimal? FinancingDownPayment { get; set; }
    public decimal? FinancingInterestRate { get; set; }

    // Aircraft categories sold
    public string[] AircraftCategories { get; set; }
    public string[] ExcludedCategories { get; set; }

    // Condition ranges
    public int MinCondition { get; set; }          // 0-100
    public int MaxCondition { get; set; }
    public int MinHours { get; set; }
    public int MaxHours { get; set; }

    // Reputation
    public double ReputationScore { get; set; }    // Affects trade-in values
    public int TotalSales { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastInventoryRefresh { get; set; }
}
```

### Dealer Distribution by Airport

| Airport Size | Dealers | Dealer Types | Max Aircraft |
|--------------|---------|--------------|--------------|
| Major | 4-6 | All types, multiple manufacturer showrooms | 80-150 |
| Large | 3-4 | Manufacturer, Regional, Budget, Executive | 40-80 |
| Medium | 2-3 | Regional, CertifiedPreOwned, Budget | 20-40 |
| Small | 1-2 | FlightSchool, Budget, maybe Regional | 8-20 |
| Tiny | 0-1 | Budget or Specialty only | 2-8 |
| Remote | 0-1 | Specialty (bush planes) only | 1-5 |

### Dealer Type Characteristics

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        DEALER CHARACTERISTICS                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  MANUFACTURER SHOWROOM                                                   │
│  ├── Condition: 100% (new only)                                         │
│  ├── Hours: 0 (demo flights only)                                       │
│  ├── Price: MSRP (×1.0)                                                 │
│  ├── Warranty: Full manufacturer warranty                                │
│  ├── Financing: Best rates (2-4% with good credit)                      │
│  └── Aircraft: Latest models from specific manufacturer                  │
│                                                                          │
│  CERTIFIED PRE-OWNED                                                     │
│  ├── Condition: 80-95%                                                  │
│  ├── Hours: 500-3000                                                    │
│  ├── Price: ×0.70-0.90 of MSRP                                          │
│  ├── Warranty: Limited (6 game months)                                  │
│  ├── Financing: Good rates (3-5%)                                       │
│  └── Aircraft: Recent models, well-maintained                           │
│                                                                          │
│  REGIONAL DEALER                                                         │
│  ├── Condition: 70-100%                                                 │
│  ├── Hours: 0-8000                                                      │
│  ├── Price: ×0.80-1.05 of MSRP                                          │
│  ├── Warranty: Varies                                                   │
│  ├── Financing: Standard rates (4-6%)                                   │
│  └── Aircraft: Regional aircraft focus (turboprops, light jets)         │
│                                                                          │
│  BUDGET LOT                                                              │
│  ├── Condition: 60-80%                                                  │
│  ├── Hours: 3000-15000+                                                 │
│  ├── Price: ×0.40-0.70 of MSRP                                          │
│  ├── Warranty: None (as-is)                                             │
│  ├── Financing: Higher rates (6-10%)                                    │
│  └── Aircraft: Older models, higher hours, needs work                   │
│                                                                          │
│  SPECIALTY DEALER                                                        │
│  ├── Condition: 65-95%                                                  │
│  ├── Hours: Varies widely                                               │
│  ├── Price: ×0.80-1.50 (collectibles premium)                           │
│  ├── Warranty: Limited                                                  │
│  ├── Financing: Case by case                                            │
│  └── Aircraft: Bush planes, floatplanes, warbirds, unique aircraft      │
│                                                                          │
│  EXECUTIVE DEALER                                                        │
│  ├── Condition: 85-100%                                                 │
│  ├── Hours: 0-5000                                                      │
│  ├── Price: ×0.95-1.10 (premium service)                                │
│  ├── Warranty: Extended options                                         │
│  ├── Financing: Premium rates for high-value aircraft                   │
│  └── Aircraft: Business jets, turboprops, high-end pistons              │
│                                                                          │
│  CARGO SPECIALIST                                                        │
│  ├── Condition: 70-90%                                                  │
│  ├── Hours: 2000-12000                                                  │
│  ├── Price: ×0.75-0.95                                                  │
│  ├── Warranty: Limited                                                  │
│  ├── Financing: Standard                                                │
│  └── Aircraft: Freighters, cargo conversions, utility aircraft          │
│                                                                          │
│  FLIGHT SCHOOL                                                           │
│  ├── Condition: 75-95%                                                  │
│  ├── Hours: 1000-8000                                                   │
│  ├── Price: ×0.65-0.85 (starter-friendly)                               │
│  ├── Warranty: Limited                                                  │
│  ├── Financing: Best starter loan rates                                 │
│  └── Aircraft: Trainers (C172, PA-28, DA40)                             │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Aircraft Category Distribution

```csharp
public class AircraftCategoryDistribution
{
    public static Dictionary<AirportSize, AircraftCategoryWeights> GetDistribution()
    {
        return new()
        {
            [AirportSize.Major] = new()
            {
                ["LightSingle"] = 0.10,      // C172, PA-28
                ["LightTwin"] = 0.10,        // C421, PA-34
                ["TurbopropSingle"] = 0.12,  // TBM, PC-12
                ["TurbopropTwin"] = 0.15,    // King Air, ATR
                ["LightJet"] = 0.12,         // Citation CJ, Phenom
                ["MidJet"] = 0.12,           // Citation XLS, Learjet
                ["HeavyJet"] = 0.08,         // Challenger, Gulfstream
                ["RegionalJet"] = 0.10,      // CRJ, E-Jets
                ["NarrowBody"] = 0.08,       // A320, B737
                ["WideBody"] = 0.03          // A350, B777
            },

            [AirportSize.Large] = new()
            {
                ["LightSingle"] = 0.15,
                ["LightTwin"] = 0.15,
                ["TurbopropSingle"] = 0.18,
                ["TurbopropTwin"] = 0.18,
                ["LightJet"] = 0.12,
                ["MidJet"] = 0.10,
                ["HeavyJet"] = 0.05,
                ["RegionalJet"] = 0.05,
                ["NarrowBody"] = 0.02,
                ["WideBody"] = 0.00
            },

            [AirportSize.Medium] = new()
            {
                ["LightSingle"] = 0.25,
                ["LightTwin"] = 0.25,
                ["TurbopropSingle"] = 0.20,
                ["TurbopropTwin"] = 0.15,
                ["LightJet"] = 0.08,
                ["MidJet"] = 0.05,
                ["HeavyJet"] = 0.02,
                ["RegionalJet"] = 0.00,
                ["NarrowBody"] = 0.00,
                ["WideBody"] = 0.00
            },

            [AirportSize.Small] = new()
            {
                ["LightSingle"] = 0.50,
                ["LightTwin"] = 0.30,
                ["TurbopropSingle"] = 0.12,
                ["TurbopropTwin"] = 0.05,
                ["LightJet"] = 0.03,
                ["MidJet"] = 0.00,
                ["HeavyJet"] = 0.00,
                ["RegionalJet"] = 0.00,
                ["NarrowBody"] = 0.00,
                ["WideBody"] = 0.00
            },

            [AirportSize.Tiny] = new()
            {
                ["LightSingle"] = 0.80,
                ["LightTwin"] = 0.15,
                ["TurbopropSingle"] = 0.05,
                ["TurbopropTwin"] = 0.00,
                ["LightJet"] = 0.00,
                ["MidJet"] = 0.00,
                ["HeavyJet"] = 0.00,
                ["RegionalJet"] = 0.00,
                ["NarrowBody"] = 0.00,
                ["WideBody"] = 0.00
            },

            [AirportSize.Remote] = new()
            {
                ["LightSingle"] = 0.70,  // STOL-capable
                ["LightTwin"] = 0.20,    // Bush twins
                ["TurbopropSingle"] = 0.10, // Caravan, Kodiak
                ["TurbopropTwin"] = 0.00,
                ["LightJet"] = 0.00,
                ["MidJet"] = 0.00,
                ["HeavyJet"] = 0.00,
                ["RegionalJet"] = 0.00,
                ["NarrowBody"] = 0.00,
                ["WideBody"] = 0.00
            }
        };
    }
}
```

### Inventory Generation Algorithm

```csharp
public class InventoryGenerationService
{
    /// <summary>
    /// Generates aircraft inventory for a dealer based on type and location.
    /// </summary>
    public async Task<List<DealerInventory>> GenerateInventoryAsync(
        AircraftDealer dealer,
        CancellationToken ct = default)
    {
        var inventory = new List<DealerInventory>();
        var airport = await _airportRepository.GetByIcaoAsync(dealer.AirportIcao, ct);
        var targetCount = _random.Next(dealer.MinInventory, dealer.MaxInventory + 1);

        // Get category weights for this airport size
        var categoryWeights = AircraftCategoryDistribution
            .GetDistribution()[airport.Size];

        // Filter by dealer's allowed categories
        categoryWeights = FilterByDealerCategories(categoryWeights, dealer);

        for (int i = 0; i < targetCount; i++)
        {
            // 1. Select aircraft category
            var category = SelectWeightedCategory(categoryWeights);

            // 2. Select specific aircraft template from category
            var template = await SelectAircraftTemplateAsync(category, dealer, ct);
            if (template == null) continue;

            // 3. Generate condition and hours based on dealer type
            var (condition, hours) = GenerateConditionAndHours(dealer);

            // 4. Calculate price
            var price = CalculatePrice(template, condition, hours, dealer, airport);

            // 5. Create inventory item
            var item = new DealerInventory
            {
                Id = Guid.NewGuid(),
                DealerId = dealer.Id,
                WorldId = dealer.WorldId,
                AircraftTemplateId = template.Id,

                Condition = condition,
                TotalHours = hours,

                BasePrice = template.BasePrice,
                ListPrice = price,

                IsNew = condition == 100 && hours == 0,
                HasWarranty = dealer.Type is DealerType.ManufacturerShowroom
                    or DealerType.CertifiedPreOwned,
                WarrantyMonths = GetWarrantyMonths(dealer.Type, condition),

                // Generate unique registration
                Registration = GenerateRegistration(airport.Country),

                // Random livery from available options
                LiveryId = SelectRandomLivery(template),

                // Optional extras
                AvionicsPackage = GenerateAvionicsPackage(template, dealer),
                Modifications = GenerateModifications(template, dealer),

                ListedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(
                    dealer.InventoryRefreshDays * 2.58) // Convert to real days
            };

            inventory.Add(item);
        }

        // Ensure variety - no more than 3 of same exact model
        return EnforceVariety(inventory);
    }

    private (int condition, int hours) GenerateConditionAndHours(AircraftDealer dealer)
    {
        return dealer.Type switch
        {
            DealerType.ManufacturerShowroom => (100, 0),

            DealerType.CertifiedPreOwned => (
                _random.Next(80, 96),
                _random.Next(500, 3001)
            ),

            DealerType.RegionalDealer => (
                _random.Next(70, 101),
                _random.Next(0, 8001)
            ),

            DealerType.BudgetLot => (
                _random.Next(60, 81),
                _random.Next(3000, 15001)
            ),

            DealerType.SpecialtyDealer => (
                _random.Next(65, 96),
                _random.Next(500, 10001)
            ),

            DealerType.ExecutiveDealer => (
                _random.Next(85, 101),
                _random.Next(0, 5001)
            ),

            DealerType.CargoSpecialist => (
                _random.Next(70, 91),
                _random.Next(2000, 12001)
            ),

            DealerType.FlightSchool => (
                _random.Next(75, 96),
                _random.Next(1000, 8001)
            ),

            _ => (85, 2000)
        };
    }
}
```

### Aircraft Pricing Formula (Complete)

```csharp
public class AircraftPricingService
{
    /// <summary>
    /// Calculates the list price for a dealer inventory aircraft.
    /// </summary>
    public decimal CalculatePrice(
        AircraftTemplate template,
        int condition,
        int totalHours,
        AircraftDealer dealer,
        AirportInfo airport)
    {
        // 1. Start with base MSRP
        decimal basePrice = template.BasePrice;

        // 2. Apply condition multiplier
        decimal conditionMult = GetConditionMultiplier(condition);

        // 3. Apply hours multiplier
        decimal hoursMult = GetHoursMultiplier(totalHours);

        // 4. Apply dealer type multiplier
        decimal dealerMult = dealer.PriceMultiplier;

        // 5. Apply location premium
        decimal locationMult = GetLocationMultiplier(airport);

        // 6. Apply market conditions (supply/demand)
        decimal marketMult = GetMarketMultiplier(template.Category);

        // 7. Calculate final price
        decimal finalPrice = basePrice
            * conditionMult
            * hoursMult
            * dealerMult
            * locationMult
            * marketMult;

        // 8. Round to nearest $1000 for cleaner prices
        return Math.Round(finalPrice / 1000) * 1000;
    }

    private decimal GetConditionMultiplier(int condition)
    {
        // Non-linear depreciation curve
        return condition switch
        {
            100 => 1.00m,
            >= 95 => 0.95m,
            >= 90 => 0.88m,
            >= 85 => 0.80m,
            >= 80 => 0.72m,
            >= 75 => 0.62m,
            >= 70 => 0.52m,
            >= 65 => 0.42m,
            >= 60 => 0.32m,
            _ => 0.25m
        };
    }

    private decimal GetHoursMultiplier(int hours)
    {
        // Time-in-service depreciation
        return hours switch
        {
            0 => 1.00m,
            < 500 => 0.98m,
            < 1000 => 0.95m,
            < 2000 => 0.92m,
            < 5000 => 0.85m,
            < 8000 => 0.78m,
            < 10000 => 0.70m,
            < 15000 => 0.62m,
            _ => 0.55m
        };
    }

    private decimal GetLocationMultiplier(AirportInfo airport)
    {
        // Remote locations have premium due to transport costs
        return airport.Size switch
        {
            AirportSize.Major => 1.00m,
            AirportSize.Large => 1.02m,
            AirportSize.Medium => 1.05m,
            AirportSize.Small => 1.08m,
            AirportSize.Tiny => 1.12m,
            AirportSize.Remote => 1.20m,
            _ => 1.00m
        };
    }

    private decimal GetMarketMultiplier(string category)
    {
        // Market conditions affect pricing
        // This could be dynamic based on supply/demand in world
        // For now, static baseline
        return 1.0m;
    }
}
```

### Inventory Pricing Examples

| Aircraft | Base MSRP | Condition | Hours | Dealer | Location | Final Price |
|----------|-----------|-----------|-------|--------|----------|-------------|
| Cessna 172 | $350,000 | 100% | 0 | Manufacturer | Major | $350,000 |
| Cessna 172 | $350,000 | 85% | 2500 | CPO | Large | $238,000 |
| Cessna 172 | $350,000 | 70% | 8000 | Budget | Medium | $119,000 |
| King Air 350 | $8,000,000 | 90% | 3000 | Executive | Major | $6,160,000 |
| King Air 350 | $8,000,000 | 75% | 9000 | Regional | Medium | $2,790,000 |
| TBM 930 | $4,200,000 | 95% | 800 | CPO | Large | $3,610,000 |
| Kodiak 100 | $2,500,000 | 80% | 4000 | Specialty | Remote | $1,530,000 |

### Inventory Refresh System

```csharp
public class InventoryRefreshService
{
    /// <summary>
    /// Background service runs daily to refresh dealer inventories.
    /// </summary>
    public async Task RefreshInventoriesAsync(Guid worldId, CancellationToken ct)
    {
        var dealers = await _dealerRepository.GetAllAsync(worldId, ct);

        foreach (var dealer in dealers)
        {
            // Check if refresh is due
            var daysSinceRefresh = (DateTimeOffset.UtcNow - dealer.LastInventoryRefresh)
                .TotalDays / 2.58; // Convert to game days

            if (daysSinceRefresh < dealer.InventoryRefreshDays)
                continue;

            // 1. Remove sold aircraft (already handled on sale)

            // 2. Remove some unsold aircraft (market rotation)
            var currentInventory = await _inventoryRepository
                .GetByDealerAsync(dealer.Id, ct);

            var toRemove = currentInventory
                .Where(i => ShouldRemoveFromInventory(i, dealer))
                .ToList();

            await _inventoryRepository.RemoveRangeAsync(toRemove, ct);

            // 3. Generate new aircraft to fill capacity
            var remaining = currentInventory.Count - toRemove.Count;
            var targetCount = _random.Next(dealer.MinInventory, dealer.MaxInventory + 1);

            if (remaining < targetCount)
            {
                var newInventory = await _inventoryGenerator.GenerateInventoryAsync(
                    dealer, targetCount - remaining, ct);

                await _inventoryRepository.AddRangeAsync(newInventory, ct);
            }

            // 4. Adjust prices on remaining inventory
            await AdjustPricesAsync(dealer, ct);

            // 5. Update dealer
            dealer.LastInventoryRefresh = DateTimeOffset.UtcNow;
            await _dealerRepository.UpdateAsync(dealer, ct);

            await _auditService.LogSystemEventAsync(
                "INVENTORY_REFRESH",
                $"Refreshed inventory for {dealer.Name}",
                new { DealerId = dealer.Id, RemovedCount = toRemove.Count });
        }
    }

    private bool ShouldRemoveFromInventory(DealerInventory item, AircraftDealer dealer)
    {
        // Older listings more likely to be removed (simulating sales to NPCs)
        var age = (DateTimeOffset.UtcNow - item.ListedAt).TotalDays;
        var baseChance = 0.1; // 10% base chance

        // Increase chance based on age
        var ageBonus = Math.Min(age / 30, 0.3); // Up to +30% for old items

        // Budget lots have higher turnover
        if (dealer.Type == DealerType.BudgetLot)
            baseChance += 0.15;

        return _random.NextDouble() < (baseChance + ageBonus);
    }

    private async Task AdjustPricesAsync(AircraftDealer dealer, CancellationToken ct)
    {
        var inventory = await _inventoryRepository.GetByDealerAsync(dealer.Id, ct);

        foreach (var item in inventory)
        {
            var age = (DateTimeOffset.UtcNow - item.ListedAt).TotalDays;

            // Price reduction for aging inventory
            if (age > 14) // After 2 weeks
            {
                var reduction = Math.Min(age / 100, 0.15); // Up to 15% reduction
                item.ListPrice *= (1 - (decimal)reduction);
                item.IsOnSale = true;
                item.SalePercentage = (int)(reduction * 100);
            }
        }

        await _inventoryRepository.UpdateRangeAsync(inventory, ct);
    }
}
```

### Dealer Inventory Entity

```csharp
public class DealerInventory
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public AircraftDealer Dealer { get; set; }
    public Guid WorldId { get; set; }
    public Guid AircraftTemplateId { get; set; }
    public AircraftTemplate AircraftTemplate { get; set; }

    // Aircraft specifics
    public string Registration { get; set; }
    public int Condition { get; set; }           // 0-100
    public int TotalHours { get; set; }
    public int? CycleCount { get; set; }         // Landings
    public Guid? LiveryId { get; set; }

    // Pricing
    public decimal BasePrice { get; set; }       // MSRP
    public decimal ListPrice { get; set; }       // Current asking price
    public bool IsOnSale { get; set; }
    public int? SalePercentage { get; set; }
    public decimal? MinAcceptablePrice { get; set; } // For negotiation

    // Status
    public bool IsNew { get; set; }
    public bool HasWarranty { get; set; }
    public int? WarrantyMonths { get; set; }
    public bool IsReserved { get; set; }
    public Guid? ReservedByPlayerId { get; set; }
    public DateTimeOffset? ReservedUntil { get; set; }

    // Extras
    public string? AvionicsPackage { get; set; }  // "Standard", "Premium", "Custom"
    public string[]? Modifications { get; set; }
    public string? Notes { get; set; }

    // Timing
    public DateTimeOffset ListedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? SoldAt { get; set; }
    public Guid? SoldToPlayerId { get; set; }
}
```

### Sales & Discounts System

```csharp
public class DealerDiscount
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public Guid WorldId { get; set; }

    public DiscountType Type { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }

    // Scope
    public string[]? ApplicableCategories { get; set; }
    public Guid[]? ApplicableTemplateIds { get; set; }
    public bool AppliesToAllInventory { get; set; }

    // Discount details
    public decimal DiscountPercentage { get; set; }  // 0.05 = 5%
    public decimal? MaxDiscountAmount { get; set; }

    // Financing specials
    public decimal? SpecialInterestRate { get; set; }
    public decimal? SpecialDownPayment { get; set; }

    // Validity
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public int? MaxRedemptions { get; set; }
    public int CurrentRedemptions { get; set; }

    // Requirements
    public decimal? MinPurchaseAmount { get; set; }
    public int? RequiredPlayerLevel { get; set; }
    public int? RequiredPreviousPurchases { get; set; }
}

public enum DiscountType
{
    SeasonalSale,       // Black Friday, Summer sale
    Clearance,          // Old inventory clearout
    NewCustomer,        // First purchase discount
    LoyaltyReward,      // Repeat buyer discount
    FleetDiscount,      // Multi-aircraft purchase
    FinancingSpecial,   // 0% intro APR
    HolidayPromotion,   // Seasonal events
    ManufacturerRebate  // Factory incentives
}
```

### Dealer Generation (World Initialization)

```csharp
public class DealerGenerationService
{
    /// <summary>
    /// Generates dealers for an airport based on its characteristics.
    /// Called during world initialization or when adding new airports.
    /// </summary>
    public async Task<List<AircraftDealer>> GenerateDealersForAirportAsync(
        string icao,
        Guid worldId,
        CancellationToken ct = default)
    {
        var airport = await _airportRepository.GetByIcaoAsync(icao, ct);
        var dealers = new List<AircraftDealer>();

        var dealerConfig = GetDealerConfiguration(airport.Size);

        foreach (var (dealerType, probability) in dealerConfig)
        {
            if (_random.NextDouble() > probability)
                continue;

            var dealer = CreateDealer(dealerType, airport, worldId);
            dealers.Add(dealer);
        }

        return dealers;
    }

    private Dictionary<DealerType, double> GetDealerConfiguration(AirportSize size)
    {
        return size switch
        {
            AirportSize.Major => new()
            {
                [DealerType.ManufacturerShowroom] = 1.0,  // Always
                [DealerType.CertifiedPreOwned] = 1.0,
                [DealerType.RegionalDealer] = 0.8,
                [DealerType.BudgetLot] = 0.7,
                [DealerType.ExecutiveDealer] = 0.9,
                [DealerType.CargoSpecialist] = 0.5
            },

            AirportSize.Large => new()
            {
                [DealerType.ManufacturerShowroom] = 0.7,
                [DealerType.CertifiedPreOwned] = 0.9,
                [DealerType.RegionalDealer] = 0.9,
                [DealerType.BudgetLot] = 0.6,
                [DealerType.ExecutiveDealer] = 0.5
            },

            AirportSize.Medium => new()
            {
                [DealerType.RegionalDealer] = 0.9,
                [DealerType.CertifiedPreOwned] = 0.6,
                [DealerType.BudgetLot] = 0.7,
                [DealerType.FlightSchool] = 0.4
            },

            AirportSize.Small => new()
            {
                [DealerType.FlightSchool] = 0.8,
                [DealerType.BudgetLot] = 0.5,
                [DealerType.RegionalDealer] = 0.3
            },

            AirportSize.Tiny => new()
            {
                [DealerType.FlightSchool] = 0.4,
                [DealerType.BudgetLot] = 0.3
            },

            AirportSize.Remote => new()
            {
                [DealerType.SpecialtyDealer] = 0.5  // Bush planes
            },

            _ => new()
        };
    }
}
```

### Files to Create (Job & Inventory Systems)

```
PilotLife.Database/Entities/Jobs/
├── Job.cs
├── JobStatus.cs (enum)
├── JobUrgency.cs (enum)
├── JobType.cs (enum)
├── DistanceCategory.cs (enum)
├── RouteDifficulty.cs (enum)
├── FlightJob.cs
└── JobGenerationLog.cs

PilotLife.Database/Entities/Marketplace/
├── AircraftDealer.cs
├── DealerType.cs (enum)
├── DealerInventory.cs
├── DealerDiscount.cs
├── DiscountType.cs (enum)
└── InventoryRefreshLog.cs

PilotLife.Database/Entities/Airports/
├── AirportSize.cs (enum)
├── AirportType.cs (enum)
└── AirportJobProfile.cs

PilotLife.Application/Services/Jobs/
├── IJobGenerationService.cs
├── JobGenerationService.cs
├── IDestinationSelector.cs
├── DestinationSelector.cs
├── IJobPayoutCalculator.cs
├── JobPayoutCalculator.cs
├── IJobLifecycleService.cs
└── JobLifecycleService.cs

PilotLife.Application/Services/Marketplace/
├── IInventoryGenerationService.cs
├── InventoryGenerationService.cs
├── IAircraftPricingService.cs
├── AircraftPricingService.cs
├── IInventoryRefreshService.cs
├── InventoryRefreshService.cs
├── IDealerGenerationService.cs
└── DealerGenerationService.cs
```

---

## Phase 6: Player Auctions

### Auction Types
- **English**: Ascending bids, most common
- **Dutch**: Price drops until buyer
- **Sealed Bid**: Blind bidding
- **Buy-It-Now**: Fixed price

### Listing Requirements
- Own aircraft outright (no active loans)
- Aircraft not in use or maintenance
- Grounded during auction

### Fees
- Listing: 0.5% of starting bid (min $500)
- Success: 5% of sale (max $500k)
- Buyer premium: 3% (max $150k)
- Transfer: $1,500 flat

### Bidding Rules
- Minimum increment: 2% or $5,000
- Proxy bidding supported
- Sniping protection: +10 min if bid in last 5 min
- Funds held in escrow

---

## Phase 7: AI Crew & Workers

### AI Crew (For Passive Income)
| Type | Salary/Game Month | Salary/Real Day | Can Fly | Incident Rate |
|------|-------------------|-----------------|---------|---------------|
| Private Pilot | $3,000 | ~$1,165 | Light singles | 5-15% |
| Commercial Pilot | $8,000 | ~$3,100 | Twins, turboprops | 2-8% |
| Airline Pilot | $15,000 | ~$5,815 | All aircraft | 0.5-3% |
| Captain | $25,000 | ~$9,690 | All, international | 0.1-1% |

*Salaries paid every game month (~2.5 real days)*

### AI Flight System
- Assign aircraft + pilot + route
- Flight simulated in real-time (or accelerated)
- Revenue calculated: Cargo rate × weight - operating costs
- Incident chance based on pilot skill + aircraft condition

### Operations Staff (Paid per Game Month / ~2.5 Real Days)
- Dispatcher ($4,500/mo): -15% fuel costs, route optimization
- Load Master ($3,500/mo): -20% loading time, +5% cargo condition
- Accountant ($5,000/mo): -5% fees, better loan rates
- Mechanics: Reduce maintenance costs and time

### Worker Attributes
- Skill (1-5 stars): Affects performance
- Morale (0-100): Low = incidents/quitting
- Traits: Efficient, Careful, Slow, Careless, etc.

---

## Phase 8: Risk & Consequences

### Illegal Cargo Detection
```
Detection Chance = Base Risk × Modifiers

Base: Cannabis(25%), Cocaine(40%), Heroin(50%), Firearms(60%)
Modifiers: +10% previous violations, +15% high-risk route, -10% small airport
```

### Consequence Tiers
| Tier | Trigger | Fine | Suspension (Game) | Suspension (Real) | Other |
|------|---------|------|-------------------|-------------------|-------|
| Minor | First offense | $50k-$100k | None | None | +2 points |
| Moderate | Repeat/medium | $100k-$500k | 1-4 game months | 2.5-10 real days | Cargo seized |
| Severe | Large quantity | $500k-$2M | 4-12 game months | 10-31 real days | Aircraft at risk |
| Criminal | Trafficking | $2M+ | All licenses revoked | Permanent | Asset seizure |

### Violation Points (Decay: 1 point per game month / ~2.5 real days)
- 0-4: Good standing
- 5-9: Increased inspections
- 10-14: Probation, random audits
- 15-19: License suspension
- 20+: Revocation

---

## Implementation Phases

### Phase 1: Foundation (World + IAM + User)
- World, WorldSettings entities
- Role, RolePermission, UserRole entities
- IAuthorizationService implementation
- Seed default "Global" world
- Seed default roles (SuperAdmin, Admin, Moderator, Player)
- Update User with world relationships
- DbContext configuration

### Phase 2: Core Entities (Aircraft)
- AircraftTemplate, OwnedAircraft, AircraftComponent
- AircraftModification, MaintenanceLog
- Aircraft pricing formula service

### Phase 3: PlayerWorld & Economy
- PlayerWorld entity (balance, credit score, stats)
- Player joins world flow
- World selection in web app

### Phase 4: Cargo & Jobs
- CargoCategory, CargoSubcategory, CargoType
- Job, FlightJob, JobPriority, JobStatus
- Job generation service
- Job acceptance/tracking

### Phase 5: Flight Connector Integration
- Connector API endpoints for job sync
- Flight tracking endpoints (start, update, complete)
- Job completion validation service
- TrackedFlight, FlightFinancials entities
- Multi-job flight support

### Phase 6: Licenses & Exams
- LicenseType, UserLicense, LicenseShopItem
- LicenseExam, ExamManeuver, ExamCheckpoint
- Exam scheduling API
- Connector exam tracking protocol
- Exam result processing
- License grant on pass

### Phase 7: Banking
- Bank, Loan, LoanPayment, CreditScoreEvent
- Loan application and approval logic
- Payment processing
- Credit score calculations

### Phase 8: Marketplace
- AircraftDealer, DealerInventory, DealerDiscount
- Inventory generation service
- Purchase flow

### Phase 9: Auctions
- Auction, AuctionBid
- Bidding engine with escrow
- Sniping protection

### Phase 10: Workers & AI Crew
- Worker entity with types
- AI flight simulation service
- Route management

### Phase 11: Risk System
- InspectionEvent, ViolationRecord
- Detection logic
- Consequence processing

### Phase 12: Admin Panel
- World settings management UI
- User management with IAM
- Economy monitoring dashboard
- Moderation tools

---

## Files to Create

```
PilotLife.Database/Entities/
├── World/
│   ├── World.cs
│   ├── WorldSettings.cs
│   └── PlayerWorld.cs
├── IAM/
│   ├── Role.cs
│   ├── RolePermission.cs
│   ├── UserRole.cs
│   └── PermissionCategory.cs (enum)
├── Aircraft/
│   ├── AircraftTemplate.cs
│   ├── OwnedAircraft.cs
│   ├── AircraftComponent.cs
│   ├── AircraftModification.cs
│   ├── MaintenanceLog.cs
│   ├── AircraftRentalListing.cs
│   └── RentalTransaction.cs
├── Airports/
│   ├── OwnedAirport.cs
│   ├── AirportFuelStock.cs
│   ├── FuelPurchase.cs
│   └── LandingFeeTransaction.cs
├── Audit/
│   ├── AuditLog.cs
│   ├── AuditCategory.cs (enum)
│   └── AuditSeverity.cs (enum)
├── Experience/
│   ├── PlayerExperience.cs
│   ├── PlayerSkill.cs
│   ├── XPTransaction.cs
│   ├── XPSource.cs (enum)
│   └── SkillDefinition.cs
├── CommunityAircraft/
│   ├── AircraftSubmission.cs
│   ├── CommunityAircraft.cs
│   ├── SubmissionStatus.cs (enum)
│   └── EngineType.cs (enum)
├── Cargo/
│   ├── CargoCategory.cs
│   ├── CargoSubcategory.cs
│   └── CargoType.cs
├── Jobs/
│   ├── Job.cs
│   └── FlightJob.cs
├── Licenses/
│   ├── LicenseType.cs
│   ├── UserLicense.cs
│   ├── LicenseShopItem.cs
│   ├── LicenseExam.cs
│   ├── ExamManeuver.cs
│   └── ExamCheckpoint.cs
├── Banking/
│   ├── Bank.cs
│   ├── Loan.cs
│   ├── LoanPayment.cs
│   └── CreditScoreEvent.cs
├── Marketplace/
│   ├── AircraftDealer.cs
│   ├── DealerInventory.cs
│   └── DealerDiscount.cs
├── Auctions/
│   ├── Auction.cs
│   └── AuctionBid.cs
├── Workers/
│   └── Worker.cs
├── Risk/
│   ├── InspectionEvent.cs
│   └── ViolationRecord.cs
└── Flights/
    ├── TrackedFlight.cs
    └── FlightFinancials.cs

PilotLife.API/Services/
├── Authorization/
│   ├── IAuthorizationService.cs
│   └── AuthorizationService.cs
├── World/
│   ├── WorldService.cs
│   └── WorldSettingsService.cs
├── Aircraft/
│   ├── AircraftPricingService.cs
│   ├── AircraftRentalService.cs
│   ├── FuelPayloadService.cs        # Auto-fill calculations
│   └── MaintenanceService.cs
├── Airports/
│   ├── AirportOwnershipService.cs
│   ├── FuelStockService.cs
│   ├── LandingFeeService.cs
│   └── AirportUpgradeService.cs
├── Jobs/
│   ├── JobGenerationService.cs
│   ├── JobCompletionService.cs
│   └── QuickJobService.cs
├── Licenses/
│   ├── LicenseService.cs
│   └── ExamService.cs
├── Banking/
│   ├── LoanService.cs
│   ├── StarterLoanService.cs
│   └── CreditScoreService.cs
├── Audit/
│   ├── IAuditService.cs
│   └── AuditService.cs
├── Experience/
│   ├── IExperienceService.cs
│   ├── ExperienceService.cs
│   ├── ISkillService.cs
│   └── SkillService.cs
├── CommunityAircraft/
│   ├── IAircraftSubmissionService.cs
│   ├── AircraftSubmissionService.cs
│   ├── ICommunityAircraftService.cs
│   └── CommunityAircraftService.cs
├── MarketplaceService.cs
├── AuctionService.cs
├── AIFlightService.cs
├── WorkerService.cs
├── InspectionService.cs
├── FlightValidationService.cs
└── PayoutService.cs

PilotLife.API/Controllers/
├── WorldsController.cs
├── IAMController.cs (admin)
├── ExamsController.cs
├── RentalsController.cs (aircraft rental listings)
├── QuickJobsController.cs (browse/accept quick jobs)
├── AirportsController.cs (owned airports, fuel, landing fees)
├── FuelController.cs (fuel purchases, auto-fill)
├── StarterLoanController.cs (first aircraft financing)
├── AuditController.cs (staff audit log viewer)
├── ExperienceController.cs (XP, levels, leaderboards)
├── SkillsController.cs (skill trees, unlocking, bonuses)
├── AircraftSubmissionsController.cs (player submissions)
├── CommunityAircraftController.cs (approved aircraft database)
├── AdminAircraftSubmissionsController.cs (admin review workflow)
├── ConnectorController.cs (flight tracking API)
└── ... (existing controllers)

PilotLife.Connector/ (C++ Updates)
├── src/
│   ├── JobTracker.cpp/h           # Job state management
│   ├── ExamTracker.cpp/h          # Exam flight monitoring
│   ├── ManeuverDetector.cpp/h     # Maneuver grading
│   ├── FlightState.cpp/h          # State machine
│   ├── FuelPayloadManager.cpp/h   # Auto-fill, weight/fuel read/write
│   ├── LandingReporter.cpp/h      # Landing fee integration
│   ├── AircraftDetector.cpp/h     # Community aircraft detection and submission
│   ├── XPTracker.cpp/h            # Experience gain tracking and reporting
│   └── ApiClient.cpp/h            # REST client updates
```

---

## Key Design Decisions Summary

| Aspect | Decision |
|--------|----------|
| Game Time | **1 game year = 31 real days** (1 game month ≈ 2.5 real days) |
| Multi-World | Isolated economies, starting with "Global" |
| **Fast Progression** | **CPL in ~1 hour (Discovery 10min → PPL 25min → CPL 30min)** |
| **First Aircraft** | **~2 hours with Starter Loan ($250k, 1.5%/mo, 0% down)** |
| **Quick Jobs** | **Rental-based jobs for players without aircraft (20-40% cut)** |
| Aircraft Pricing | Real-world based formula |
| Earnings | High ($8k-$8M per flight based on aircraft) |
| **Fuel System** | **Finite daily supply at airports, AVGAS + Jet-A, variable pricing** |
| **Auto-Fill** | **Connector sets fuel/payload via SimConnect on player request** |
| **Player Airports** | **Buy airports ($50k-$15M), earn landing fees + fuel markup** |
| **Landing Fees** | **Charged on every landing, goes to airport owner (90%)** |
| Loans | 1-24 game months, simple monthly interest |
| **Starter Loan** | **$250k max, 1.5%/mo, 0% down, CPL required, once per world** |
| Licenses | Expiring, 3-6 game months validity, NO hour requirements |
| License Exams | Practical flight tests tracked by connector (skill-based, not time-gated) |
| **Exam Prerequisites** | **Discovery → PPL → (CPL/Night/IR/MEP parallel) → ATPL** |
| Jobs | Dynamic, expiring, multi-job flights |
| Job Completion | Connector validates departure/arrival airports |
| **Aircraft Rentals** | **System fleet + player fleet, passive income for owners** |
| AI Crew | Passive income via simulated flights |
| Auctions | Player-to-player with escrow |
| Detection | Hybrid (base + behavior modifiers) |
| Punishment | Moderate (fines + suspensions) |
| IAM | Role-based, global + per-world scoped |
| World Settings | Admin-adjustable via UI |
| **Experience System** | **XP from jobs/exams/flights, level up → skill points, 100 levels** |
| **Skill Trees** | **4 trees (Economy, Aviation, Business, Reputation), passive bonuses** |
| **Community Aircraft** | **Player submissions → Admin review (24-48hr) → SuperAdmin approval** |
| **Aircraft Matching** | **Connector collects sim values, maps to AircraftTemplate, per-world** |
| SimBrief | Future integration, API ready |

---

## Connector API Endpoints (New)

```
POST   /api/connector/auth              - Authenticate connector session
GET    /api/connector/active-jobs       - Get player's active jobs for current world
GET    /api/connector/active-exam       - Get current exam if scheduled
POST   /api/connector/flight/start      - Start flight tracking
POST   /api/connector/flight/update     - Position/state update (batched)
POST   /api/connector/flight/complete   - Flight completed, process jobs
POST   /api/connector/exam/start        - Begin exam
POST   /api/connector/exam/maneuver     - Report maneuver result
POST   /api/connector/exam/complete     - Submit exam results
```

---

## Quick Jobs & Rental API Endpoints

```
# Quick Jobs
GET    /api/worlds/{worldId}/quick-jobs              - List available quick jobs (with rentals)
GET    /api/worlds/{worldId}/quick-jobs/{jobId}      - Get quick job details + rental options
POST   /api/worlds/{worldId}/quick-jobs/{jobId}/accept - Accept quick job with rental selection

# Aircraft Rentals (Owner management)
GET    /api/worlds/{worldId}/my-rentals              - List my aircraft rental listings
POST   /api/worlds/{worldId}/my-rentals              - Create rental listing for owned aircraft
PUT    /api/worlds/{worldId}/my-rentals/{id}         - Update rental settings (%, restrictions)
DELETE /api/worlds/{worldId}/my-rentals/{id}         - Remove aircraft from rental pool
GET    /api/worlds/{worldId}/my-rentals/earnings     - View rental income history

# Rental Browse (For renters)
GET    /api/worlds/{worldId}/rentals/available       - Browse available rentals at location
GET    /api/worlds/{worldId}/rentals/{icao}          - Rentals at specific airport
```

---

## Fuel & Airport API Endpoints

```
# Fuel System
GET    /api/worlds/{worldId}/fuel/{icao}             - Get fuel availability/prices at airport
GET    /api/worlds/{worldId}/fuel/{icao}/history     - Fuel price history
POST   /api/worlds/{worldId}/fuel/purchase           - Purchase fuel
POST   /api/worlds/{worldId}/fuel/auto-fill          - Calculate and request auto-fill

# Auto-Fill (Connector integration)
POST   /api/connector/auto-fill/calculate            - Calculate fuel/payload for route
POST   /api/connector/auto-fill/execute              - Execute auto-fill on aircraft
GET    /api/connector/aircraft/weights               - Get current aircraft weights

# Player-Owned Airports
GET    /api/worlds/{worldId}/airports/for-sale       - Browse airports available for purchase
GET    /api/worlds/{worldId}/airports/{icao}         - Get airport details (fees, fuel, owner)
POST   /api/worlds/{worldId}/airports/{icao}/purchase - Buy an airport
GET    /api/worlds/{worldId}/my-airports             - List my owned airports
PUT    /api/worlds/{worldId}/my-airports/{icao}/fees - Update landing fees
PUT    /api/worlds/{worldId}/my-airports/{icao}/fuel - Update fuel markup
POST   /api/worlds/{worldId}/my-airports/{icao}/upgrade - Purchase airport upgrade
GET    /api/worlds/{worldId}/my-airports/{icao}/revenue - View revenue dashboard

# Landing Fees (automatic via connector, but viewable)
GET    /api/worlds/{worldId}/landing-fees/history    - My landing fee payments
GET    /api/worlds/{worldId}/landing-fees/earnings   - My airport landing fee earnings
```

---

## Starter Loan API Endpoints

```
# Starter Loan Program
GET    /api/worlds/{worldId}/starter-loan/eligibility  - Check if player qualifies
GET    /api/worlds/{worldId}/starter-loan/terms        - View loan terms and options
POST   /api/worlds/{worldId}/starter-loan/apply        - Apply for starter loan
GET    /api/worlds/{worldId}/starter-loan/status       - View current loan status
POST   /api/worlds/{worldId}/starter-loan/payment      - Make extra payment
```

---

## Audit API Endpoints (Staff Only)

```
# Audit Log Viewing (requires Logs_View permission)
GET    /api/worlds/{worldId}/audit                     - Search audit logs (paginated)
GET    /api/worlds/{worldId}/audit/{id}                - Get specific audit entry details
GET    /api/worlds/{worldId}/audit/player/{playerId}   - Get player's audit history
GET    /api/worlds/{worldId}/audit/staff/{staffId}     - Get staff member's actions
GET    /api/worlds/{worldId}/audit/export              - Export audit logs to CSV

# Query Parameters for /audit:
# - category: AuditCategory enum
# - severity: Info, Warning, Critical
# - actorId: Filter by who performed action
# - targetType: Player, Airport, Aircraft, etc.
# - targetId: Filter by affected entity
# - dateFrom, dateTo: Date range
# - search: Full-text search
# - page, pageSize: Pagination
```

---

## Experience & Skills API Endpoints

```
# Experience & Leveling
GET    /api/worlds/{worldId}/experience              - Get player's current XP, level, available points
GET    /api/worlds/{worldId}/experience/history      - XP gain history
GET    /api/worlds/{worldId}/experience/leaderboard  - World XP leaderboard

# Skill Trees
GET    /api/worlds/{worldId}/skills                  - Get all skill tree definitions
GET    /api/worlds/{worldId}/skills/my-skills        - Get player's unlocked skills
POST   /api/worlds/{worldId}/skills/unlock           - Spend skill point to unlock a skill
GET    /api/worlds/{worldId}/skills/bonuses          - Get active bonuses from skills
POST   /api/worlds/{worldId}/skills/respec           - Reset skills (costs in-game currency)
```

---

## Community Aircraft API Endpoints

```
# Aircraft Submission (Players)
GET    /api/worlds/{worldId}/aircraft-submissions/my-submissions  - List player's submissions
POST   /api/worlds/{worldId}/aircraft-submissions                 - Submit aircraft for approval
GET    /api/worlds/{worldId}/aircraft-submissions/{id}            - Get submission status
DELETE /api/worlds/{worldId}/aircraft-submissions/{id}            - Cancel pending submission

# Aircraft Submission (Admin - requires aircraft review permission)
GET    /api/admin/worlds/{worldId}/aircraft-submissions           - List all pending submissions
GET    /api/admin/worlds/{worldId}/aircraft-submissions/{id}      - Get submission details for review
PUT    /api/admin/worlds/{worldId}/aircraft-submissions/{id}/review - Submit admin review (approve/reject/needs-info)

# Aircraft Approval (SuperAdmin - final approval)
GET    /api/admin/aircraft-submissions/pending-approval           - List submissions awaiting final approval
PUT    /api/admin/aircraft-submissions/{id}/approve               - Final approval, adds to community aircraft
PUT    /api/admin/aircraft-submissions/{id}/reject                - Final rejection

# Community Aircraft Database
GET    /api/worlds/{worldId}/community-aircraft                   - List approved community aircraft
GET    /api/worlds/{worldId}/community-aircraft/{id}              - Get specific community aircraft details
GET    /api/worlds/{worldId}/community-aircraft/by-sim-title/{title} - Find by simulator title

# Connector Integration
GET    /api/connector/community-aircraft/check                    - Check if current aircraft is in database
POST   /api/connector/community-aircraft/auto-detect              - Submit aircraft data for matching
```

---

## Web App World Selection Flow

```
1. User logs in
2. Check if user has joined any worlds
   - No worlds: Show "Join World" prompt
   - Has worlds: Show world selector (or last used)
3. User selects world → Store in session/local storage
4. All API calls include worldId header or path
5. UI shows current world indicator
6. World switcher in header/sidebar
```
