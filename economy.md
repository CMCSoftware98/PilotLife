# PilotLife Economy System Design

## Overview

This document outlines a comprehensive economy system for PilotLife that includes:
- **Cargo & Passenger Transport** - Legal and illegal goods with real-world items
- **Dynamic Job System** - Jobs with expiry, risk levels, multi-job flights, license requirements
- **License System** - Aircraft type ratings, maintenance certifications, special endorsements
- **License Shop** - Purchase and renew licenses with prerequisites
- **Aircraft Marketplace** - Dealers at airports, new/used aircraft, sales and discounts
- **Player Auctions** - Sell aircraft to other players via bidding system
- **Risk & Consequences** - Illegal cargo penalties, license suspension, fines
- **Banking & Loans** - Multiple banks with credit scores affecting loan terms
- **AI Crew System** - Hire AI pilots to run automated routes for passive income
- **Worker System** - Hire employees for operations
- **Integration with MSFS** - Real flight tracking for earnings

### Economy Philosophy

**Realistic Business Simulation**: This is NOT a casual game economy. Aircraft cost real-world prices, and earnings are proportional. An A320 costing $98M should take significant effort to pay off, but the earnings per flight are substantial ($2M+ cargo runs).

**ROI Consistency**: All aircraft maintain roughly 2-5% ROI per flight, meaning:
- Expensive aircraft = high earnings, high risk
- Cheap aircraft = lower earnings, lower risk
- Break-even typically takes 30-50 successful flights

### Key Design Decisions

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Aircraft Pricing** | Real-World | A320 = ~$98M, Cessna 172 = ~$350k - creates meaningful progression |
| **Earnings per Flight** | High | $8-10k (small), $40-50k (medium), $2M+ (airliners) - worth the investment |
| **Detection System** | Hybrid | Base random chance + behavior modifiers (previous violations, routes, patterns) |
| **License Expiry** | Expiring | Licenses expire and require renewal fees - realistic, ongoing costs |
| **Punishment Level** | Moderate | Fines + short suspensions (7-30 days) - balanced risk/reward |
| **New Player Start** | Earn Everything | Start with $50,000, must earn first PPL - full progression |

### Earnings by Aircraft Class

| Aircraft Class | Example | Purchase Price | Earnings/Flight (Cargo) | Earnings/Flight (Pax) | ROI/Flight |
|----------------|---------|----------------|------------------------|----------------------|------------|
| **Light Single** | Cessna 172 | $350,000 | $8,000-$12,000 | $4,000-$6,000 | 2-3% |
| **Light Single (High Perf)** | Cessna 182 | $500,000 | $12,000-$18,000 | $6,000-$10,000 | 2-4% |
| **Light Twin** | Cessna 421C | $800,000-$1.5M | $40,000-$60,000 | $20,000-$35,000 | 3-5% |
| **Turboprop Single** | TBM 930 | $4M | $80,000-$120,000 | $50,000-$80,000 | 2-3% |
| **Turboprop Twin** | King Air 350 | $8M | $150,000-$250,000 | $100,000-$150,000 | 2-3% |
| **Regional Jet** | CRJ-700 | $25M | $400,000-$600,000 | $250,000-$400,000 | 2% |
| **Narrow Body** | A320 | $98M | $1.5M-$2.5M | $800,000-$1.2M | 1.5-2.5% |
| **Wide Body** | B777 | $350M | $5M-$8M | $3M-$5M | 1.5-2% |

**Why Cargo > Passengers**: Cargo has higher margins because:
- No cabin crew required
- No passenger services (meals, entertainment)
- Simpler operations
- Higher weight utilization

### New Player Progression Path

```
NEW PLAYER JOURNEY:

1. STARTING STATE
   ├── Balance: $50,000 (starting capital)
   ├── Licenses: None
   ├── Aircraft: None (must rent)
   ├── Credit Score: 650 (neutral)
   └── Can do: Training flights only (no pay)

2. FIRST STEPS (Hours 0-10)
   ├── Rent small aircraft (C150, C152) - $200/hr wet rental
   ├── Complete training flights
   ├── No cargo/passenger jobs allowed
   └── Goal: Accumulate 10 flight hours (~$2,000 spent)

3. STUDENT PILOT LICENSE (SPL) - $2,500
   ├── Requirements: 10 flight hours
   ├── Allows: Solo training flights
   ├── Cannot: Carry passengers or cargo for hire
   └── Goal: Accumulate 40 total hours (~$8,000 spent)

4. PRIVATE PILOT LICENSE (PPL) - $12,000
   ├── Requirements: 40 flight hours, SPL, exam
   ├── Allows: Personal flights, passengers (no pay)
   ├── Cannot: Fly for hire yet
   └── Goal: Get SEP rating, accumulate 100 hours

5. SINGLE ENGINE PISTON RATING (SEP) - $5,000
   ├── Requirements: PPL, 50 total hours
   ├── Allows: Fly single-engine piston aircraft commercially
   └── First "real" aircraft access

6. COMMERCIAL PILOT LICENSE (CPL) - $35,000
   ├── Requirements: PPL, IR, 200 hours
   ├── Allows: Fly for hire!
   ├── Access: Cargo jobs, charter flights
   └── First income opportunity

TOTAL TO REACH CPL: ~$65,000-$80,000 + 200 hours flight time
(This is why starting capital is $50,000 + ability to take loans)

RENTAL AIRCRAFT (Available to all):
- Cessna 150/152: $200/hr wet (training only until SPL)
- Cessna 172: $350/hr wet (requires SEP)
- Cessna 182: $500/hr wet (requires SEP + Complex)
- Piper PA-34 Seneca: $800/hr wet (requires MEP)
```

---

## 1. Cargo System

### 1.1 Cargo Categories

```
CATEGORY HIERARCHY:
├── General Cargo
│   ├── Mail & Parcels
│   ├── Consumer Goods
│   └── Industrial Equipment
├── Perishables
│   ├── Food & Beverages
│   ├── Pharmaceuticals
│   └── Medical Supplies
├── Bulk Materials
│   ├── Construction Materials
│   ├── Raw Materials
│   └── Agricultural Products
├── High Value
│   ├── Electronics
│   ├── Precious Metals
│   ├── Artwork & Antiques
│   └── Currency/Securities
├── Dangerous Goods (Legal)
│   ├── Flammables
│   ├── Explosives
│   ├── Radioactive
│   └── Corrosives
├── Live Cargo
│   ├── Animals
│   └── Organ Transport
├── Passengers
│   ├── Economy
│   ├── Business
│   ├── First Class
│   └── Charter/VIP
└── Contraband (Illegal)
    ├── Narcotics
    ├── Weapons
    ├── Counterfeit Goods
    └── Smuggled Items
```

### 1.2 Cargo Category Hierarchy

The cargo system uses a three-tier hierarchy: **Category → Subcategory → Item**

Jobs can request:
- A specific item: "Deliver 500kg of Steel"
- Any item in a subcategory: "Deliver 500kg of any Metal"
- Any item in a category: "Deliver 500kg of any Raw Material"

```
CARGO HIERARCHY:

📦 MAIL & PARCELS (Category)
├── 📬 Standard Mail
│   ├── Letters (very light, time-sensitive)
│   ├── Small Parcels (up to 5kg each)
│   └── Priority Mail (urgent, bonus for speed)
└── 📦 Express Packages
    ├── Documents (contracts, legal papers)
    ├── E-commerce Parcels
    └── Medical Samples

🏭 RAW MATERIALS (Category)
├── 🔩 Metals
│   ├── Iron Ore
│   ├── Steel Billets
│   ├── Aluminum Ingots
│   ├── Copper Wire
│   └── Scrap Metal
├── 🪵 Wood Products
│   ├── Lumber (boards)
│   ├── Plywood Sheets
│   ├── Hardwood Logs
│   └── Wood Chips
├── 🪨 Stone & Minerals
│   ├── Gravel
│   ├── Marble Slabs
│   ├── Granite Blocks
│   └── Sand
└── 🧪 Chemicals (non-hazardous)
    ├── Industrial Chemicals
    ├── Fertilizers
    └── Plastics (pellets)

🔧 INDUSTRIAL GOODS (Category)
├── 🛠️ Tools & Equipment
│   ├── Power Tools
│   ├── Hand Tools
│   ├── Industrial Machinery Parts
│   └── Construction Equipment
├── 🚗 Automotive
│   ├── Car Parts
│   ├── Tires
│   ├── Engines
│   └── Vehicle Bodies
└── ⚙️ Manufacturing
    ├── Factory Components
    ├── Bearings & Gears
    └── Electrical Components

📱 CONSUMER GOODS (Category)
├── 📺 Electronics
│   ├── Smartphones
│   ├── Computers/Laptops
│   ├── TVs & Monitors
│   └── Gaming Consoles
├── 👕 Textiles & Clothing
│   ├── Clothing (bulk)
│   ├── Fabrics
│   └── Shoes
└── 🏠 Household
    ├── Furniture (flat-pack)
    ├── Appliances
    └── Home Decor

🥗 PERISHABLES (Category)
├── 🍎 Food Products
│   ├── Fresh Produce
│   ├── Frozen Foods
│   ├── Dairy Products
│   └── Meat & Seafood
├── 💊 Pharmaceuticals
│   ├── Medications
│   ├── Vaccines (cold chain)
│   └── Medical Supplies
└── 🌸 Flowers & Plants
    ├── Cut Flowers
    └── Live Plants

💎 HIGH VALUE (Category)
├── 💰 Precious Metals
│   ├── Gold Bars
│   ├── Silver Bars
│   └── Platinum
├── 💍 Luxury Goods
│   ├── Jewelry
│   ├── Watches
│   └── Designer Items
├── 🖼️ Art & Antiques
│   ├── Paintings
│   ├── Sculptures
│   └── Antique Furniture
└── 💵 Currency & Securities
    ├── Banknotes
    ├── Coins
    └── Bearer Bonds

⚠️ DANGEROUS GOODS (Category) - Requires DG License
├── 🔥 Flammables
│   ├── Aviation Fuel
│   ├── Industrial Solvents
│   └── Paints & Coatings
├── 💥 Explosives
│   ├── Mining Explosives
│   ├── Fireworks
│   └── Ammunition
└── ☢️ Hazardous
    ├── Radioactive Materials
    ├── Corrosives
    └── Toxic Substances

🐾 LIVE CARGO (Category)
├── 🐕 Animals
│   ├── Pets (dogs, cats)
│   ├── Livestock
│   ├── Exotic Animals
│   └── Racing Horses
└── 🏥 Medical
    ├── Organ Transport
    ├── Blood Products
    └── Medical Teams

🚫 CONTRABAND (Category) - ILLEGAL
├── 💉 Narcotics
│   ├── Cocaine
│   ├── Heroin
│   ├── Cannabis
│   ├── LSD
│   └── Synthetic Drugs
├── 🔫 Weapons
│   ├── Firearms
│   ├── Ammunition (illegal)
│   └── Military Equipment
└── 🎭 Smuggled Goods
    ├── Counterfeit Currency
    ├── Stolen Art
    ├── Counterfeit Goods
    └── Human Trafficking (NOT INCLUDED - too dark)
```

### 1.3 Cargo Pricing (High-Earnings Model)

**IMPORTANT: Earnings scale with aircraft capability!**

The cargo system is designed around meaningful earnings that justify real-world aircraft prices:
- **Small Singles** (C172, C182): $8,000-$18,000 per flight
- **Light Twins** (C421C, PA-34): $40,000-$60,000 per flight
- **Turboprops** (TBM, King Air): $80,000-$250,000 per flight
- **Regional Jets** (CRJ, ERJ): $400,000-$600,000 per flight
- **Narrow Body** (A320, B737): $1,500,000-$2,500,000 per flight
- **Wide Body** (B777, A350): $5,000,000-$8,000,000 per flight

#### Cargo Pricing Formula

```
CARGO JOB VALUE = BaseCargoRate × Weight × DistanceMultiplier × AircraftCapabilityBonus

Where:
- BaseCargoRate: $/kg based on cargo type
- Weight: kg of cargo
- DistanceMultiplier: Scales with route length
- AircraftCapabilityBonus: Larger/faster aircraft get better paying jobs
```

#### Base Rates by Category ($ per kg)

| Category | Subcategory | Base Rate/kg | Notes |
|----------|-------------|--------------|-------|
| **Mail & Parcels** | Standard | $15-25 | High volume, time-sensitive |
| | Express | $40-60 | Premium rates for speed |
| **Raw Materials** | Metals | $8-15 | Heavy, fills capacity |
| | Wood | $5-10 | Bulky |
| | Stone | $3-6 | Very heavy |
| | Chemicals | $12-25 | Requires handling |
| **Industrial** | Tools | $25-50 | Medium value |
| | Automotive | $30-60 | Parts, engines |
| | Manufacturing | $35-70 | Precision equipment |
| **Consumer** | Electronics | $80-150 | High value density |
| | Textiles | $15-30 | Light, bulky |
| | Household | $20-40 | Mixed |
| **Perishables** | Food | $25-50 | Time-critical |
| | Pharma | $150-300 | Cold chain, urgent |
| | Flowers | $60-100 | Very time-sensitive |
| **High Value** | Precious Metals | $500-1,000 | Security required |
| | Luxury | $200-400 | Insurance needed |
| | Art | $150-300 | Fragile handling |
| | Currency | $300-600 | Maximum security |
| **Dangerous** | Flammables | $40-80 | DG license required |
| | Explosives | $100-200 | Special permits |
| | Hazardous | $80-150 | Strict regulations |
| **Live Cargo** | Animals | $50-100 | Care requirements |
| | Medical | $400-800 | Organs, emergency |

#### Example Legal Cargo Jobs

```
SMALL SINGLE (Cessna 172) - ~400kg capacity:
├── Mail run: 300kg × $20/kg × 1.2 (distance) = $7,200
├── Electronics: 200kg × $100/kg × 1.0 = $20,000 (premium job)
└── Average job: $8,000-$12,000

LIGHT TWIN (Cessna 421C) - ~800kg capacity:
├── Industrial parts: 600kg × $50/kg × 1.3 = $39,000
├── Pharmaceuticals: 300kg × $200/kg × 1.0 = $60,000
└── Average job: $40,000-$60,000

TURBOPROP (King Air 350) - ~1,500kg capacity:
├── Mixed cargo: 1200kg × $60/kg × 1.5 = $108,000
├── Medical emergency: 500kg × $500/kg × 1.0 = $250,000
└── Average job: $150,000-$250,000

NARROW BODY (A320) - ~20,000kg capacity:
├── General cargo: 18,000kg × $40/kg × 1.8 = $1,296,000
├── Electronics bulk: 15,000kg × $100/kg × 1.5 = $2,250,000
└── Average job: $1,500,000-$2,500,000

WIDE BODY (B777) - ~50,000kg capacity:
├── Bulk freight: 45,000kg × $50/kg × 2.0 = $4,500,000
├── High value mix: 40,000kg × $150/kg × 1.5 = $9,000,000
└── Average job: $5,000,000-$8,000,000
```

#### Illegal Cargo (CONTRABAND) - High Risk, High Reward

Illegal cargo pays 3-5x legal rates but with severe consequences if caught.

| Item | Base Rate/kg | Max Quantity | Max Payout (Small Plane) | Detection Risk |
|------|--------------|--------------|--------------------------|----------------|
| **Cannabis** | $100-200 | 200kg | $40,000 | 25% |
| **Cocaine** | $500-800 | 100kg | $80,000 | 40% |
| **Heroin** | $600-1,000 | 50kg | $50,000 | 50% |
| **LSD** | $2,000-4,000 | 10kg | $40,000 | 45% |
| **Firearms** | $400-800 | 200kg | $160,000 | 60% |
| **Counterfeit Currency** | $200-400 | 100kg | $40,000 | 35% |
| **Stolen Art** | $300-600 | 150kg | $90,000 | 30% |

**Illegal Cargo Risk Analysis:**
```
Example: Cocaine run in Cessna 421C
- 80kg × $600/kg × 1.3 (distance) = $62,400
- 40% detection chance

If caught:
- Cargo seized: -$62,400 (lost earnings)
- Fine: $100,000-$500,000
- License suspension: 30-90 days
- Criminal record: Increased future detection
- Possible aircraft seizure

Expected Value = (0.6 × $62,400) - (0.4 × $300,000) = $37,440 - $120,000 = -$82,560

Illegal cargo is VERY risky - only profitable if you avoid detection consistently.
Repeated runs increase detection chance significantly.
```

#### Distance & Time Multipliers

```
DISTANCE MULTIPLIER:
- 0-200 NM: ×1.0 (local)
- 200-500 NM: ×1.3 (regional)
- 500-1000 NM: ×1.5 (medium haul)
- 1000-2000 NM: ×1.8 (long haul)
- 2000-4000 NM: ×2.0 (very long haul)
- 4000+ NM: ×2.5 (intercontinental)

TIME-SENSITIVE BONUS:
- Standard: ×1.0
- Priority (24hr): ×1.3
- Express (12hr): ×1.6
- Urgent (6hr): ×2.0
- Critical (2hr): ×3.0
```

### 1.3 Cargo Entity Design

```csharp
public class CargoType
{
    public Guid Id { get; set; }

    // Identity
    public string Name { get; set; }              // "Gold Bullion"
    public string Description { get; set; }
    public CargoCategory Category { get; set; }
    public string IconUrl { get; set; }

    // Physical Properties
    public decimal MinWeightKg { get; set; }
    public decimal MaxWeightKg { get; set; }
    public decimal DensityKgPerM3 { get; set; }   // For volume calculation
    public bool IsStackable { get; set; }
    public bool IsFragile { get; set; }

    // Value
    public decimal BaseValuePerKg { get; set; }
    public decimal ValueVariancePercent { get; set; }  // Market fluctuation range

    // Requirements
    public bool RequiresRefrigeration { get; set; }
    public bool RequiresPressurization { get; set; }
    public bool RequiresSecureHold { get; set; }
    public bool RequiresDangerousGoodsLicense { get; set; }
    public int MinimumAircraftWtc { get; set; }   // Minimum wake turbulence category

    // Legality
    public LegalStatus LegalStatus { get; set; }
    public decimal DetectionRiskPercent { get; set; }  // Chance of inspection
    public decimal BaseFineAmount { get; set; }
    public int LicenseSuspensionDays { get; set; }

    // Timing
    public bool IsTimeSensitive { get; set; }
    public int MaxDeliveryHours { get; set; }     // 0 = no limit
    public decimal LatePenaltyPercentPerHour { get; set; }

    // Availability
    public bool IsEnabled { get; set; }
    public string[] AvailableAtAirportTypes { get; set; }  // ["large_airport", "medium_airport"]

    public DateTime CreatedAt { get; set; }
}

public enum CargoCategory
{
    Mail,
    ConsumerGoods,
    Industrial,
    Perishables,
    BulkMaterials,
    HighValue,
    DangerousGoods,
    LiveCargo,
    Passengers,
    Contraband
}

public enum LegalStatus
{
    Legal,
    RequiresPermit,      // Legal with special license
    Restricted,          // Legal only in certain jurisdictions
    Illegal              // Always illegal
}
```

---

## 1.5 Dynamic Job System

### Job Overview

Jobs are the core income source. They're dynamically generated at airports and have:
- **Expiry times** - Jobs disappear if not accepted
- **Risk levels** - Higher risk = better pay, but requires licenses
- **Multiple jobs per flight** - Combine cargo going same direction
- **License requirements** - Some jobs need specific endorsements

### Job Generation

```
JOB GENERATION FACTORS:

1. AIRPORT TYPE (affects quantity and quality)
   ├── Large Hub (KJFK, EGLL, KLAX): 50-100 jobs available
   │   ├── High-value cargo frequent
   │   ├── International routes
   │   └── Airliner-sized jobs
   ├── Medium Airport: 20-50 jobs available
   │   ├── Regional cargo
   │   ├── Mixed aircraft sizes
   │   └── Some specialty cargo
   ├── Small Airport: 5-20 jobs available
   │   ├── Local deliveries
   │   ├── Small aircraft jobs
   │   └── Occasional premium jobs
   └── Remote/Bush: 1-10 jobs available
       ├── Supply runs
       ├── Emergency cargo
       └── High pay for accessibility

2. TIME FACTORS
   ├── Business hours (6AM-6PM): More jobs, better variety
   ├── Night: Fewer jobs, night rating often required
   ├── Weekdays: Business cargo heavy
   └── Weekends: Consumer goods, leisure travel

3. MARKET CONDITIONS
   ├── Events: Holiday rush (+50% mail/packages)
   ├── Seasons: Agriculture seasonal, tourism peaks
   ├── Economic: Recession reduces high-value cargo
   └── Random: Supply chain disruption, factory orders

4. PLAYER REPUTATION
   ├── High reputation: Access to premium contracts
   ├── Specialized: DG-certified gets DG jobs offered
   └── Reliability: Express jobs offered to on-time pilots
```

### Job Properties

```
JOB STRUCTURE:

📋 JOB ENTITY
├── Identity
│   ├── JobId (GUID)
│   ├── JobNumber (human-readable: "JOB-2024-123456")
│   └── JobType (Cargo, Passenger, Charter, Emergency)
│
├── Route
│   ├── OriginAirportId
│   ├── DestinationAirportId
│   ├── DistanceNm (calculated)
│   └── EstimatedFlightTimeMin
│
├── Cargo Details
│   ├── CargoTypeId
│   ├── WeightKg
│   ├── VolumeM3 (for space-limited loads)
│   ├── Quantity (for countable items)
│   └── SpecialHandling (flags)
│
├── Timing
│   ├── CreatedAt (when job appeared)
│   ├── ExpiresAt (job disappears - typically 2-24 hours)
│   ├── PickupDeadline (must collect by this time)
│   ├── DeliveryDeadline (must deliver by this time)
│   └── Priority (Standard, Priority, Express, Urgent, Critical)
│
├── Risk & Requirements
│   ├── RiskLevel (1-5)
│   ├── RequiredLicenses[] (license codes needed)
│   ├── MinimumFlightHours
│   ├── MinimumAircraftMtow
│   └── RequiredEquipment[] (de-icing, cargo door, etc.)
│
├── Financial
│   ├── BasePayout
│   ├── DistanceBonus
│   ├── TimeBonus (on-time delivery)
│   ├── EarlyBonus (ahead of schedule)
│   ├── ConditionBonus (cargo arrives undamaged)
│   ├── LatePenaltyPerHour
│   ├── DamagePenaltyPercent
│   └── TotalEstimatedPayout
│
├── Status
│   ├── Status (Available, Accepted, InTransit, Delivered, Failed, Expired)
│   ├── AcceptedByUserId
│   ├── AcceptedAt
│   ├── AssignedFlightId
│   └── CompletedAt
│
└── Metadata
    ├── ClientName (fictional: "Acme Corp", "GlobalMed")
    ├── ClientReputation (affects future job flow)
    └── Notes
```

### Risk Levels & License Requirements

```
RISK LEVELS:

⭐ LEVEL 1 - STANDARD
├── Requirements: CPL + appropriate aircraft rating
├── Cargo: General goods, mail, non-perishable
├── Routes: Major airports, good weather routes
├── Pay Multiplier: ×1.0
└── Example: "500kg mail, KLAX → KLAS, $8,000"

⭐⭐ LEVEL 2 - PRIORITY
├── Requirements: Level 1 + 100 hours + good on-time record
├── Cargo: Time-sensitive, perishables, express
├── Routes: Regional, moderate complexity
├── Pay Multiplier: ×1.3
└── Example: "200kg pharmaceuticals, 12hr deadline, $15,000"

⭐⭐⭐ LEVEL 3 - SPECIALIZED
├── Requirements: Level 2 + specific endorsement
├── Cargo: Dangerous goods, live animals, high-value
├── Routes: May include challenging airports
├── Pay Multiplier: ×1.6
├── Endorsements needed:
│   ├── DG License (dangerous goods)
│   ├── Live Cargo (animals)
│   ├── High Value Transport (precious metals, art)
│   └── Medical Transport (organs, emergency)
└── Example: "50kg hazmat chemicals, DG required, $25,000"

⭐⭐⭐⭐ LEVEL 4 - COMPLEX
├── Requirements: Level 3 + 500 hours + IR/Night as needed
├── Cargo: Complex operations, difficult timing
├── Routes: Remote airports, night ops, IFR required
├── Pay Multiplier: ×2.0
├── Additional requirements:
│   ├── Instrument Rating (IFR routes)
│   ├── Night Rating (night deliveries)
│   ├── Mountain Flying (high altitude airports)
│   └── Bush Flying (unimproved strips)
└── Example: "Emergency supplies to remote strip, night, $80,000"

⭐⭐⭐⭐⭐ LEVEL 5 - CRITICAL
├── Requirements: Level 4 + 1000 hours + excellent reputation
├── Cargo: VIP, government, medical emergency, time-critical
├── Routes: Any, often complex multi-leg
├── Pay Multiplier: ×3.0
├── Special access required:
│   ├── Security clearance (government contracts)
│   ├── VIP endorsement (executive transport)
│   ├── Excellent safety record (no incidents)
│   └── Reputation score 4.5+ stars
└── Example: "Organ transport, 4hr deadline, $150,000"
```

### Multi-Job Flights

Aircraft can accept multiple jobs for the same flight:

```
MULTI-JOB SYSTEM:

1. COMBINING JOBS
   ├── Same destination: Multiple jobs to same airport
   ├── En-route stops: Jobs along the way (multi-leg)
   ├── Weight limit: Total cargo ≤ aircraft payload
   ├── Volume limit: Total volume ≤ cargo space
   └── Compatibility: Some cargo can't mix (DG rules)

2. CARGO COMPATIBILITY MATRIX
   ├── ✅ Mail + Consumer Goods (compatible)
   ├── ✅ Industrial + Raw Materials (compatible)
   ├── ⚠️ Food + Chemicals (requires separation)
   ├── ❌ Explosives + Flammables (prohibited)
   ├── ❌ Animals + Strong odors (prohibited)
   └── ⚠️ High Value + General (security concerns)

3. DEADLINE MANAGEMENT
   ├── Each job keeps its own deadline
   ├── Plan route to meet all deadlines
   ├── Failure on one job doesn't affect others
   └── Bonus if all jobs delivered on-time

4. EXAMPLE MULTI-JOB FLIGHT:

   Aircraft: Cessna 421C (800kg payload)
   Route: KLAX → KLAS (236 NM)

   Job 1: Mail (200kg) - $6,000 - Deadline: 8 hours
   Job 2: Electronics (300kg) - $35,000 - Deadline: 6 hours
   Job 3: Pharmaceuticals (150kg) - $22,000 - Deadline: 4 hours

   Total Cargo: 650kg (within 800kg limit)
   Total Payout: $63,000

   Priority: Deliver by Hour 4 to meet all deadlines
```

### Job Expiry System

```
JOB EXPIRY:

EXPIRY TIMEFRAMES (from job creation):
├── Critical jobs: 1-2 hours (take it or leave it)
├── Urgent jobs: 2-6 hours
├── Express jobs: 6-12 hours
├── Priority jobs: 12-24 hours
├── Standard jobs: 24-48 hours
└── Bulk contracts: 48-72 hours

WHAT HAPPENS ON EXPIRY:
├── Job removed from board
├── May reappear later (client reposted)
├── Affects airport job supply temporarily
└── No penalty to player

ACCEPTING A JOB:
├── Job reserved for player
├── Pickup deadline starts
├── Can cancel within 1 hour (no penalty)
├── Cancel after 1 hour: Reputation hit
└── Fail to pickup: Reputation hit + possible fine
```

### Job Entity Design

```csharp
public class Job
{
    public Guid Id { get; set; }
    public string JobNumber { get; set; }          // "JOB-2024-123456"

    // Route
    public int OriginAirportId { get; set; }
    public Airport OriginAirport { get; set; }
    public int DestinationAirportId { get; set; }
    public Airport DestinationAirport { get; set; }
    public int DistanceNm { get; set; }

    // Cargo
    public Guid CargoTypeId { get; set; }
    public CargoType CargoType { get; set; }
    public int WeightKg { get; set; }
    public decimal VolumeM3 { get; set; }

    // Timing
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? PickupDeadline { get; set; }
    public DateTime? DeliveryDeadline { get; set; }
    public JobPriority Priority { get; set; }

    // Requirements
    public int RiskLevel { get; set; }             // 1-5
    public string[] RequiredLicenseCodes { get; set; }
    public int MinFlightHours { get; set; }
    public int MinAircraftMtowKg { get; set; }

    // Financial
    public decimal BasePayout { get; set; }
    public decimal OnTimeBonus { get; set; }
    public decimal EarlyBonusPerHour { get; set; }
    public decimal LatePenaltyPerHour { get; set; }
    public decimal DamagePenaltyPercent { get; set; }

    // Status
    public JobStatus Status { get; set; }
    public Guid? AcceptedByUserId { get; set; }
    public User? AcceptedBy { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public Guid? AssignedFlightId { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Client (for immersion)
    public string ClientName { get; set; }
    public string? Notes { get; set; }
}

public enum JobStatus
{
    Available,
    Accepted,
    PickedUp,
    InTransit,
    Delivered,
    Failed,
    Expired,
    Cancelled
}

public enum JobPriority
{
    Standard,      // 24-48 hr expiry
    Priority,      // 12-24 hr expiry
    Express,       // 6-12 hr expiry
    Urgent,        // 2-6 hr expiry
    Critical       // 1-2 hr expiry
}

// Junction table for multi-job flights
public class FlightJob
{
    public Guid Id { get; set; }
    public Guid FlightId { get; set; }
    public TrackedFlight Flight { get; set; }
    public Guid JobId { get; set; }
    public Job Job { get; set; }

    // Per-job tracking within flight
    public decimal CargoConditionPercent { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public decimal FinalPayout { get; set; }
    public string? DeliveryNotes { get; set; }
}
```

---

## 1.6 Aircraft Marketplace

### Overview

Aircraft are purchased from dealers located at airports. Different dealer types offer different inventory, pricing, and services.

### Dealer Types

```
AIRCRAFT DEALERS:

🏭 MANUFACTURER DEALERSHIP
├── Profile: Official authorized dealer
├── Inventory: New aircraft only
├── Condition: 100% (brand new)
├── Warranty: Full manufacturer warranty (2 years)
├── Pricing: MSRP (full price)
├── Discounts:
│   ├── Loyalty (repeat buyer): 5%
│   ├── Fleet purchase (3+): 8%
│   └── Seasonal promotions: 5-15%
├── Financing: Factory financing available (best rates)
├── Locations: Major airports only
└── Examples: "Cessna Aviation Center", "Airbus Sales & Leasing"

🔧 CERTIFIED PRE-OWNED DEALER
├── Profile: Quality used aircraft specialist
├── Inventory: Used aircraft (80-95% condition)
├── Condition: Inspected and certified
├── Warranty: Limited warranty (6 months)
├── Pricing: 60-85% of new price
├── Discounts:
│   ├── Cash purchase: 3%
│   ├── Quick sale items: 10-20%
│   └── As-is specials: 25%+
├── Financing: Partner banks available
├── Locations: Medium to large airports
└── Examples: "SkyTrade Aviation", "Executive Aircraft Sales"

🏪 BUDGET AIRCRAFT LOT
├── Profile: Affordable used aircraft
├── Inventory: Older/higher-hour aircraft (60-80% condition)
├── Condition: Flyable, may need work
├── Warranty: None (as-is)
├── Pricing: 30-60% of new price
├── Discounts:
│   ├── Volume: 5%
│   └── Project aircraft: Additional 20%
├── Financing: Limited (high-risk lenders only)
├── Locations: Small to medium airports
└── Examples: "Bargain Wings", "Affordable Aviation"

🏛️ AUCTION HOUSE
├── Profile: Player-to-player and repossession sales
├── Inventory: Player aircraft, bank repos, estate sales
├── Condition: Varies widely (40-100%)
├── Warranty: None
├── Pricing: Market-driven (bidding)
├── Fees:
│   ├── Buyer premium: 5%
│   └── Seller commission: 8%
├── Types:
│   ├── Live auction (timed bidding)
│   ├── Buy-it-now (fixed price)
│   └── Dutch auction (price drops)
├── Locations: Virtual (accessible anywhere)
└── Examples: "Aviation Auctions International"

✈️ SPECIALTY DEALER
├── Profile: Niche aircraft specialist
├── Inventory: Specific types (warbirds, bush planes, etc.)
├── Condition: Varies, often restored
├── Pricing: Premium for rare aircraft
├── Types:
│   ├── Warbirds & Vintage
│   ├── Bush/STOL Specialists
│   ├── Aerobatic Aircraft
│   └── Cargo Conversions
├── Locations: Specific regions
└── Examples: "Heritage Aviation", "Bush Pilot Supply"
```

### Base Aircraft Pricing Formula

```
AIRCRAFT BASE PRICE CALCULATION:

BASE_PRICE = (
    (MTOW_kg × $400) +                      // Size factor
    (CRUISE_TAS_kts × $40,000) +            // Speed factor
    (RANGE_nm × $800) +                     // Range factor
    (PASSENGER_CAPACITY × $80,000) +        // Revenue potential
    (ENGINE_COUNT × ENGINE_FACTOR)          // Propulsion
) × CATEGORY_MULTIPLIER × MARKET_ADJUSTMENT

ENGINE_FACTOR:
├── Piston: $200,000 per engine
├── Turboprop: $1,500,000 per engine
├── Turbojet: $3,000,000 per engine
└── Turbofan: $5,000,000 per engine

CATEGORY_MULTIPLIER:
├── Light Sport: 0.15
├── Single Piston: 0.25
├── Twin Piston: 0.40
├── Single Turboprop: 0.70
├── Twin Turboprop: 0.85
├── Light Jet: 1.0
├── Regional Jet: 1.3
├── Narrow Body: 2.0
└── Wide Body: 3.0

MARKET_ADJUSTMENT:
├── High demand model: ×1.1 to ×1.3
├── Standard model: ×1.0
├── Discontinued: ×0.8 to ×0.9
└── Overproduced: ×0.85

EXAMPLE CALCULATIONS:

Cessna 172:
├── MTOW: 1,111kg × $400 = $444,400
├── Cruise: 122kts × $40,000 = $4,880,000
├── Range: 640nm × $800 = $512,000
├── Passengers: 4 × $80,000 = $320,000
├── Engines: 1 × $200,000 = $200,000
├── Subtotal: $6,356,400
├── Category: ×0.25 (Single Piston)
├── Base Price: $1,589,100
├── Market: ×0.22 (very common)
└── FINAL: ~$350,000 ✓

A320-200:
├── MTOW: 78,000kg × $400 = $31,200,000
├── Cruise: 450kts × $40,000 = $18,000,000
├── Range: 3,300nm × $800 = $2,640,000
├── Passengers: 180 × $80,000 = $14,400,000
├── Engines: 2 × $5,000,000 = $10,000,000
├── Subtotal: $76,240,000
├── Category: ×2.0 (Narrow Body)
├── Base Price: $152,480,000
├── Market: ×0.64 (common, competitive)
└── FINAL: ~$98,000,000 ✓
```

### Condition & Hours Adjustments

```
USED AIRCRAFT PRICING:

CONDITION ADJUSTMENT (% of base price):
├── 100%: 100% (like new)
├── 95%: 95%
├── 90%: 88%
├── 85%: 80%
├── 80%: 70%
├── 75%: 60%
├── 70%: 50%
├── 65%: 40%
├── 60%: 32%
└── <60%: 25% (project aircraft)

HOURS ADJUSTMENT (additional reduction):
├── 0-500 hours: 0%
├── 500-2,000 hours: -3%
├── 2,000-5,000 hours: -8%
├── 5,000-10,000 hours: -15%
├── 10,000-20,000 hours: -22%
├── 20,000-30,000 hours: -30%
└── 30,000+ hours: -40%

AGE ADJUSTMENT:
├── 0-5 years: 0%
├── 5-10 years: -5%
├── 10-20 years: -12%
├── 20-30 years: -20%
└── 30+ years: -30% (but may be vintage premium)

EXAMPLE USED AIRCRAFT:

Cessna 421C:
├── Base new price: $1,200,000
├── Condition: 82% → ×0.74
├── Hours: 4,500 → -8%
├── Age: 15 years → -12%
├── Subtotal: $1,200,000 × 0.74 = $888,000
├── Hours: $888,000 × 0.92 = $816,960
├── Age: $816,960 × 0.88 = $718,925
└── FINAL: ~$720,000
```

### Sales & Discounts

```
DISCOUNT TYPES:

🏷️ SEASONAL SALES
├── Spring Sale (March): 10% off training aircraft
├── Summer Sale (July): 15% off touring aircraft
├── Black Friday (November): 20% off select models
├── End of Year (December): 25% off remaining inventory
└── New Year (January): Financing specials

🔥 CLEARANCE
├── Model year changeover: 15-25% off outgoing
├── Overstock: 10-20% off
├── Demo aircraft: 10-15% off (low hours, full warranty)
└── Damaged/repaired: 30-50% off (disclosure required)

⭐ LOYALTY PROGRAMS
├── First purchase: 0%
├── Second purchase: 3% off
├── Third purchase: 5% off
├── Fleet buyer (5+ aircraft): 10% off
└── Manufacturer loyalty: 5% if same brand

💳 FINANCING SPECIALS
├── 0% APR for 12 months (on approved credit)
├── Low down payment: 5% down (higher rate)
├── Deferred payments: No payments for 6 months
└── Trade-in bonus: 110% trade value

🎁 BUNDLE DEALS
├── Aircraft + Type Rating: 15% off training
├── Aircraft + Insurance (1 year): 20% off insurance
├── Aircraft + Maintenance package: Free first service
└── Aircraft + Avionics upgrade: Cost price on upgrade
```

### Aircraft at Airports

```
AIRPORT INVENTORY SYSTEM:

INVENTORY BY AIRPORT SIZE:
├── Large Hub: 20-50 aircraft available
│   ├── All categories represented
│   ├── Multiple dealers present
│   └── Includes airliners
├── Medium Airport: 10-25 aircraft
│   ├── GA and small commercial
│   ├── 1-2 dealers
│   └── Up to regional jets
├── Small Airport: 3-10 aircraft
│   ├── Single-engine dominant
│   ├── Usually one dealer
│   └── Occasional twin
└── Remote/Bush: 0-3 aircraft
    ├── Bush planes only
    ├── Private sales common
    └── Specialty dealer occasional

INVENTORY REFRESH:
├── New inventory: Weekly
├── Price changes: Daily (market driven)
├── Sales events: Monthly
└── Seasonal specials: Quarterly

REGIONAL PREFERENCES:
├── Alaska: Bush planes, floats common
├── Florida: Training aircraft heavy
├── Texas: Turboprops, ranch aircraft
├── Europe: Efficient twins, citations
└── Middle East: Luxury, large jets
```

### Dealer Entity Design

```csharp
public class AircraftDealer
{
    public Guid Id { get; set; }

    // Identity
    public string Name { get; set; }              // "Cessna Aviation Center"
    public DealerType Type { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }

    // Location
    public int AirportId { get; set; }
    public Airport Airport { get; set; }

    // Inventory Focus
    public string[]? ManufacturerFocus { get; set; }  // ["Cessna", "Beechcraft"]
    public AircraftCategory[]? CategoryFocus { get; set; }
    public int MinConditionPercent { get; set; }     // 80 = only 80%+ aircraft

    // Pricing
    public decimal PriceMultiplier { get; set; }     // 1.0 = standard, 0.9 = 10% off
    public bool OffersFinancing { get; set; }
    public decimal FinancingRateModifier { get; set; } // -1% = 1% better rate

    // Reputation
    public decimal Reputation { get; set; }          // 0-5 stars
    public int TotalSales { get; set; }

    // Status
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<DealerInventory> Inventory { get; set; }
    public ICollection<DealerDiscount> ActiveDiscounts { get; set; }
}

public class DealerInventory
{
    public Guid Id { get; set; }

    public Guid DealerId { get; set; }
    public AircraftDealer Dealer { get; set; }

    public Guid AircraftTemplateId { get; set; }
    public AircraftTemplate Template { get; set; }

    // Specific Aircraft Details
    public string? Registration { get; set; }        // For used aircraft
    public int ConditionPercent { get; set; }
    public int TotalFlightHours { get; set; }
    public int AircraftAgeYears { get; set; }

    // Pricing
    public decimal ListPrice { get; set; }           // Calculated base
    public decimal? SalePrice { get; set; }          // If on sale
    public Guid? ActiveDiscountId { get; set; }

    // Availability
    public int QuantityAvailable { get; set; }       // For new aircraft
    public bool IsAvailable { get; set; }
    public DateTime ListedAt { get; set; }
    public DateTime? SoldAt { get; set; }

    // Details
    public string? Description { get; set; }
    public string[]? Features { get; set; }          // ["Garmin G1000", "Leather Interior"]
    public string[]? ImageUrls { get; set; }
}

public class DealerDiscount
{
    public Guid Id { get; set; }

    public Guid DealerId { get; set; }
    public AircraftDealer Dealer { get; set; }

    public string Name { get; set; }                 // "Summer Sale"
    public DiscountType Type { get; set; }
    public decimal DiscountPercent { get; set; }     // 0.15 = 15% off

    // Scope
    public Guid[]? ApplicableTemplateIds { get; set; }  // Null = all aircraft
    public string[]? ApplicableManufacturers { get; set; }

    // Timing
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsActive { get; set; }

    // Limits
    public int? MaxUses { get; set; }
    public int TimesUsed { get; set; }
}

public enum DealerType
{
    ManufacturerDealer,
    CertifiedPreOwned,
    BudgetLot,
    AuctionHouse,
    SpecialtyDealer
}

public enum DiscountType
{
    Seasonal,
    Clearance,
    Loyalty,
    Financing,
    Bundle
}
```

---

## 1.7 License Shop

### Overview

The License Shop is where players purchase, renew, and upgrade their licenses. Each license has prerequisites, costs, and validity periods.

### License Shop Structure

```
LICENSE SHOP:

📜 PILOT LICENSES
├── Student Pilot License (SPL)
│   ├── Price: $2,500
│   ├── Prerequisites: 10 flight hours
│   ├── Exam: Written (multiple choice)
│   ├── Validity: Until PPL obtained
│   └── Allows: Solo training flights
│
├── Private Pilot License (PPL)
│   ├── Price: $12,000
│   ├── Prerequisites: SPL, 40 flight hours
│   ├── Exam: Written + Practical checkride
│   ├── Validity: 24 months
│   ├── Renewal: $3,000 + checkride
│   └── Allows: Non-commercial flights
│
├── Commercial Pilot License (CPL)
│   ├── Price: $35,000
│   ├── Prerequisites: PPL, IR, 200 flight hours
│   ├── Exam: Written + Practical checkride
│   ├── Validity: 12 months
│   ├── Renewal: $8,000 + checkride
│   └── Allows: Commercial operations
│
└── Airline Transport Pilot License (ATPL)
    ├── Price: $75,000
    ├── Prerequisites: CPL, 1500 flight hours
    ├── Exam: Written + Practical + Simulator
    ├── Validity: 12 months
    ├── Renewal: $15,000 + checkride
    └── Allows: Airline captain operations

✈️ AIRCRAFT RATINGS
├── Single Engine Piston (SEP)
│   ├── Price: $5,000
│   ├── Prerequisites: PPL
│   └── Aircraft: C172, C182, PA-28, etc.
│
├── Multi Engine Piston (MEP)
│   ├── Price: $12,000
│   ├── Prerequisites: PPL, 70 total hours
│   └── Aircraft: C421, PA-34, BE58, etc.
│
├── Single Engine Turbine (SET)
│   ├── Price: $25,000
│   ├── Prerequisites: PPL, IR, 150 hours
│   └── Aircraft: TBM, PC-12, etc.
│
└── Multi Engine Turbine (MET)
    ├── Price: $40,000
    ├── Prerequisites: CPL, IR, 300 hours
    └── Aircraft: King Air, jets, etc.

🎯 TYPE RATINGS (Per aircraft family)
├── Complex Single (C210, BE36)
│   ├── Price: $8,000
│   └── Requires: SEP
│
├── Light Jet (Citation, Phenom)
│   ├── Price: $35,000
│   └── Requires: MET, 500 hours
│
├── Regional Jet (CRJ, ERJ)
│   ├── Price: $50,000
│   └── Requires: MET, 750 hours
│
├── Narrow Body (A320 Family)
│   ├── Price: $75,000
│   └── Requires: MET, ATPL, 1000 hours
│
├── Narrow Body (B737 Family)
│   ├── Price: $70,000
│   └── Requires: MET, ATPL, 1000 hours
│
└── Wide Body (A350, B777, B787)
    ├── Price: $150,000
    └── Requires: ATPL, 2000 hours, narrow body type

🌙 OPERATIONAL RATINGS
├── Instrument Rating (IR)
│   ├── Price: $18,000
│   ├── Prerequisites: PPL, 50 XC hours
│   └── Allows: IFR flight
│
├── Night Rating
│   ├── Price: $8,000
│   ├── Prerequisites: PPL, 5 night hours
│   └── Allows: Night VFR
│
├── Aerobatic Rating
│   ├── Price: $6,000
│   ├── Prerequisites: PPL
│   └── Allows: Aerobatic maneuvers
│
└── Mountain Flying
    ├── Price: $10,000
    ├── Prerequisites: PPL, IR recommended
    └── Allows: High-altitude airports

⚠️ SPECIAL ENDORSEMENTS
├── Dangerous Goods (DG)
│   ├── Price: $15,000
│   ├── Prerequisites: CPL, 100 cargo hours
│   ├── Validity: 24 months
│   └── Allows: Hazmat cargo
│
├── Live Cargo
│   ├── Price: $8,000
│   ├── Prerequisites: CPL
│   └── Allows: Animal transport
│
├── High Value Transport
│   ├── Price: $12,000
│   ├── Prerequisites: CPL, clean record
│   └── Allows: Precious cargo
│
├── Medical Transport
│   ├── Price: $20,000
│   ├── Prerequisites: CPL, IR
│   └── Allows: Organ/emergency medical
│
├── ETOPS (Extended Twin Ops)
│   ├── Price: $25,000
│   ├── Prerequisites: ATPL, MET
│   └── Allows: Extended overwater
│
├── RVSM (Reduced Vertical Sep)
│   ├── Price: $15,000
│   ├── Prerequisites: IR, FL290+ capable
│   └── Allows: FL290-FL410 ops
│
└── CAT II/III Approaches
    ├── Price: $30,000
    ├── Prerequisites: IR, 500 IFR hours
    └── Allows: Low visibility landings

🔧 MAINTENANCE LICENSES
├── Category A (Line)
│   ├── Price: $20,000
│   ├── Prerequisites: None
│   └── Allows: Basic maintenance
│
├── Category B1 (Mechanical)
│   ├── Price: $50,000
│   ├── Prerequisites: Cat A, 2 years exp
│   └── Allows: Engine, structure work
│
├── Category B2 (Avionics)
│   ├── Price: $50,000
│   ├── Prerequisites: Cat A, 2 years exp
│   └── Allows: Electrical, avionics
│
└── Category C (Base)
    ├── Price: $100,000
    ├── Prerequisites: Cat B1 or B2, 5 years
    └── Allows: Major overhauls
```

### License Entity Updates

```csharp
public class LicenseShopItem
{
    public Guid Id { get; set; }

    public Guid LicenseTypeId { get; set; }
    public LicenseType LicenseType { get; set; }

    // Pricing
    public decimal BasePrice { get; set; }
    public decimal RenewalPrice { get; set; }
    public decimal? ExamFee { get; set; }
    public decimal? PracticalTestFee { get; set; }

    // Discounts
    public decimal? CurrentDiscount { get; set; }
    public DateTime? DiscountEndsAt { get; set; }

    // Availability
    public bool IsAvailable { get; set; }
    public int? StockLimit { get; set; }           // For limited training spots
}
```

---

## 1.8 Player Auction System

### Overview

Players can sell their owned aircraft to other players through an auction system. This creates a player-driven market for used aircraft.

### Auction Types

```
AUCTION FORMATS:

📈 ENGLISH AUCTION (Standard - Ascending)
├── How it works:
│   ├── Seller sets starting bid
│   ├── Bidders increase price
│   ├── Highest bid wins
│   └── Extends if bid in final minutes
├── Duration: 1-7 days
├── Best for: Desirable aircraft, fair market value
└── Most common format

📉 DUTCH AUCTION (Descending)
├── How it works:
│   ├── Starts at high price
│   ├── Price drops every hour
│   ├── First buyer wins at current price
│   └── Ends if price hits minimum
├── Duration: 1-3 days
├── Best for: Quick sales, clearance
└── Risk: May sell low if no early buyer

📝 SEALED BID AUCTION
├── How it works:
│   ├── All bids submitted blind
│   ├── Highest bid revealed at end
│   ├── Winner pays their bid
│   └── No bid modification
├── Duration: 24-48 hours
├── Best for: Rare aircraft, preventing sniping
└── Creates urgency to bid high

💰 BUY-IT-NOW (Fixed Price)
├── How it works:
│   ├── Seller sets fixed price
│   ├── First buyer wins instantly
│   ├── Can include "Make Offer" option
│   └── No bidding war
├── Duration: Until sold or 30 days
├── Best for: Quick transactions, known value
└── Simple and predictable
```

### Listing Requirements

```
LISTING AN AIRCRAFT:

1. OWNERSHIP REQUIREMENTS
   ├── Must own aircraft outright
   ├── No active loans on aircraft
   │   └── Or: Pay off loan first
   ├── Aircraft not assigned to AI routes
   └── Aircraft not in maintenance

2. AIRCRAFT PREPARATION
   ├── Aircraft grounded during auction
   ├── Cannot fly during listing
   ├── Location locked (where listed)
   └── Condition locked (recorded at listing)

3. LISTING DETAILS
   ├── Starting Bid (minimum)
   ├── Reserve Price (optional, hidden)
   │   └── Auction fails if not met
   ├── Buy-It-Now Price (optional)
   ├── Auction Duration (1-7 days)
   └── Condition Report (required)

4. FEES
   ├── Listing Fee: 0.5% of starting bid
   │   └── Non-refundable, paid upfront
   ├── Success Fee: 5% of final sale
   │   └── Deducted from proceeds
   └── Relisting Fee: 0.25% (if unsold)
```

### Bidding Rules

```
BIDDING SYSTEM:

PLACING BIDS:
├── Minimum first bid: Starting price
├── Minimum increment: Greater of:
│   ├── 2% of current bid
│   └── $5,000
├── Bid holds funds in escrow
├── Outbid releases held funds
└── Maximum bid: Can set auto-bid limit

PROXY BIDDING:
├── Set maximum you're willing to pay
├── System bids minimum needed to win
├── Auto-increases up to your max
└── Prevents constant monitoring

SNIPING PROTECTION:
├── Bid in last 5 minutes
├── Auction extends by 10 minutes
├── Maximum 3 extensions per auction
└── Encourages fair bidding

BID LIMITS:
├── Maximum 50 bids per user per auction
├── Must have funds available
├── Verified account required
└── Reputation minimum for high-value (>$1M)
```

### Auction Fees Structure

```
FEE BREAKDOWN:

SELLER FEES:
├── Listing Fee: 0.5% of starting bid
│   ├── Paid when listing created
│   ├── Non-refundable
│   └── Minimum: $500
│
├── Success Fee: 5% of final sale price
│   ├── Deducted from proceeds
│   └── Maximum: $500,000
│
├── Relisting Fee: 0.25% (if unsold)
│   └── Can choose to relist or withdraw
│
└── Reserve Not Met:
    ├── Can accept highest bid
    ├── Or decline (no additional fee)
    └── Aircraft returns to inventory

BUYER FEES:
├── Buyer Premium: 3% of winning bid
│   ├── Added to final price
│   └── Maximum: $150,000
│
├── Transfer Fee: $1,500 flat
│   └── Covers registration transfer
│
└── Ferry Cost (optional):
    └── If buying remote aircraft

EXAMPLE SALE:
Aircraft sells for $2,000,000

Seller:
├── Listing fee (paid earlier): $10,000
├── Success fee: $100,000
└── Net received: $1,890,000

Buyer:
├── Winning bid: $2,000,000
├── Buyer premium: $60,000
├── Transfer fee: $1,500
└── Total paid: $2,061,500

Platform revenue: $171,500
```

### Auction Entity Design

```csharp
public class Auction
{
    public Guid Id { get; set; }
    public string AuctionNumber { get; set; }        // "AUC-2024-12345"

    // Seller
    public Guid SellerId { get; set; }
    public User Seller { get; set; }

    // Aircraft
    public Guid AircraftId { get; set; }
    public OwnedAircraft Aircraft { get; set; }

    // Auction Type
    public AuctionType Type { get; set; }
    public AuctionStatus Status { get; set; }

    // Pricing
    public decimal StartingBid { get; set; }
    public decimal? ReservePrice { get; set; }       // Hidden minimum
    public decimal? BuyItNowPrice { get; set; }
    public decimal CurrentBid { get; set; }
    public int BidCount { get; set; }

    // Timing
    public DateTime CreatedAt { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public DateTime? ExtendedUntil { get; set; }
    public int ExtensionCount { get; set; }

    // Winner
    public Guid? WinningBidId { get; set; }
    public AuctionBid? WinningBid { get; set; }
    public Guid? WinnerId { get; set; }
    public User? Winner { get; set; }

    // Fees
    public decimal ListingFeePaid { get; set; }
    public decimal? SuccessFeePaid { get; set; }
    public decimal? BuyerPremiumPaid { get; set; }

    // Details
    public string Title { get; set; }
    public string? Description { get; set; }
    public string[]? ImageUrls { get; set; }

    // Aircraft Snapshot (at listing time)
    public int ConditionAtListing { get; set; }
    public int HoursAtListing { get; set; }
    public int AirportIdAtListing { get; set; }

    // Navigation
    public ICollection<AuctionBid> Bids { get; set; }
}

public class AuctionBid
{
    public Guid Id { get; set; }

    public Guid AuctionId { get; set; }
    public Auction Auction { get; set; }

    public Guid BidderId { get; set; }
    public User Bidder { get; set; }

    public decimal BidAmount { get; set; }
    public decimal? MaxBidAmount { get; set; }       // For proxy bidding
    public DateTime BidAt { get; set; }

    public BidStatus Status { get; set; }
    public bool IsWinning { get; set; }

    // Escrow
    public decimal AmountHeld { get; set; }
    public bool FundsReleased { get; set; }
}

public enum AuctionType
{
    English,        // Ascending bids
    Dutch,          // Descending price
    SealedBid,      // Blind bidding
    BuyItNow        // Fixed price
}

public enum AuctionStatus
{
    Draft,
    Pending,        // Awaiting start time
    Active,
    Extended,       // Sniping protection active
    Ended,
    Sold,
    Unsold,         // Reserve not met / no bids
    Cancelled
}

public enum BidStatus
{
    Active,
    Outbid,
    Winning,
    Won,
    Lost,
    Retracted       // By admin only
}
```

---

## 1.9 Worker System

### Overview

Workers are employees you hire to run your operations. They include pilots (AI crew), cabin crew, ground crew, mechanics, and administrative staff.

### Worker Types

```
WORKER CATEGORIES:

👨‍✈️ FLIGHT CREW
├── Already defined in AI Crew System (Section 9)
├── Types: Private Pilot → Captain
├── Salaries: $3,000 - $25,000/month
└── Required for: AI-operated flights

👩‍💼 CABIN CREW
├── Flight Attendant: $2,500/month
├── Senior FA: $4,000/month
├── Purser: $6,000/month
└── Required for: Passenger AI flights

🔧 MAINTENANCE CREW
├── Line Mechanic (Cat A): $4,000/month
│   ├── Performs: Pre-flight checks, minor repairs
│   └── Reduces: Inspection time -20%
│
├── Certified Mechanic (Cat B1/B2): $7,000/month
│   ├── Performs: Component repairs, avionics
│   └── Reduces: Repair costs -10%
│
├── Senior Mechanic (Cat C): $12,000/month
│   ├── Performs: Major overhauls
│   └── Allows: In-house maintenance
│
└── Benefits:
    ├── Reduce outsourced maintenance costs
    ├── Faster turnaround times
    └── Required for workshops

📋 OPERATIONS STAFF
├── Dispatcher: $4,500/month
│   ├── Effect: Better route optimization
│   ├── Effect: -15% fuel costs
│   └── Effect: Weather avoidance planning
│
├── Load Master: $3,500/month
│   ├── Effect: -20% cargo loading time
│   ├── Effect: Better weight distribution
│   └── Effect: +5% cargo condition
│
├── Customer Service Rep: $3,000/month
│   ├── Effect: +10% passenger satisfaction
│   ├── Effect: Handles complaints
│   └── Effect: Repeat bookings
│
└── Accountant: $5,000/month
    ├── Effect: Tax optimization (-5% fees)
    ├── Effect: Loan rate improvement
    └── Effect: Financial reporting

🏢 MANAGEMENT
├── Operations Manager: $10,000/month
│   ├── Required for: 5+ employees
│   ├── Effect: Employee efficiency +10%
│   └── Effect: Coordination bonuses
│
├── Chief Pilot: $15,000/month
│   ├── Required for: 3+ pilots
│   ├── Effect: Pilot training discount
│   └── Effect: Safety record improvement
│
└── CFO (Chief Financial Officer): $20,000/month
    ├── Required for: $10M+ operations
    ├── Effect: Better financing terms
    └── Effect: Investment opportunities
```

### Worker Attributes

```
WORKER PROPERTIES:

SKILL RATING (1-5 stars):
├── Determines effectiveness
├── Higher skill = better performance
├── Skill grows with experience
└── Affects salary expectations

EXPERIENCE:
├── Time employed
├── Tasks completed
├── Affects promotions
└── Reduces incident rates

MORALE (0-100):
├── Affected by:
│   ├── Pay vs market rate
│   ├── Work hours
│   ├── Equipment quality
│   └── Management style
├── Low morale effects:
│   ├── Higher incident rate
│   ├── May quit
│   └── Reduced efficiency
└── High morale effects:
    ├── Bonus performance
    ├── Loyalty (won't poach)
    └── Trains others faster

TRAITS (Random):
├── Positive:
│   ├── Efficient: -10% time on tasks
│   ├── Careful: -5% incident rate
│   ├── Leadership: +10% team performance
│   ├── Experienced: +1 effective skill
│   └── Loyal: Reduced poaching risk
└── Negative:
    ├── Slow: +15% time on tasks
    ├── Careless: +10% incident rate
    ├── Loner: No team bonus
    ├── Greedy: Demands raises
    └── Unreliable: May miss work
```

### Worker Entity Design

```csharp
public class Worker
{
    public Guid Id { get; set; }

    // Employment
    public Guid EmployerId { get; set; }
    public User Employer { get; set; }
    public WorkerType Type { get; set; }
    public WorkerStatus Status { get; set; }

    // Identity (generated)
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? PhotoUrl { get; set; }

    // Skills
    public int SkillRating { get; set; }           // 1-5
    public int Experience { get; set; }            // Months employed
    public int TasksCompleted { get; set; }

    // For Pilots
    public string[]? LicenseCodes { get; set; }
    public int FlightHours { get; set; }
    public string[]? TypeRatings { get; set; }

    // Employment Terms
    public decimal MonthlySalary { get; set; }
    public DateTime HiredAt { get; set; }
    public DateTime? ContractEndsAt { get; set; }
    public int ContractMonths { get; set; }

    // Performance
    public int Morale { get; set; }                // 0-100
    public decimal PerformanceRating { get; set; } // 0-5
    public int IncidentCount { get; set; }

    // Traits
    public WorkerTrait[]? Traits { get; set; }

    // Assignment
    public Guid? AssignedAircraftId { get; set; }
    public Guid? AssignedRouteId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum WorkerType
{
    // Flight Crew
    PrivatePilot,
    CommercialPilot,
    AirlinePilot,
    Captain,

    // Cabin
    FlightAttendant,
    SeniorFlightAttendant,
    Purser,

    // Maintenance
    LineMechanic,
    CertifiedMechanic,
    SeniorMechanic,

    // Operations
    Dispatcher,
    LoadMaster,
    CustomerService,
    Accountant,

    // Management
    OperationsManager,
    ChiefPilot,
    CFO
}

public enum WorkerStatus
{
    Available,
    OnDuty,
    OnLeave,
    Training,
    Terminated
}

public enum WorkerTrait
{
    Efficient,
    Careful,
    Leadership,
    Experienced,
    Loyal,
    Slow,
    Careless,
    Loner,
    Greedy,
    Unreliable
}
```

### 1.4 Passenger System

```csharp
public class PassengerClass
{
    public Guid Id { get; set; }

    public string Name { get; set; }              // "Economy", "Business", "First", "VIP"
    public string Description { get; set; }

    // Space Requirements
    public decimal SeatPitchInches { get; set; }  // 30" economy, 38" business, 60" first
    public decimal SeatWidthInches { get; set; }
    public decimal WeightPerPassengerKg { get; set; }  // Including luggage

    // Pricing
    public decimal BaseRatePerNm { get; set; }    // Per passenger per nautical mile
    public decimal MinimumFare { get; set; }

    // Requirements
    public bool RequiresCatering { get; set; }
    public bool RequiresEntertainment { get; set; }
    public int MinCrewPerPassengers { get; set; } // 1 crew per X passengers

    // Quality Factors
    public decimal CustomerSatisfactionMultiplier { get; set; }
    public decimal RepeatBusinessChance { get; set; }
}
```

---

## 2. License System

### 2.1 License Categories

```
LICENSE HIERARCHY:
├── Pilot Licenses
│   ├── Student Pilot License (SPL)
│   ├── Private Pilot License (PPL)
│   ├── Commercial Pilot License (CPL)
│   └── Airline Transport Pilot License (ATPL)
├── Ratings & Endorsements
│   ├── Single Engine Piston (SEP)
│   ├── Multi Engine Piston (MEP)
│   ├── Single Engine Turbine (SET)
│   ├── Multi Engine Turbine (MET)
│   ├── Instrument Rating (IR)
│   ├── Night Rating (NR)
│   └── Aerobatic Rating
├── Type Ratings (Aircraft Specific)
│   ├── Light Aircraft (C172, PA28, etc.) - No rating required
│   ├── Complex Singles (C210, BE36)
│   ├── Light Twins (BE58, PA34)
│   ├── Turboprops (PC12, TBM, King Air)
│   ├── Regional Jets (CRJ, ERJ, ATR)
│   ├── Narrow Body (A320, B737)
│   ├── Wide Body (A330, B777, B787)
│   └── Heavy (A380, B747)
├── Special Authorizations
│   ├── Dangerous Goods (DG)
│   ├── Low Visibility Operations (LVO)
│   ├── RVSM (Reduced Vertical Separation)
│   ├── ETOPS (Extended Twin Operations)
│   └── Polar Operations
└── Maintenance Licenses
    ├── Category A (Line Maintenance)
    ├── Category B1 (Mechanical)
    ├── Category B2 (Avionics)
    └── Category C (Base Maintenance)
```

### 2.2 License Requirements

| License | Prerequisites | Flight Hours | Cost | Exam Required | Validity |
|---------|--------------|--------------|------|---------------|----------|
| **PPL** | None | 40 hrs | $5,000 | Yes | 2 years |
| **CPL** | PPL, IR | 200 hrs | $15,000 | Yes | 1 year |
| **ATPL** | CPL | 1,500 hrs | $25,000 | Yes | 1 year |
| **MEP** | PPL | 70 hrs total, 10 ME | $3,000 | Yes | 2 years |
| **IR** | PPL | 50 hrs XC | $8,000 | Yes | 1 year |
| **Night Rating** | PPL | 5 hrs night | $1,500 | Yes | Lifetime* |
| **Type: A320** | CPL, MET | 500 hrs jet | $50,000 | Yes (sim) | 1 year |
| **Type: B737** | CPL, MET | 500 hrs jet | $45,000 | Yes (sim) | 1 year |
| **Type: B747** | ATPL | 2,000 hrs, widebody | $100,000 | Yes (sim) | 1 year |
| **DG License** | CPL | 100 hrs cargo | $5,000 | Yes | 2 years |
| **Maintenance A** | None | N/A | $10,000 | Yes | 2 years |
| **Maintenance B1** | Cat A, 2 yrs exp | N/A | $25,000 | Yes | 2 years |
| **Maintenance C** | Cat B, 5 yrs exp | N/A | $50,000 | Yes | 2 years |

### 2.3 License Entity Design

```csharp
public class LicenseType
{
    public Guid Id { get; set; }

    // Identity
    public string Code { get; set; }              // "CPL", "TYPE_A320", "MAINT_B1"
    public string Name { get; set; }              // "Commercial Pilot License"
    public string Description { get; set; }
    public LicenseCategory Category { get; set; }

    // Requirements
    public string[] PrerequisiteLicenseCodes { get; set; }  // ["PPL", "IR"]
    public int RequiredFlightHours { get; set; }
    public int RequiredFlightHoursInType { get; set; }      // For type ratings
    public decimal ExamFee { get; set; }
    public decimal IssuanceFee { get; set; }
    public bool RequiresSimulatorCheck { get; set; }
    public bool RequiresMedicalCertificate { get; set; }

    // Validity
    public int ValidityMonths { get; set; }       // 0 = lifetime
    public decimal RenewalFee { get; set; }
    public int RenewalFlightHoursRequired { get; set; }

    // Privileges
    public string[] AllowedAircraftTypes { get; set; }      // ICAO codes
    public string[] AllowedCargoCategories { get; set; }
    public bool AllowsCommercialOperations { get; set; }
    public bool AllowsPassengerTransport { get; set; }
    public int MaxPassengers { get; set; }

    // Suspension Rules
    public bool CanBeSuspended { get; set; }
    public int PointsToSuspension { get; set; }   // Violation points threshold

    public DateTime CreatedAt { get; set; }
}

public class UserLicense
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public Guid LicenseTypeId { get; set; }
    public LicenseType LicenseType { get; set; }

    // Status
    public LicenseStatus Status { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public DateTime? SuspensionEndsAt { get; set; }
    public string SuspensionReason { get; set; }

    // Points System
    public int ViolationPoints { get; set; }      // Points decay over time
    public DateTime LastViolationAt { get; set; }

    // Experience
    public int FlightHoursAtIssuance { get; set; }
    public int FlightHoursSinceIssuance { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum LicenseCategory
{
    PilotLicense,
    Rating,
    TypeRating,
    SpecialAuthorization,
    MaintenanceLicense
}

public enum LicenseStatus
{
    Active,
    Expired,
    Suspended,
    Revoked,
    PendingRenewal
}
```

### 2.4 Aircraft Type Rating Requirements

```csharp
public class AircraftLicenseRequirement
{
    public Guid Id { get; set; }

    public Guid AircraftTemplateId { get; set; }
    public AircraftTemplate AircraftTemplate { get; set; }

    // Required Licenses (ALL must be held)
    public string[] RequiredLicenseCodes { get; set; }  // ["CPL", "MET", "IR"]

    // Type Rating (if specific type rating needed)
    public string TypeRatingCode { get; set; }          // "TYPE_A320" or null for simple aircraft

    // Minimum Experience
    public int MinTotalFlightHours { get; set; }
    public int MinHoursInCategory { get; set; }         // Hours in similar aircraft

    // Special Requirements
    public bool RequiresDangerousGoodsForCargo { get; set; }
    public bool RequiresETOPS { get; set; }
    public bool RequiresRVSM { get; set; }
}
```

---

## 3. Risk & Consequences System

### 3.1 Inspection System

```csharp
public class InspectionEvent
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public Guid? FlightId { get; set; }           // If during flight
    public Guid? JobId { get; set; }              // Related job

    // Inspection Details
    public InspectionType Type { get; set; }
    public InspectionTrigger Trigger { get; set; }
    public int AirportId { get; set; }
    public Airport Airport { get; set; }

    // Outcome
    public InspectionOutcome Outcome { get; set; }
    public string[] ViolationsFound { get; set; }
    public decimal FineAmount { get; set; }
    public string[] LicensesSuspended { get; set; }
    public int SuspensionDays { get; set; }

    // Cargo Seized
    public Guid? SeizedCargoId { get; set; }
    public decimal SeizedCargoValue { get; set; }

    public DateTime OccurredAt { get; set; }
}

public enum InspectionType
{
    Customs,
    Police,
    AviationAuthority,
    Random
}

public enum InspectionTrigger
{
    Random,                  // Random inspection
    Flagged,                 // Player flagged from previous violations
    HighRiskRoute,           // Known smuggling route
    SuspiciousBehavior,      // Flying patterns suggest smuggling
    TipOff,                  // Anonymous tip (random event)
    RoutineAudit            // Scheduled inspection
}

public enum InspectionOutcome
{
    Clear,                   // Nothing found
    Warning,                 // Minor violation, warning only
    Fine,                    // Fine issued
    CargoSeized,             // Illegal cargo confiscated
    LicenseSuspended,        // License(s) suspended
    Arrested                 // Severe - account restrictions
}
```

### 3.2 Detection Risk Calculation

```
Base Detection Risk = CargoType.DetectionRiskPercent

Modifiers:
+ 10% if player has previous violations in last 30 days
+ 15% if route is flagged as high-risk
+ 20% if cargo weight is unusually high
+ 5% per inspection evaded in last 7 days
- 10% if using "clean" front cargo (hiding contraband)
- 5% per successful legal flight since last violation
- 10% if departing from smaller/less monitored airport

Final Detection Chance = Base × (1 + sum of modifiers)
Capped at 95% maximum

Example:
Cocaine (40% base) + Previous violation (+10%) + High-risk route (+15%)
= 40% × 1.25 = 50% chance of inspection
```

### 3.3 Consequence Tiers

```
TIER 1 - MINOR VIOLATION
├── Triggers: First offense, small quantity, less serious contraband
├── Fine: $5,000 - $25,000
├── License Points: +2 points
├── Suspension: None
└── Record: Flagged for 30 days

TIER 2 - MODERATE VIOLATION
├── Triggers: Repeat offense, medium quantity, or serious contraband
├── Fine: $25,000 - $100,000
├── License Points: +5 points
├── Suspension: 7-30 days on relevant licenses
├── Cargo: Seized
└── Record: Flagged for 90 days

TIER 3 - SEVERE VIOLATION
├── Triggers: Multiple offenses, large quantity, weapons
├── Fine: $100,000 - $500,000
├── License Points: +10 points
├── Suspension: 30-180 days on ALL licenses
├── Cargo: Seized
├── Aircraft: Potentially seized
└── Record: Permanent flag, restricted routes

TIER 4 - CRIMINAL
├── Triggers: Repeated severe violations, trafficking quantities
├── Fine: $500,000+
├── License: ALL licenses revoked
├── Assets: Aircraft seized
├── Account: Restricted mode (legal cargo only for 1 year)
└── Record: Permanent criminal record
```

### 3.4 License Point System

```csharp
public class ViolationRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public ViolationType Type { get; set; }
    public string Description { get; set; }
    public int PointsAssessed { get; set; }

    // Related Entities
    public Guid? InspectionEventId { get; set; }
    public Guid? FlightId { get; set; }

    // Financial Impact
    public decimal FineAmount { get; set; }
    public bool FinePaid { get; set; }

    // Point Decay
    public DateTime OccurredAt { get; set; }
    public DateTime PointsExpireAt { get; set; }  // Points decay after X months

    public DateTime CreatedAt { get; set; }
}

public enum ViolationType
{
    // Traffic Violations
    Speeding,
    UnauthorizedAirspace,
    RunwayIncursion,

    // Cargo Violations
    IllegalCargo,
    UndeclaredCargo,
    ExceededWeightLimit,
    DangerousGoodsViolation,

    // License Violations
    FlyingWithoutLicense,
    ExpiredLicense,
    ExceededPrivileges,

    // Safety Violations
    UnsafeOperation,
    MaintenanceViolation,
    FatigueViolation,

    // Other
    FraudulentActivity,
    TaxEvasion,
    InsuranceFraud
}
```

### 3.5 Point Thresholds

| Points | Consequence |
|--------|-------------|
| 0-4 | Good standing |
| 5-9 | Warning issued, increased inspection chance |
| 10-14 | 30-day probation, random audits |
| 15-19 | License suspension (30 days) |
| 20-24 | License suspension (90 days) |
| 25+ | License revocation, review required |

**Point Decay**: 1 point removed per 30 days with no violations

---

## 4. Economy & Pricing

### 4.1 Job Pricing Formula

```
CARGO JOB PRICING:

Base Pay = Cargo.BaseValuePerKg × Weight × DistanceMultiplier

Distance Multiplier:
- 0-100 NM: 1.0 (base rate)
- 100-500 NM: 0.8 (bulk discount)
- 500-1500 NM: 0.6 (long haul efficiency)
- 1500+ NM: 0.5 (ultra long haul)

Modifiers:
× 1.5 if time-sensitive (express delivery)
× 1.3 if requires special handling (refrigeration, DG, etc.)
× 2.0-5.0 if illegal (risk premium)
× 0.9-1.2 based on market demand
× 1.1-1.3 based on route difficulty (terrain, weather)

PASSENGER JOB PRICING:

Base Pay = PassengerClass.BaseRatePerNm × Distance × Passengers

Modifiers:
× 1.2 if holiday/peak season
× 1.5 if charter (exclusive aircraft)
× 1.0-1.3 based on destination popularity
× 0.8 if budget airline simulation

EXAMPLE CALCULATIONS:

1. Legal Cargo - Mail (500kg, 300NM):
   $3/kg × 500kg × 0.8 = $1,200

2. High Value - Gold (50kg, 500NM):
   $60,000/kg × 50kg × 0.8 × 1.3 (security) = $3,120,000
   (Insurance premium: 2% = $62,400)

3. Illegal - Cocaine (20kg, 200NM):
   $35,000/kg × 20kg × 1.0 × 3.0 (risk) = $2,100,000
   (But 40% detection risk!)

4. Passengers - Business Class (50 pax, 1000NM):
   $0.50/NM × 1000NM × 50 × 1.2 (demand) = $30,000
```

### 4.2 Operating Costs

```csharp
public class OperatingCostCalculator
{
    public decimal CalculateFlightCost(
        OwnedAircraft aircraft,
        int distanceNm,
        int flightTimeMinutes)
    {
        var template = aircraft.Template;

        // Fuel Cost (simplified)
        decimal fuelBurnPerHour = template.MtowKg * 0.00015m; // kg/hr approximation
        decimal fuelPricePerKg = 1.50m; // Jet-A price
        decimal fuelCost = (flightTimeMinutes / 60m) * fuelBurnPerHour * fuelPricePerKg;

        // Maintenance Reserve (per flight hour)
        decimal maintenanceReserve = template.MtowKg * 0.02m;
        decimal maintenanceCost = (flightTimeMinutes / 60m) * maintenanceReserve;

        // Landing Fees (based on MTOW)
        decimal landingFee = template.MtowKg * 0.01m;

        // Navigation Fees (per NM)
        decimal navFees = distanceNm * 0.10m;

        // Handling Fees
        decimal handlingFees = 100m + (template.MtowKg * 0.005m);

        // Insurance (per flight)
        decimal insuranceCost = aircraft.CurrentValue * 0.00001m;

        // Crew Costs (if applicable)
        decimal crewCost = (flightTimeMinutes / 60m) * 150m; // $150/hr

        return fuelCost + maintenanceCost + landingFee +
               navFees + handlingFees + insuranceCost + crewCost;
    }
}
```

### 4.3 Market Dynamics

```csharp
public class MarketCondition
{
    public Guid Id { get; set; }

    // Location
    public int? AirportId { get; set; }           // Null = global
    public string Region { get; set; }            // "Europe", "North America", etc.

    // Cargo Market
    public Guid CargoTypeId { get; set; }
    public CargoType CargoType { get; set; }

    // Supply & Demand
    public decimal DemandMultiplier { get; set; } // 0.5 - 2.0
    public decimal SupplyLevel { get; set; }      // 0.0 - 1.0 (availability)
    public decimal PriceMultiplier { get; set; }  // Calculated from supply/demand

    // Time-based
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveUntil { get; set; }

    // Events
    public string EventDescription { get; set; }  // "Holiday Rush", "Trade Embargo", etc.
}

// Example market events:
// "Christmas Rush" - Mail demand +100%, Electronics +50%
// "Gold Price Spike" - Precious metals value +30%
// "Trade Embargo" - Certain routes blocked or premium
// "Fuel Crisis" - Operating costs +50%
// "Tourism Boom" - Passenger rates +40%
```

### 4.4 Player Income Statement

```csharp
public class FlightFinancials
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid FlightId { get; set; }
    public Guid? JobId { get; set; }

    // Revenue
    public decimal CargoRevenue { get; set; }
    public decimal PassengerRevenue { get; set; }
    public decimal CharterFee { get; set; }
    public decimal BonusPayments { get; set; }    // On-time, condition, etc.
    public decimal TotalRevenue { get; set; }

    // Costs
    public decimal FuelCost { get; set; }
    public decimal LandingFees { get; set; }
    public decimal NavigationFees { get; set; }
    public decimal HandlingFees { get; set; }
    public decimal CrewCost { get; set; }
    public decimal InsuranceCost { get; set; }
    public decimal MaintenanceReserve { get; set; }
    public decimal TotalCosts { get; set; }

    // Penalties
    public decimal LatePenalty { get; set; }
    public decimal DamagePenalty { get; set; }
    public decimal FinesAssessed { get; set; }
    public decimal TotalPenalties { get; set; }

    // Net
    public decimal NetProfit { get; set; }        // Revenue - Costs - Penalties
    public decimal ProfitMarginPercent { get; set; }

    public DateTime CalculatedAt { get; set; }
}
```

---

## 5. Workshop & Maintenance Business

### 5.1 Player-Owned Workshop

```csharp
public class Workshop
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }
    public User Owner { get; set; }

    // Location
    public int AirportId { get; set; }
    public Airport Airport { get; set; }
    public string HangarNumber { get; set; }

    // Capabilities
    public MaintenanceCapability Capability { get; set; }
    public string[] CertifiedAircraftTypes { get; set; }  // ICAO codes
    public int MaxAircraftMtowKg { get; set; }
    public int SimultaneousAircraft { get; set; }  // Hangar capacity

    // Licensing
    public Guid? OwnerMaintenanceLicenseId { get; set; }
    public UserLicense OwnerMaintenanceLicense { get; set; }

    // Economics
    public decimal PurchasePrice { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal MonthlyUtilities { get; set; }
    public decimal ToolsAndEquipmentValue { get; set; }

    // Operations
    public int EmployeeCount { get; set; }
    public decimal HourlyLaborRate { get; set; }
    public decimal PartsMarkupPercent { get; set; }  // Profit on parts

    // Status
    public WorkshopStatus Status { get; set; }
    public decimal Reputation { get; set; }       // 0-5 stars
    public int JobsCompleted { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum MaintenanceCapability
{
    LineOnly,           // Cat A - Basic inspections, simple repairs
    LightMaintenance,   // Cat B1/B2 - Component replacement, avionics
    BaseMaintenance,    // Cat C - Heavy maintenance, modifications
    FullMRO             // Full MRO - Overhauls, paint, interiors
}

public enum WorkshopStatus
{
    Active,
    Suspended,          // License issues
    UnderInspection,
    Closed
}
```

### 5.2 Workshop Revenue Model

```
WORKSHOP SERVICES PRICING:

Line Maintenance (Cat A):
- Hourly labor: $75-150/hr
- Pre-flight inspections: $200-500
- Tire changes: $500-2000
- Fluid top-ups: $100-300

Component Maintenance (Cat B):
- Hourly labor: $150-300/hr
- Engine repairs: $5,000-50,000
- Avionics repairs: $2,000-20,000
- Landing gear service: $3,000-15,000

Base Maintenance (Cat C):
- Hourly labor: $200-400/hr
- C-Check (narrow body): $500,000-1,000,000
- D-Check (narrow body): $2,000,000-5,000,000
- Paint job: $50,000-200,000

PROFIT MARGINS:
- Labor: 40-60% margin
- Parts: 15-30% markup
- Average job profit: 25-35%
```

---

## 6. Integration with MSFS

### 6.1 Flight Tracking Data

```csharp
public class TrackedFlight
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public Guid? OwnedAircraftId { get; set; }
    public OwnedAircraft OwnedAircraft { get; set; }

    public Guid? ActiveJobId { get; set; }
    public Job ActiveJob { get; set; }

    // Flight Plan
    public int DepartureAirportId { get; set; }
    public int? ArrivalAirportId { get; set; }     // Null if still in flight
    public string PlannedRoute { get; set; }
    public int PlannedDistanceNm { get; set; }

    // Actual Data (from SimConnect)
    public string ActualDepartureIcao { get; set; }
    public string ActualArrivalIcao { get; set; }
    public int ActualDistanceNm { get; set; }
    public int FlightTimeMinutes { get; set; }
    public int FuelUsedKg { get; set; }

    // Performance Metrics
    public int MaxAltitudeFt { get; set; }
    public int MaxSpeedKts { get; set; }
    public int LandingRateFpm { get; set; }        // Vertical speed at touchdown
    public decimal MaxGForce { get; set; }
    public int TouchdownDistanceFt { get; set; }

    // Status
    public FlightStatus Status { get; set; }
    public DateTime DepartedAt { get; set; }
    public DateTime? ArrivedAt { get; set; }

    // Validation
    public bool IsValidForPayout { get; set; }
    public string[] ValidationIssues { get; set; }

    // Cargo Condition
    public decimal CargoConditionPercent { get; set; }  // 100% = perfect
    public string CargoConditionNotes { get; set; }     // "Rough landing", etc.

    public DateTime CreatedAt { get; set; }
}

public enum FlightStatus
{
    Planning,
    Departed,
    EnRoute,
    Approaching,
    Landed,
    Completed,
    Cancelled,
    Crashed,
    Diverted
}
```

### 6.2 Validation Rules for Payout

```
FLIGHT VALIDATION:

1. DEPARTURE VALIDATION:
   ✓ Must depart from job's departure airport
   ✓ Must be in correct aircraft (if job specifies)
   ✓ Must have required licenses
   ✓ Cargo must be loaded

2. EN-ROUTE VALIDATION:
   ✓ No time acceleration beyond 4x
   ✓ No teleporting/slew mode
   ✓ Continuous flight (no disconnects > 5 min)
   ✓ Stay within altitude restrictions

3. ARRIVAL VALIDATION:
   ✓ Land at job's destination airport
   ✓ Complete stop on runway/taxiway
   ✓ No crash/damage events
   ✓ Within time limit (if applicable)

4. CARGO CONDITION:
   - Landing rate > 600 fpm: -10% cargo condition
   - Landing rate > 1000 fpm: -30% cargo condition
   - G-force > 2.5: -20% cargo condition
   - Crash: 0% cargo condition

5. PAYOUT MODIFIERS:
   × 1.0 if 100% cargo condition
   × 0.9 if 90% cargo condition
   × 0.5 if 50% cargo condition
   × 0.0 if cargo destroyed
   × 1.1 if on-time delivery
   × 0.9 if late delivery
```

---

## 7. Entity Summary

### New Entities Required

```
CARGO SYSTEM:
├── CargoType              - Defines cargo categories and properties
├── PassengerClass         - Defines passenger service levels
└── MarketCondition        - Dynamic pricing and availability

LICENSE SYSTEM:
├── LicenseType            - Defines available licenses
├── UserLicense            - Player's held licenses
├── AircraftLicenseRequirement - Links aircraft to required licenses
└── LicenseExam            - Exam/test tracking (optional)

RISK SYSTEM:
├── InspectionEvent        - Records of inspections
├── ViolationRecord        - Player violation history
└── CriminalRecord         - Severe violations (optional)

ECONOMY:
├── FlightFinancials       - Per-flight profit/loss
├── TransactionLog         - All money movements
└── BankAccount            - Player financial accounts (optional)

WORKSHOP:
├── Workshop               - Player-owned maintenance facilities
├── WorkshopJob            - Maintenance jobs in progress
└── WorkshopCertification  - Workshop capabilities

FLIGHT TRACKING:
├── TrackedFlight          - Real-time flight data from MSFS
├── FlightWaypoint         - Route tracking points
└── FlightEvent            - Events during flight (violations, etc.)
```

---

## 8. Banking & Loan System

### 8.1 Overview

Players need financing to purchase expensive aircraft. The banking system provides:
- Multiple banks with different risk appetites
- Credit scores affecting loan availability and rates
- Various loan products (aircraft financing, business loans, lines of credit)
- Consequences for missed payments

### 8.2 Banks

```
BANKING INSTITUTIONS:

🏦 FIRST NATIONAL AVIATION BANK (Conservative)
├── Profile: Traditional, risk-averse, best rates
├── Interest Rate: 5-8% APR
├── Requirements:
│   ├── Credit Score: 750+ required
│   ├── Down Payment: 25% minimum
│   ├── Flight Hours: 500+ hours
│   └── Clean Record: No violations in 6 months
├── Loan Terms: 5-15 years
├── Max Loan: $50,000,000
└── Benefits: Lowest rates, flexible early repayment

🏛️ SKYLINE COMMERCIAL BANK (Standard)
├── Profile: Mainstream aviation lender
├── Interest Rate: 8-12% APR
├── Requirements:
│   ├── Credit Score: 650+ required
│   ├── Down Payment: 15% minimum
│   ├── Flight Hours: 200+ hours
│   └── Clean Record: No major violations in 3 months
├── Loan Terms: 3-10 years
├── Max Loan: $100,000,000
└── Benefits: Balanced terms, larger loan amounts

💳 RAPID AVIATION FINANCE (Flexible)
├── Profile: Aviation specialist, works with pilots
├── Interest Rate: 12-18% APR
├── Requirements:
│   ├── Credit Score: 550+ required
│   ├── Down Payment: 10% minimum
│   ├── Flight Hours: 100+ hours
│   └── Clean Record: No criminal record
├── Loan Terms: 2-7 years
├── Max Loan: $25,000,000
└── Benefits: Fast approval, lower barriers

⚠️ HIGHWING CAPITAL (High-Risk Lender)
├── Profile: Last resort, predatory rates
├── Interest Rate: 18-30% APR
├── Requirements:
│   ├── Credit Score: 400+ (accepts almost anyone)
│   ├── Down Payment: 5% minimum
│   ├── Flight Hours: Any
│   └── Clean Record: Not required
├── Loan Terms: 1-5 years
├── Max Loan: $10,000,000
├── Penalties: Severe late fees, aggressive collection
└── Warning: Should only be used as last resort
```

### 8.3 Credit Score System

```
CREDIT SCORE RANGE: 300-850

SCORE BRACKETS:
├── 800-850: Excellent - Best rates, highest limits
├── 750-799: Very Good - Premium rates available
├── 700-749: Good - Standard rates
├── 650-699: Fair - Higher rates, some restrictions
├── 550-649: Poor - Limited options, high rates
├── 400-549: Very Poor - High-risk lenders only
└── 300-399: Critical - Very limited borrowing ability

STARTING SCORE: 650 (New players)

FACTORS THAT IMPROVE CREDIT (+):
├── On-time loan payments: +5 per payment (max +50/month)
├── Loan paid off early: +25 bonus
├── Loan paid in full: +50 bonus
├── Consistent income: +3 per $100k earned/month (max +15)
├── Account age: +1 per month (max +60)
├── Low debt-to-income ratio: +10 if under 30%
└── Clean legal record: +5 per month no violations

FACTORS THAT HURT CREDIT (-):
├── Late payment (1-30 days): -25 per occurrence
├── Late payment (30-60 days): -50 per occurrence
├── Late payment (60-90 days): -75 per occurrence
├── Missed payment: -100 per occurrence
├── Loan default: -200 (plus legal consequences)
├── License suspension: -30
├── Criminal violation: -50 to -150
├── Multiple loan applications: -5 per application (soft inquiry)
└── High debt-to-income ratio: -20 if over 50%

CREDIT RECOVERY:
- Negative marks reduce impact over time
- Late payments: Full impact for 6 months, then -50% per year
- Defaults: Full impact for 2 years, then -25% per year
- Bankrupty: Resets score to 400, 5 year recovery period
```

### 8.4 Loan Products

```
LOAN TYPES:

1. AIRCRAFT FINANCING
   ├── Purpose: Purchase specific aircraft
   ├── Collateral: The aircraft itself
   ├── Rate Discount: -1% (secured loan)
   ├── Max LTV: 75-95% (based on credit)
   └── If default: Aircraft repossessed

2. BUSINESS EXPANSION LOAN
   ├── Purpose: General business needs
   ├── Collateral: Business assets
   ├── Rate Premium: +2% (higher risk)
   ├── Max Amount: Based on income history
   └── Use for: Training, licenses, workshop setup

3. LINE OF CREDIT
   ├── Purpose: Flexible borrowing
   ├── Collateral: None (unsecured)
   ├── Rate Premium: +4% (highest risk)
   ├── Max Amount: 2x monthly income
   └── Use for: Operating expenses, emergencies

4. REFINANCING
   ├── Purpose: Replace existing loan
   ├── Benefit: Lower rate if credit improved
   ├── Fee: 1-2% of loan amount
   └── Requirement: 12+ months on current loan
```

### 8.5 Loan Calculations

```csharp
public class LoanCalculator
{
    public decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int termMonths)
    {
        // Standard amortization formula
        decimal monthlyRate = annualRate / 12 / 100;
        decimal payment = principal *
            (monthlyRate * Math.Pow(1 + monthlyRate, termMonths)) /
            (Math.Pow(1 + monthlyRate, termMonths) - 1);
        return payment;
    }
}

// Example: A320 Purchase ($98,000,000)
// Credit Score: 720 (Good)
// Bank: Skyline Commercial (10% APR)
// Down Payment: 15% ($14,700,000)
// Loan Amount: $83,300,000
// Term: 10 years (120 months)
// Monthly Payment: $1,101,000
// Total Interest: $48,820,000
// Total Cost: $147,520,000

// With good credit (750+) at First National (6% APR):
// Monthly Payment: $925,000
// Total Interest: $27,700,000
// Total Cost: $125,700,000
// SAVINGS: $21,820,000 over loan life!
```

### 8.6 Default & Consequences

```
MISSED PAYMENT TIMELINE:

Day 1-7: Grace period (no penalty)
Day 8-30: Late fee (5% of payment)
         Credit impact: -25
Day 31-60: Additional late fee (10% of payment)
          Credit impact: -50
          Warning letter sent
Day 61-90: Collections process begins
          Credit impact: -75
          Interest rate increased by 5%
Day 91+: DEFAULT
        Credit impact: -200
        Collateral seizure initiated
        Legal action possible

AIRCRAFT REPOSSESSION:
- If loan secured by aircraft
- 30 days notice before seizure
- Player can pay in full to prevent
- Aircraft sold at auction (player gets excess minus fees)
- Deficiency balance still owed if sale < loan

BANKRUPTCY OPTION:
- Eliminates all debt
- Credit score reset to 400
- Cannot take new loans for 3 years
- All aircraft and business assets liquidated
- Start fresh with $10,000
```

---

## 9. AI Crew System (Passive Income)

### 9.1 Overview

Players can hire AI pilots and crew to operate their aircraft on routes automatically, generating passive income while offline or flying other aircraft. This creates an "airline tycoon" element.

### 9.2 AI Crew Types

```
CREW POSITIONS:

👨‍✈️ PILOTS
├── Student Pilot (Cannot hire - players only)
├── Private Pilot
│   ├── Salary: $3,000/month
│   ├── Can fly: Light singles
│   ├── Skill Rating: 1-3 stars
│   └── Incident Rate: 5-15%
├── Commercial Pilot
│   ├── Salary: $8,000/month
│   ├── Can fly: Twins, small turboprops
│   ├── Skill Rating: 2-4 stars
│   └── Incident Rate: 2-8%
├── Airline Pilot
│   ├── Salary: $15,000/month
│   ├── Can fly: All aircraft
│   ├── Skill Rating: 3-5 stars
│   └── Incident Rate: 0.5-3%
└── Captain (Senior)
    ├── Salary: $25,000/month
    ├── Can fly: All aircraft, international
    ├── Skill Rating: 4-5 stars
    └── Incident Rate: 0.1-1%

👩‍💼 CABIN CREW (Required for passenger flights)
├── Flight Attendant
│   ├── Salary: $2,500/month
│   ├── Ratio: 1 per 50 passengers
│   └── Affects: Customer satisfaction
├── Senior Flight Attendant
│   ├── Salary: $4,000/month
│   ├── Required: 1 per aircraft
│   └── Affects: Service quality +20%
└── Purser (Large aircraft)
    ├── Salary: $6,000/month
    ├── Required: Wide body aircraft
    └── Affects: First class ratings

🔧 GROUND CREW (Reduce turnaround time)
├── Load Master
│   ├── Salary: $3,500/month
│   └── Effect: -20% cargo loading time
├── Fueler
│   ├── Salary: $2,000/month
│   └── Effect: -15% refueling time
└── Dispatcher
    ├── Salary: $4,500/month
    └── Effect: Better route optimization
```

### 9.3 AI Flight Simulation

```
AI FLIGHT MECHANICS:

When player assigns AI crew to aircraft + route:

1. FLIGHT SCHEDULING
   ├── Player sets route (origin → destination)
   ├── System calculates flight time based on aircraft
   ├── Flight departs at scheduled time
   └── Completes in real-time (can be accelerated)

2. FLIGHT SIMULATION (Background)
   ├── No actual MSFS simulation
   ├── Calculated based on:
   │   ├── Route distance
   │   ├── Aircraft performance
   │   ├── Weather conditions (random events)
   │   ├── Pilot skill level
   │   └── Aircraft condition
   └── Results determined at completion

3. OUTCOME CALCULATION
   ├── Success Rate = BasePilotSkill + AircraftCondition - WeatherPenalty
   ├── Revenue = Standard cargo/passenger rates
   ├── Costs = Fuel + Crew + Maintenance + Fees
   └── Profit = Revenue - Costs

4. INCIDENT SYSTEM
   ├── Incident Chance = BaseIncidentRate × (100 - AircraftCondition)/100 × WeatherFactor
   ├── Incident Types:
   │   ├── Minor delay (-10% profit, no damage)
   │   ├── Cargo damage (-30% profit)
   │   ├── Passenger complaints (-reputation)
   │   ├── Hard landing (aircraft damage)
   │   ├── Mechanical issue (aircraft grounded)
   │   └── Serious incident (rare, major damage)
   └── Pilot experience reduces incident severity

EXAMPLE AI FLIGHT:

Aircraft: Cessna 421C (hired AI pilot - Commercial, 3-star)
Route: KLAX → KLAS (236 NM)
Cargo: 600kg Electronics @ $100/kg = $60,000 revenue
Flight Time: 1.5 hours

Costs:
├── Fuel: $800
├── Pilot salary portion: $150 (hourly rate)
├── Landing fees: $200
├── Navigation: $50
└── Total Costs: $1,200

Profit: $60,000 - $1,200 = $58,800

Incident roll: 5% chance (3-star pilot)
Result: Success! +$58,800 to player balance

Next flight available in: 2 hours (turnaround)
```

### 9.4 Route Management

```
ROUTE SETUP:

1. DEFINE ROUTE
   ├── Select origin airport (where aircraft is)
   ├── Select destination airport
   ├── System shows available contracts
   └── Select cargo/passenger type

2. ASSIGN RESOURCES
   ├── Select aircraft
   ├── Assign pilot(s) - must match aircraft requirements
   ├── Assign cabin crew (if passengers)
   └── Confirm operating costs

3. SCHEDULING OPTIONS
   ├── Single flight
   ├── Round trip (auto-return)
   ├── Repeating schedule (daily, weekly)
   └── Until cancelled

4. MONITORING
   ├── Flight status dashboard
   ├── Real-time position on map
   ├── Estimated arrival
   ├── Profit projection
   └── Incident alerts

ROUTE RESTRICTIONS:
- Aircraft must be at origin airport
- Pilot must be qualified for aircraft type
- Pilot must have required licenses
- Cannot assign to aircraft player is currently flying
- Maximum 10 active AI routes per player (can upgrade)
```

### 9.5 AI Crew Hiring

```
HIRING PROCESS:

1. JOB POSTING
   ├── Specify position type
   ├── Set salary offer (affects quality)
   ├── Set requirements (hours, ratings)
   └── Post for 24-48 hours (game time)

2. APPLICANT POOL
   ├── Random generation based on market
   ├── Higher salary = better applicants
   ├── Location matters (more pilots near hubs)
   └── 3-8 applicants typically

3. APPLICANT QUALITIES
   ├── Skill Rating: 1-5 stars
   ├── Experience: Flight hours
   ├── Licenses: What they can fly
   ├── Personality: Affects incidents
   │   ├── Careful: -2% incident rate
   │   ├── Aggressive: +5% incident rate, faster flights
   │   ├── Efficient: -10% fuel consumption
   │   └── Veteran: Handles incidents better
   └── Salary Expectation: May negotiate

4. CONTRACTS
   ├── Duration: 1, 3, 6, 12 months
   ├── Early termination: 1 month severance
   ├── Performance reviews: Monthly
   └── Salary increases: Based on performance

CREW MANAGEMENT:
- Maximum crew size scales with player level
- Starter: 2 pilots, 4 cabin crew
- Established: 5 pilots, 12 cabin crew
- Airline: 20 pilots, 50 cabin crew
- Can fire crew (severance required)
- Crew morale affects performance
```

### 9.6 AI Fleet Operations Dashboard

```
FLEET DASHBOARD:

┌─────────────────────────────────────────────────────────────┐
│  MY FLEET OPERATIONS                     Total Profit: $2.4M│
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ✈️ N421CC (Cessna 421C) - ACTIVE                          │
│  ├── Pilot: John Smith (3⭐)                               │
│  ├── Route: KLAX → KLAS                                    │
│  ├── Status: En Route (45 min remaining)                   │
│  ├── Cargo: Electronics (600kg)                            │
│  └── Est. Profit: $58,800                                  │
│                                                             │
│  ✈️ N350KA (King Air 350) - AVAILABLE                      │
│  ├── Pilot: Sarah Jones (4⭐)                              │
│  ├── Location: KJFK                                        │
│  ├── Status: Ready for assignment                          │
│  └── Condition: 92%                                        │
│                                                             │
│  ✈️ N320PL (A320-200) - IN MAINTENANCE                     │
│  ├── Pilot: Mike Chen (5⭐)                                │
│  ├── Location: EGLL                                        │
│  ├── Status: C-Check (2 days remaining)                    │
│  └── Condition: Will be 100%                               │
│                                                             │
│  [+ Add Route]  [View All Flights]  [Hire Crew]            │
└─────────────────────────────────────────────────────────────┘
```

---

## 10. Future Expansion Features

### Features to Support

1. **Airlines/Virtual Airlines**
   - Company entity owning multiple aircraft
   - Employee pilots (real players)
   - Shared fleet and profits
   - Airline branding and liveries

2. **Stock Market**
   - Cargo commodity trading
   - Company stocks
   - Economic speculation
   - Futures contracts on cargo

3. **Insurance System**
   - Multiple insurance tiers
   - Claim processing
   - Premium calculation based on history
   - Required for financed aircraft

4. **Reputation System**
   - Customer ratings
   - Airline reputation score
   - Route popularity
   - Affects job availability and pricing

5. **Events/Seasons**
   - Holiday events (Christmas rush)
   - Weather events (storms, volcanic ash)
   - Economic crises
   - Special high-value missions

6. **Multiplayer Economy**
   - Player-to-player cargo contracts
   - Auction house for aircraft
   - Competitive routes
   - Price wars

7. **Real-World Data Integration**
   - Real fuel prices
   - Real weather affecting routes
   - Real-world news events

---

## 9. Database Schema Overview

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│      User       │────<│   UserLicense    │>────│   LicenseType   │
└────────┬────────┘     └──────────────────┘     └─────────────────┘
         │
         │ 1:N
         ▼
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  OwnedAircraft  │────<│   TrackedFlight  │>────│       Job       │
└────────┬────────┘     └────────┬─────────┘     └────────┬────────┘
         │                       │                        │
         │                       │                        │
         ▼                       ▼                        ▼
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│AircraftComponent│     │ FlightFinancials │     │    CargoType    │
└─────────────────┘     └──────────────────┘     └─────────────────┘
                                │
                                │ 1:N
                                ▼
                        ┌──────────────────┐
                        │ InspectionEvent  │
                        └────────┬─────────┘
                                 │
                                 ▼
                        ┌──────────────────┐
                        │ ViolationRecord  │
                        └──────────────────┘

┌─────────────────┐     ┌──────────────────┐
│    Workshop     │────<│   WorkshopJob    │
└─────────────────┘     └──────────────────┘
```

---

## 10. Implementation Priority

### Phase 1: Core Economy
1. CargoType entity and seed data
2. Basic job pricing system
3. Flight tracking integration
4. Basic payout calculation

### Phase 2: License System
1. LicenseType and UserLicense entities
2. Aircraft-license requirements
3. License checks on job acceptance
4. Basic license purchase flow

### Phase 3: Risk System
1. Illegal cargo types
2. Inspection system
3. Violation tracking
4. License suspension

### Phase 4: Advanced Economy
1. Market dynamics
2. Operating costs
3. Flight financials
4. Transaction history

### Phase 5: Workshop System
1. Workshop entity
2. Maintenance licensing
3. Player-owned workshops
4. Third-party repair jobs
