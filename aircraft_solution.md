# Aircraft Ownership & Modification System - Solution Design

## Overview

This document outlines the design for a comprehensive aircraft ownership system that allows players to:
- Own individual instances of aircraft types
- Repair specific components (engines, wings, gear, fuselage)
- Modify their aircraft (seat-to-cargo conversion, upgrades)
- Have unique stats per aircraft instance while sharing the same base type

---

## Current State Analysis

### Existing Database Structure
- **User**: Has `Balance`, `TotalFlightMinutes`, `CurrentAirportId`, `HomeAirportId`
- **Aircraft**: Shared catalog of aircraft types (not player-owned)
- **AircraftRequest**: User requests for new aircraft types to be added
- **Job**: Cargo/passenger jobs with payouts
- **Airport**: Location data

### Key Gap
Currently, Aircraft is a **shared catalog** with no ownership model. We need a bridge between Users and Aircraft that supports individual instances with their own state.

### Available Data (from aircraft.csv - 398 aircraft)
- Performance specs (speeds, climb rates, range, ceiling)
- Dimensions (wingspan, length, height)
- Power plant details (engine count, type, thrust/power)
- Classification (ICAO, type, WTC, RECAT-EU)
- Physical characteristics (wing position, gear type, tail config)

---

## Solution Approaches

### Approach 1: Simple JSON-Based (Not Recommended)

Store everything in a single `OwnedAircraft` table with JSON columns for parts and modifications.

```
OwnedAircraft
├── Id (Guid)
├── UserId (FK)
├── AircraftTemplateId (FK)
├── PartConditions (JSON) ← {"engine_1": 85, "left_wing": 92, ...}
├── Modifications (JSON) ← [{"type": "cargo", "seats_removed": 50}]
└── Stats (JSON)
```

**Pros**: Simple, flexible, fast to implement
**Cons**:
- Cannot query part conditions efficiently
- No referential integrity for parts/mods
- Hard to calculate aggregate stats
- Poor for analytics

---

### Approach 2: Fully Normalized (Complex but Powerful)

Separate tables for every entity with full relationships.

```
AircraftTemplate (398 rows from CSV)
    ↓
OwnedAircraft (player instances)
    ↓
├── AircraftComponent (individual parts)
├── AircraftModification (applied mods)
└── MaintenanceLog (repair history)
```

**Pros**: Full SQL querying, integrity, analytics-ready
**Cons**: Many joins, more complex queries

---

### Approach 3: Hybrid (RECOMMENDED)

Core entities normalized, with computed/derived fields and strategic denormalization for performance.

**Pros**:
- Best query performance for common operations
- Maintains integrity for critical data
- Flexible for future expansion
- Clear separation between template and instance

---

## Recommended Solution: Hybrid Approach

### Database Schema

#### 1. AircraftTemplate (Master Data from CSV)

This replaces/enhances the current Aircraft table with full Eurocontrol data.

```csharp
public class AircraftTemplate
{
    public Guid Id { get; set; }

    // Identification
    public string IcaoCode { get; set; }          // "A320", "B738", etc.
    public string Name { get; set; }               // "A320-200"
    public string Manufacturer { get; set; }       // "AIRBUS"
    public string IataCode { get; set; }           // "320"

    // Classification
    public string AircraftType { get; set; }       // "L2J" (Land, 2 engines, Jet)
    public string Apc { get; set; }                // Approach category A-E
    public string Wtc { get; set; }                // Wake turbulence L/M/H/J
    public string RecatEu { get; set; }            // EU wake category

    // Weights
    public int MtowKg { get; set; }                // Max takeoff weight
    public int BaseEmptyWeightKg { get; set; }     // Calculated from MTOW
    public int MaxPayloadKg { get; set; }          // MTOW - Empty - Fuel

    // Performance
    public int CruiseTasKts { get; set; }
    public decimal CruiseMach { get; set; }
    public int CruiseCeilingFl { get; set; }
    public int RangeNm { get; set; }
    public int TakeoffDistanceM { get; set; }
    public int LandingDistanceM { get; set; }

    // Climb/Descent Performance
    public int V2IasKts { get; set; }
    public int InitialClimbIasKts { get; set; }
    public int InitialClimbRocFtMin { get; set; }
    public int ApproachIasKts { get; set; }
    public int VatIasKts { get; set; }

    // Dimensions
    public decimal WingSpanM { get; set; }
    public decimal LengthM { get; set; }
    public decimal HeightM { get; set; }

    // Engine Configuration
    public string PowerPlantDescription { get; set; }  // Full text from CSV
    public int EngineCount { get; set; }               // Parsed: 1, 2, 3, 4, 6
    public string EngineType { get; set; }             // "Turbofan", "Turboprop", "Piston"
    public int EngineThrustKn { get; set; }            // Per engine (for jets)
    public int EngineShp { get; set; }                 // Per engine (for props)

    // Physical Configuration
    public string WingPosition { get; set; }           // "Low wing", "High wing"
    public string EnginePosition { get; set; }         // "Underwing mounted"
    public string TailConfiguration { get; set; }      // "T-tail", "Regular tail"
    public string LandingGearType { get; set; }        // "Tricycle retractable"

    // Capacity (Calculated/Estimated)
    public int BasePassengerCapacity { get; set; }     // Estimated from type
    public int MaxCargoVolumeM3 { get; set; }          // Estimated from dimensions

    // Economics (Game Balance)
    public decimal BasePurchasePrice { get; set; }     // Based on MTOW/type
    public decimal HourlyOperatingCost { get; set; }   // Fuel + crew + maintenance
    public decimal BaseInsuranceCost { get; set; }     // Monthly

    // Metadata
    public bool IsAvailableForPurchase { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<OwnedAircraft> OwnedInstances { get; set; }
}
```

#### 2. OwnedAircraft (Player Instances)

```csharp
public class OwnedAircraft
{
    public Guid Id { get; set; }

    // Ownership
    public Guid OwnerId { get; set; }
    public User Owner { get; set; }
    public Guid AircraftTemplateId { get; set; }
    public AircraftTemplate Template { get; set; }

    // Identity
    public string Registration { get; set; }       // "N12345", player-customizable
    public string Nickname { get; set; }           // "Blue Thunder"
    public string Livery { get; set; }             // Livery identifier

    // Location & Status
    public int CurrentAirportId { get; set; }
    public Airport CurrentAirport { get; set; }
    public AircraftStatus Status { get; set; }     // Available, InFlight, InMaintenance, Damaged

    // Flight Hours
    public int TotalFlightHours { get; set; }
    public int HoursSinceLastService { get; set; }
    public DateTime? LastServiceDate { get; set; }

    // Overall Condition (0-100, calculated from components)
    public int OverallCondition { get; set; }

    // Current Configuration (Modified Stats)
    public int CurrentPassengerCapacity { get; set; }  // After seat removal
    public int CurrentCargoCapacityKg { get; set; }    // After cargo conversion
    public int CurrentRangeNm { get; set; }            // After fuel tank mods
    public int CurrentMaxPayloadKg { get; set; }       // After mods

    // Financial
    public decimal PurchasePrice { get; set; }         // What player paid
    public DateTime PurchaseDate { get; set; }
    public decimal CurrentValue { get; set; }          // Depreciated value
    public decimal TotalMaintenanceSpent { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<AircraftComponent> Components { get; set; }
    public ICollection<AircraftModification> Modifications { get; set; }
    public ICollection<MaintenanceLog> MaintenanceLogs { get; set; }
}

public enum AircraftStatus
{
    Available,
    InFlight,
    InMaintenance,
    Damaged,
    ForSale,
    Destroyed
}
```

#### 3. AircraftComponent (Repairable Parts)

```csharp
public class AircraftComponent
{
    public Guid Id { get; set; }

    public Guid OwnedAircraftId { get; set; }
    public OwnedAircraft OwnedAircraft { get; set; }

    // Component Identity
    public ComponentType Type { get; set; }        // Engine1, LeftWing, etc.
    public string Name { get; set; }               // "CFM56-5B Engine #1"

    // Condition
    public int Condition { get; set; }             // 0-100%
    public int WearRate { get; set; }              // Condition loss per flight hour

    // Lifecycle
    public int HoursSinceNew { get; set; }
    public int HoursSinceOverhaul { get; set; }
    public int OverhaulIntervalHours { get; set; } // TBO - Time Between Overhauls
    public DateTime InstallDate { get; set; }
    public DateTime? LastRepairDate { get; set; }

    // Damage
    public bool IsDamaged { get; set; }
    public string DamageDescription { get; set; }
    public int DamageSeverity { get; set; }        // 1-5 scale

    // Economics
    public decimal ReplacementCost { get; set; }
    public decimal RepairCostPerPercent { get; set; } // Cost to repair 1% condition
    public decimal OverhaulCost { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum ComponentType
{
    // Engines (supports up to 6 based on aircraft type)
    Engine1,
    Engine2,
    Engine3,
    Engine4,
    Engine5,
    Engine6,

    // Wings (single component per side for simplicity)
    LeftWing,
    RightWing,

    // Landing Gear (single component for entire gear system)
    LandingGear,

    // Fuselage (single component for entire fuselage)
    Fuselage
}
```

#### 4. AircraftModification (Customizations)

```csharp
public class AircraftModification
{
    public Guid Id { get; set; }

    public Guid OwnedAircraftId { get; set; }
    public OwnedAircraft OwnedAircraft { get; set; }

    // Modification Details
    public ModificationType Type { get; set; }
    public string Name { get; set; }               // "Economy to Cargo Conversion"
    public string Description { get; set; }

    // Configuration
    public int SeatsRemoved { get; set; }          // For cargo conversion
    public int CargoCapacityAddedKg { get; set; }  // Resulting cargo increase
    public int RangeModifierNm { get; set; }       // + or - range
    public int PayloadModifierKg { get; set; }     // + or - payload
    public decimal SpeedModifierPercent { get; set; } // % speed change

    // Installation
    public DateTime InstalledDate { get; set; }
    public decimal InstallationCost { get; set; }
    public int InstallationTimeHours { get; set; }
    public bool IsReversible { get; set; }
    public decimal RemovalCost { get; set; }

    // Status
    public bool IsActive { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
}

public enum ModificationType
{
    // Cargo Conversions
    CargoConversionPartial,    // Some seats removed
    CargoConversionFull,       // Full freighter conversion
    CombiConversion,           // Mixed pax/cargo

    // Range/Fuel
    AuxiliaryFuelTank,
    WingletInstallation,

    // Performance
    EngineUpgrade,
    AvionicsUpgrade,

    // Comfort/Capacity
    SeatReconfiguration,       // Change seat pitch/count
    InteriorRefurbishment,

    // Special
    MedicalEvacuation,
    VIPConfiguration,
    SurveyEquipment
}
```

#### 5. MaintenanceLog (History)

```csharp
public class MaintenanceLog
{
    public Guid Id { get; set; }

    public Guid OwnedAircraftId { get; set; }
    public OwnedAircraft OwnedAircraft { get; set; }

    public Guid? ComponentId { get; set; }         // Null if general maintenance
    public AircraftComponent Component { get; set; }

    // Details
    public MaintenanceType Type { get; set; }
    public string Description { get; set; }
    public int ConditionBefore { get; set; }
    public int ConditionAfter { get; set; }

    // Costs
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal TotalCost { get; set; }

    // Time
    public DateTime PerformedAt { get; set; }
    public int AircraftHoursAtTime { get; set; }
    public int DowntimeHours { get; set; }

    // Location
    public int AirportId { get; set; }
    public Airport Airport { get; set; }
}

public enum MaintenanceType
{
    Repair,
    Overhaul,
    Replacement,
    Inspection,
    ScheduledService,
    DamageRepair,
    ModificationInstall,
    ModificationRemoval
}
```

#### 6. ModificationTemplate (Available Mods Catalog)

```csharp
public class ModificationTemplate
{
    public Guid Id { get; set; }

    public ModificationType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Requirements
    public string RequiredAircraftTypes { get; set; }  // JSON array of compatible types
    public int MinimumAircraftCondition { get; set; }

    // Effects
    public int SeatsRemovedPerUnit { get; set; }       // e.g., 1 cargo unit = 4 seats
    public int CargoAddedPerUnit { get; set; }         // kg per unit
    public int MaxUnits { get; set; }                  // Max conversion units

    // Costs
    public decimal BaseCost { get; set; }
    public decimal CostPerUnit { get; set; }
    public int InstallationTimeHours { get; set; }

    // Properties
    public bool IsReversible { get; set; }
    public decimal RemovalCostPercent { get; set; }    // % of install cost

    public DateTime CreatedAt { get; set; }
}
```

---

## Cargo Conversion System Design

### Conversion Formula

```
Base Cargo Capacity = (Length_m × 2.5m × 2.0m) × 150 kg/m³
                    = Approximate hold volume × density factor

Per-Seat Cargo Gain = 90kg (weight) + 0.5m³ (volume)

Conversion Options:
1. Partial: Remove 25-50% of seats
2. Full Freighter: Remove all seats, add cargo door
3. Combi: Front cargo, rear passengers
```

### Estimated Passenger Capacities (by aircraft type code)

| Type Code | Description | Est. Passengers |
|-----------|-------------|-----------------|
| L1P | Land, 1 engine, Piston | 1-4 |
| L1T | Land, 1 engine, Turboprop | 4-9 |
| L2P | Land, 2 engines, Piston | 4-9 |
| L2T | Land, 2 engines, Turboprop | 19-78 |
| L2J | Land, 2 engines, Jet | 50-280 |
| L3J | Land, 3 engines, Jet | 150-250 |
| L4J | Land, 4 engines, Jet | 250-550 |
| L4T | Land, 4 engines, Turboprop | 40-100 |
| H1T/H2T | Helicopter | 2-15 |

### Conversion Presets (User Choice)

```
1. LIGHT CARGO (25% Conversion):
   - Removes 25% of passenger seats
   - Cargo Gain: 25% of max payload as cargo
   - Cost: Base price × 0.15
   - Reversible: Yes (25% cost to reverse)
   - Time: 24-48 hours

2. MIXED USE (50% Conversion):
   - Removes 50% of passenger seats
   - Cargo Gain: 50% of max payload as cargo
   - Cost: Base price × 0.30
   - Reversible: Yes (40% cost to reverse)
   - Time: 48-96 hours

3. CARGO PRIORITY (75% Conversion):
   - Removes 75% of passenger seats
   - Cargo Gain: 75% of max payload as cargo
   - Cost: Base price × 0.50
   - Reversible: Yes (60% cost to reverse)
   - Time: 96-200 hours

4. FULL FREIGHTER (100% Conversion):
   - Removes all passenger seats
   - Cargo Gain: Full payload capacity as cargo
   - Cost: Base price × 0.80
   - Reversible: No (major structural changes)
   - Time: 200-500 hours
   - Requires: Cargo door installation, floor reinforcement
```

### Cargo Conversion Enum

```csharp
public enum CargoConversionLevel
{
    None = 0,           // Standard passenger configuration
    Light = 25,         // 25% cargo conversion
    Mixed = 50,         // 50% cargo conversion
    CargoPriority = 75, // 75% cargo conversion
    FullFreighter = 100 // 100% cargo (no passengers)
}
```

---

## Component Wear & Repair System

### Condition Degradation

```
Per Flight Hour:
- Engine: -0.05% to -0.15% (depends on engine type)
- Wings: -0.01% (very slow wear)
- Landing Gear: -0.1% per landing cycle
- Fuselage: -0.02%
- Avionics: -0.03%

Per Hard Landing (> 600 fpm descent):
- Landing Gear: -5% to -20%
- Fuselage: -2% to -10%

Per Over-G Event:
- Wings: -5% to -15%
- Fuselage: -3% to -10%
```

### Repair Costs (Per Percent Restored)

Based on aircraft MTOW and component type:

```
Cost Per 1% Repair = BaseCost × MTOWFactor × ComponentFactor

MTOWFactor:
- < 5,700 kg (Light): 1.0
- 5,700 - 50,000 kg (Medium): 2.5
- 50,000 - 150,000 kg (Heavy): 5.0
- > 150,000 kg (Super): 10.0

ComponentFactor:
- Engine: 5.0
- Wing: 3.0
- Landing Gear: 2.0
- Fuselage: 4.0
- Avionics: 2.5

Example: A320 (MTOW 73,900 kg) Engine Repair
- BaseCost: $100
- MTOWFactor: 2.5 (Medium)
- ComponentFactor: 5.0 (Engine)
- Cost per 1%: $100 × 2.5 × 5.0 = $1,250
- Full engine repair (0→100%): $125,000
```

### Overhaul vs Repair

```
Repair: Restore condition, doesn't reset hours
- Available anytime
- Cost: Linear per percent

Overhaul: Full restoration + hours reset
- Required every TBO (Time Between Overhauls)
- Cost: 50-70% of replacement cost
- Resets HoursSinceOverhaul to 0
- Sets Condition to 100%

Replacement: Brand new component
- Condition: 100%
- Hours: 0
- Cost: Full component price
```

---

## Pricing Formula for Aircraft Purchase

```csharp
public decimal CalculatePurchasePrice(AircraftTemplate template)
{
    // Base price from MTOW (roughly $500-$1000 per kg)
    decimal basePrice = template.MtowKg * 750m;

    // Adjustments
    decimal engineFactor = template.EngineType switch
    {
        "Turbofan" => 1.5m,
        "Turboprop" => 1.2m,
        "Piston" => 0.8m,
        _ => 1.0m
    };

    decimal rangeFactor = 1.0m + (template.RangeNm / 10000m);
    decimal ageFactor = 0.85m; // Assume used aircraft

    return basePrice * engineFactor * rangeFactor * ageFactor;
}

// Examples:
// Cessna 172 (MTOW 1,111 kg): ~$700,000 (game currency)
// A320 (MTOW 73,900 kg): ~$83M
// B747 (MTOW 412,775 kg): ~$620M
```

---

## API Endpoints

### Aircraft Templates
```
GET    /api/aircraft-templates                    # List all available aircraft
GET    /api/aircraft-templates/{icao}             # Get specific template
GET    /api/aircraft-templates/search?q=          # Search by name/ICAO
```

### Owned Aircraft
```
GET    /api/my-aircraft                           # List user's aircraft
GET    /api/my-aircraft/{id}                      # Get specific owned aircraft
POST   /api/my-aircraft/purchase                  # Buy an aircraft
POST   /api/my-aircraft/{id}/sell                 # Sell an aircraft
PATCH  /api/my-aircraft/{id}                      # Update registration/nickname
```

### Components & Repairs
```
GET    /api/my-aircraft/{id}/components           # Get all components
GET    /api/my-aircraft/{id}/components/{type}    # Get specific component
POST   /api/my-aircraft/{id}/components/{type}/repair    # Repair component
POST   /api/my-aircraft/{id}/components/{type}/overhaul  # Overhaul component
POST   /api/my-aircraft/{id}/components/{type}/replace   # Replace component
```

### Modifications
```
GET    /api/modifications                         # List available modifications
GET    /api/my-aircraft/{id}/modifications        # Get installed mods
POST   /api/my-aircraft/{id}/modifications        # Install modification
DELETE /api/my-aircraft/{id}/modifications/{id}   # Remove modification
```

### Maintenance
```
GET    /api/my-aircraft/{id}/maintenance          # Get maintenance history
POST   /api/my-aircraft/{id}/service              # Perform scheduled service
GET    /api/my-aircraft/{id}/maintenance/estimate # Get repair cost estimate
```

---

## Migration Strategy

### Phase 1: Create New Tables
1. Create `AircraftTemplate` table
2. Import all 398 aircraft from CSV
3. Parse engine data to populate EngineCount, EngineType, etc.
4. Calculate estimated passenger capacities and pricing

### Phase 2: Ownership System
1. Create `OwnedAircraft` table
2. Create `AircraftComponent` table with default components
3. Add component initialization logic

### Phase 3: Modifications
1. Create `ModificationTemplate` catalog
2. Create `AircraftModification` table
3. Implement cargo conversion logic

### Phase 4: Maintenance
1. Create `MaintenanceLog` table
2. Implement wear calculation per flight
3. Add repair/overhaul endpoints

---

## Entity Relationship Diagram

```
┌─────────────────┐       ┌──────────────────┐
│      User       │       │ AircraftTemplate │
│─────────────────│       │──────────────────│
│ Id              │       │ Id               │
│ Balance         │       │ IcaoCode         │
│ ...             │       │ Performance...   │
└────────┬────────┘       │ Dimensions...    │
         │                │ Economics...     │
         │ 1:N            └────────┬─────────┘
         │                         │ 1:N
         ▼                         ▼
┌─────────────────────────────────────────────┐
│              OwnedAircraft                  │
│─────────────────────────────────────────────│
│ Id, OwnerId, TemplateId                     │
│ Registration, Nickname, Status              │
│ TotalFlightHours, OverallCondition          │
│ CurrentPassengerCapacity, CurrentCargoKg    │
│ PurchasePrice, CurrentValue                 │
└──────────┬─────────────────┬────────────────┘
           │ 1:N             │ 1:N
           ▼                 ▼
┌──────────────────┐  ┌────────────────────┐
│ AircraftComponent│  │AircraftModification│
│──────────────────│  │────────────────────│
│ Type (Engine1..) │  │ Type (Cargo...)    │
│ Condition 0-100  │  │ SeatsRemoved       │
│ HoursSinceNew    │  │ CargoCapacityAdded │
│ RepairCosts      │  │ InstallationCost   │
└──────────────────┘  └────────────────────┘
           │
           │ 1:N
           ▼
┌──────────────────┐
│  MaintenanceLog  │
│──────────────────│
│ Type, Cost       │
│ ConditionChange  │
│ PerformedAt      │
└──────────────────┘
```

---

## Files to Create/Modify

### New Entity Files
- `PilotLife.Database/Entities/AircraftTemplate.cs`
- `PilotLife.Database/Entities/OwnedAircraft.cs`
- `PilotLife.Database/Entities/AircraftComponent.cs`
- `PilotLife.Database/Entities/AircraftModification.cs`
- `PilotLife.Database/Entities/ModificationTemplate.cs`
- `PilotLife.Database/Entities/MaintenanceLog.cs`

### Modified Files
- `PilotLife.Database/Data/PilotLifeDbContext.cs` - Add DbSets and configurations
- `PilotLife.Database/Entities/User.cs` - Add navigation to OwnedAircraft

### New API Controllers
- `PilotLife.API/Controllers/AircraftTemplatesController.cs`
- `PilotLife.API/Controllers/OwnedAircraftController.cs`
- `PilotLife.API/Controllers/MaintenanceController.cs`
- `PilotLife.API/Controllers/ModificationsController.cs`

### New Services
- `PilotLife.API/Services/AircraftPricingService.cs`
- `PilotLife.API/Services/MaintenanceService.cs`
- `PilotLife.API/Services/AircraftImportService.cs` - Import from CSV

---

---

## Final Design Decisions

Based on user requirements:

1. **Component Complexity**: Core 4 only (Engines, Wings, Gear, Fuselage)
2. **Condition Impact**: Economic only - affects value and repair costs, not flight performance
3. **Cargo Conversion**: Preset options (25%, 50%, 75%, Full freighter)
4. **Migration Strategy**: Replace existing Aircraft entity with AircraftTemplate

---

## Summary

This hybrid approach provides:

1. **Template-Instance Pattern**: Base aircraft specs (AircraftTemplate) separate from player-owned instances (OwnedAircraft)

2. **Component-Level Tracking**: Individual parts (engines, wings, gear, fuselage) with independent condition and repair

3. **Flexible Modifications**: Cargo conversions and upgrades that modify aircraft stats while preserving base template

4. **Economic Depth**: Realistic pricing, maintenance costs, and depreciation

5. **Full History**: Maintenance logs for analytics and player reference

6. **Scalability**: Multiple players can own the same aircraft type with completely different configurations and conditions
