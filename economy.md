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
- Job, FlightJob
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

## License Exam System

### Overview
Players must pass practical flight exams to earn licenses. Exams are started from the web app and tracked by the connector.

### Exam Types

| Exam | Prerequisites | Duration | Aircraft | Key Requirements |
|------|--------------|----------|----------|------------------|
| SPL | None | 30 min | Any single | 3 takeoffs/landings, basic maneuvers |
| PPL | SPL | 60 min | Single piston | Navigation, emergencies, landings |
| CPL | PPL | 90 min | Complex single | Commercial maneuvers, precision |
| ATPL | CPL | 120 min | Multi-engine | Airline procedures, multi-crew |
| Night Rating | PPL | 45 min | Any | 5 night landings, navigation |
| IR | PPL | 90 min | IFR equipped | Approaches, holds, navigation |
| MEP | PPL | 60 min | Twin piston | Engine-out procedures |
| Type Rating | CPL or ATPL | 60-120 min | Specific type | Multi-airport circuit, touch-and-go's |

**No Hour Requirements**: If you can afford the exam fee and pass the practical test, you earn the license. Skill > time logged.

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

| Failure Type | Cooldown Period | Retake Fee |
|--------------|-----------------|------------|
| First failure (60-69%) | 24 hours | 50% of original |
| First failure (<60%) | 48 hours | 75% of original |
| Second failure | 72 hours | 100% of original |
| Third+ failure | 7 days | 150% of original |

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
| Exam | Fee | Retake |
|------|-----|--------|
| SPL | $500 | $250 |
| PPL | $2,000 | $1,000 |
| CPL | $5,000 | $2,500 |
| ATPL | $10,000 | $5,000 |
| Night/IR/MEP | $3,000 | $1,500 |
| Type Rating | $5,000-$20,000 | 50% |

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

## Game Time Conversion

**Critical Design Decision: 1 Game Month = 7 Real Days**

| Game Time | Real Time |
|-----------|-----------|
| 1 game week | ~1.75 real days |
| 1 game month | 7 real days |
| 1 game quarter | 21 real days |
| 1 game year | 84 real days (~3 months) |

All time-based systems use game time unless explicitly stated as "real time".

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

### Loan Terms (Game Months)
- Short-term: 1-3 months (7-21 real days)
- Standard: 3-6 months (21-42 real days)
- Aircraft: 6-12 months (42-84 real days)
- Major: 12-24 months (84-168 real days)

### Credit Score (300-850)
- Starting: 650
- On-time payment: +5 (max +50/month)
- Late payment: -25 to -100
- Default: -200
- Recovery: +1 per game month with no violations

### Payment Schedule
- Due: Every game month (7 real days)
- Grace period: 2 real days
- Late: Day 3-4 (fee + credit hit)
- Very late: Day 5-7 (major credit hit)
- Default: Day 8+ (asset seizure begins)

---

## Phase 3: License System (Game Time Adjusted)

### License Validity (Game Months)
| License | Price | Validity | Renewal |
|---------|-------|----------|---------|
| SPL | $2,500 | Until PPL | N/A |
| PPL | $12,000 | 6 months | $3,000 |
| CPL | $35,000 | 3 months | $8,000 |
| ATPL | $75,000 | 3 months | $15,000 |
| SEP/MEP | $5,000/$12,000 | 6 months | $1,500/$4,000 |
| IR | $18,000 | 3 months | $5,000 |
| Type Ratings | $25k-$150k | 3 months | 25% of cost |
| DG License | $15,000 | 6 months | $5,000 |

### License Shop Features
- View all available licenses with prerequisites
- Check if player qualifies
- Purchase with instant or exam-required options
- Renewal reminders before expiry
- Bulk discounts for multiple ratings

---

## Phase 4: Job System

### Job Generation
- Jobs spawn at airports based on size and type
- Large hubs: 50-100 jobs, all categories
- Small airports: 5-20 jobs, local cargo
- Refresh: New jobs every few hours

### Job Properties
- **Expiry**: Critical(1-2hr), Urgent(2-6hr), Express(6-12hr), Priority(12-24hr), Standard(24-48hr)
- **Risk Levels**: 1-5 stars with increasing license requirements
- **Multi-Job**: Aircraft can accept multiple jobs to same destination
- **Deadlines**: Each job has pickup and delivery deadlines

### Risk Level Requirements
| Level | Requirements | Pay Multiplier |
|-------|-------------|----------------|
| ⭐ Standard | CPL + aircraft rating | ×1.0 |
| ⭐⭐ Priority | Good reputation (3.0+) | ×1.3 |
| ⭐⭐⭐ Specialized | Endorsement (DG, Medical, etc) | ×1.6 |
| ⭐⭐⭐⭐ Complex | IR + Night ratings | ×2.0 |
| ⭐⭐⭐⭐⭐ Critical | Reputation 4.5+ | ×3.0 |

**No Hour Gates**: Job access is based on licenses held and reputation earned, not time logged. A skilled player who passes exams quickly can access high-tier jobs immediately.

---

## Phase 5: Aircraft Marketplace

### Dealer Types
1. **Manufacturer Dealership** - New aircraft, full warranty, MSRP
2. **Certified Pre-Owned** - Used 80-95% condition, limited warranty
3. **Budget Lot** - Used 60-80% condition, as-is, steep discounts
4. **Specialty Dealer** - Niche aircraft (bush planes, warbirds)

### Inventory by Airport
- Large Hub: 20-50 aircraft, multiple dealers
- Medium: 10-25 aircraft, 1-2 dealers
- Small: 3-10 aircraft, usually one dealer
- Remote: 0-3 aircraft, bush planes

### Used Aircraft Pricing
```
PRICE = BASE × CONDITION_MULT × HOURS_MULT

CONDITION: 100%(1.0), 90%(0.88), 80%(0.70), 70%(0.50), 60%(0.32)
HOURS: 0-500(1.0), 2k-5k(0.92), 5k-10k(0.85), 10k+(0.65)
```

### Sales & Discounts
- Seasonal: Spring/Summer/Black Friday (10-25% off)
- Clearance: Model changeover, overstock
- Loyalty: Repeat buyer, fleet discounts
- Financing specials: 0% intro rate, low down payment

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
| Type | Salary/Month | Can Fly | Incident Rate |
|------|-------------|---------|---------------|
| Private Pilot | $3,000 | Light singles | 5-15% |
| Commercial Pilot | $8,000 | Twins, turboprops | 2-8% |
| Airline Pilot | $15,000 | All aircraft | 0.5-3% |
| Captain | $25,000 | All, international | 0.1-1% |

### AI Flight System
- Assign aircraft + pilot + route
- Flight simulated in real-time (or accelerated)
- Revenue calculated: Cargo rate × weight - operating costs
- Incident chance based on pilot skill + aircraft condition

### Operations Staff
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
| Tier | Trigger | Fine | Suspension | Other |
|------|---------|------|------------|-------|
| Minor | First offense | $50k-$100k | None | +2 points |
| Moderate | Repeat/medium | $100k-$500k | 1-4 game months | Cargo seized |
| Severe | Large quantity | $500k-$2M | 4-12 game months | Aircraft at risk |
| Criminal | Trafficking | $2M+ | All licenses revoked | Asset seizure |

### Violation Points (Decay: 1/game month)
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
│   └── MaintenanceLog.cs
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
├── AircraftPricingService.cs
├── MaintenanceService.cs
├── JobGenerationService.cs
├── JobCompletionService.cs
├── LicenseService.cs
├── ExamService.cs
├── LoanService.cs
├── CreditScoreService.cs
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
├── ConnectorController.cs (flight tracking API)
└── ... (existing controllers)

PilotLife.Connector/ (C++ Updates)
├── src/
│   ├── JobTracker.cpp/h        # Job state management
│   ├── ExamTracker.cpp/h       # Exam flight monitoring
│   ├── ManeuverDetector.cpp/h  # Maneuver grading
│   ├── FlightState.cpp/h       # State machine
│   └── ApiClient.cpp/h         # REST client updates
```

---

## Key Design Decisions Summary

| Aspect | Decision |
|--------|----------|
| Game Time | 1 game month = 7 real days |
| Multi-World | Isolated economies, starting with "Global" |
| Aircraft Pricing | Real-world based formula |
| Earnings | High ($8k-$8M per flight based on aircraft) |
| Loans | 1-24 game months, simple monthly interest |
| Licenses | Expiring, 3-6 game months validity, NO hour requirements |
| License Exams | Practical flight tests tracked by connector (skill-based, not time-gated) |
| Jobs | Dynamic, expiring, multi-job flights |
| Job Completion | Connector validates departure/arrival airports |
| AI Crew | Passive income via simulated flights |
| Auctions | Player-to-player with escrow |
| Detection | Hybrid (base + behavior modifiers) |
| Punishment | Moderate (fines + suspensions) |
| IAM | Role-based, global + per-world scoped |
| World Settings | Admin-adjustable via UI |
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
