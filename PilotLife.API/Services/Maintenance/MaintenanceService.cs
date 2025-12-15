using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PilotLife.Application.Maintenance;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services.Maintenance;

public class MaintenanceService : IMaintenanceService
{
    private readonly PilotLifeDbContext _context;
    private readonly MaintenanceConfiguration _config;
    private readonly ILogger<MaintenanceService> _logger;

    public MaintenanceService(
        PilotLifeDbContext context,
        IOptions<MaintenanceConfiguration> config,
        ILogger<MaintenanceService> logger)
    {
        _context = context;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<MaintenanceStatusResult> GetMaintenanceStatusAsync(
        Guid ownedAircraftId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var aircraft = await _context.OwnedAircraft
            .Include(a => a.Owner)
            .Include(a => a.Components)
            .Include(a => a.MaintenanceLogs.Where(m => !m.IsCompleted))
            .FirstOrDefaultAsync(a => a.Id == ownedAircraftId, cancellationToken);

        if (aircraft == null || aircraft.Owner.UserId != userId)
        {
            throw new InvalidOperationException("Aircraft not found or not owned by user");
        }

        var activeMaintenance = aircraft.MaintenanceLogs.FirstOrDefault(m => !m.IsCompleted);

        // Get last inspections
        var lastAnnualInspection = await _context.MaintenanceLogs
            .Where(m => m.OwnedAircraftId == ownedAircraftId &&
                        m.MaintenanceType == MaintenanceType.AnnualInspection &&
                        m.IsCompleted)
            .OrderByDescending(m => m.CompletedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var lastHundredHour = await _context.MaintenanceLogs
            .Where(m => m.OwnedAircraftId == ownedAircraftId &&
                        m.MaintenanceType == MaintenanceType.HundredHourInspection &&
                        m.IsCompleted)
            .OrderByDescending(m => m.CompletedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Calculate if inspections are due
        var annualDue = lastAnnualInspection == null ||
                        lastAnnualInspection.CompletedAt?.AddDays(_config.AnnualInspectionIntervalDays) < DateTimeOffset.UtcNow;

        var hundredHourDue = aircraft.HoursSinceLastInspection >= _config.HundredHourInspectionMinutes;

        // Build component statuses
        var componentStatuses = aircraft.Components.Select(c => new ComponentStatus
        {
            ComponentId = c.Id,
            ComponentType = c.ComponentType,
            Condition = c.Condition,
            TboPercentUsed = c.TboPercentUsed,
            LifePercentUsed = c.LifePercentUsed,
            NeedsAttention = c.NeedsAttention,
            IsServiceable = c.IsServiceable
        }).ToList();

        // Build alerts
        var alerts = new List<MaintenanceAlert>();

        if (annualDue)
        {
            alerts.Add(new MaintenanceAlert
            {
                Severity = "critical",
                Message = "Annual inspection is overdue",
                RecommendedAction = "Schedule annual inspection immediately"
            });
        }

        if (hundredHourDue)
        {
            alerts.Add(new MaintenanceAlert
            {
                Severity = "warning",
                Message = "100-hour inspection is due",
                RecommendedAction = "Schedule 100-hour inspection before next flight"
            });
        }

        if (aircraft.Condition < _config.MinAirworthyCondition)
        {
            alerts.Add(new MaintenanceAlert
            {
                Severity = "critical",
                Message = $"Aircraft condition is below airworthy minimum ({aircraft.Condition}%)",
                RecommendedAction = "Major maintenance required before flight"
            });
        }
        else if (aircraft.Condition < 70)
        {
            alerts.Add(new MaintenanceAlert
            {
                Severity = "warning",
                Message = $"Aircraft condition is low ({aircraft.Condition}%)",
                RecommendedAction = "Consider scheduling maintenance soon"
            });
        }

        // Check components
        foreach (var comp in aircraft.Components.Where(c => c.NeedsAttention))
        {
            var severity = comp.Condition < _config.MinServiceableComponentCondition ? "critical" : "warning";
            var message = comp.IsTboExceeded
                ? $"{comp.ComponentType} has exceeded TBO"
                : comp.IsTboApproaching
                    ? $"{comp.ComponentType} is approaching TBO"
                    : $"{comp.ComponentType} condition is low ({comp.Condition}%)";

            alerts.Add(new MaintenanceAlert
            {
                Severity = severity,
                Message = message,
                RecommendedAction = $"Service or overhaul {comp.ComponentType}",
                ComponentId = comp.Id
            });
        }

        return new MaintenanceStatusResult
        {
            OwnedAircraftId = ownedAircraftId,
            OverallCondition = aircraft.Condition,
            IsAirworthy = aircraft.IsAirworthy,
            IsInMaintenance = aircraft.IsInMaintenance,
            HoursSinceLastInspection = aircraft.HoursSinceLastInspection,
            InspectionDue = hundredHourDue,
            AnnualInspectionDue = annualDue,
            LastInspectionDate = lastHundredHour?.CompletedAt,
            LastAnnualInspectionDate = lastAnnualInspection?.CompletedAt,
            ActiveMaintenance = activeMaintenance,
            Components = componentStatuses,
            Alerts = alerts
        };
    }

    public async Task<IEnumerable<MaintenanceOption>> GetAvailableMaintenanceAsync(
        Guid ownedAircraftId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var aircraft = await _context.OwnedAircraft
            .Include(a => a.Owner)
            .Include(a => a.Components)
            .FirstOrDefaultAsync(a => a.Id == ownedAircraftId, cancellationToken);

        if (aircraft == null || aircraft.Owner.UserId != userId)
        {
            throw new InvalidOperationException("Aircraft not found or not owned by user");
        }

        var options = new List<MaintenanceOption>();

        // Calculate cost modifiers based on condition
        var conditionModifier = 1 + (100 - aircraft.Condition) / 100m;

        // Annual Inspection
        options.Add(CreateMaintenanceOption(
            MaintenanceType.AnnualInspection,
            "Annual Inspection",
            "Comprehensive yearly inspection of all aircraft systems as required by regulations.",
            _config.AnnualInspectionBaseCost * conditionModifier,
            _config.AnnualInspectionDurationHours,
            _config.AnnualInspectionConditionImprovement,
            aircraft.HoursSinceLastInspection >= _config.HundredHourInspectionMinutes || aircraft.Condition < _config.MinAirworthyCondition,
            aircraft));

        // 100-Hour Inspection
        options.Add(CreateMaintenanceOption(
            MaintenanceType.HundredHourInspection,
            "100-Hour Inspection",
            "Periodic inspection required every 100 hours of flight time.",
            _config.HundredHourInspectionBaseCost * conditionModifier,
            _config.HundredHourInspectionDurationHours,
            _config.HundredHourInspectionConditionImprovement,
            aircraft.HoursSinceLastInspection >= _config.HundredHourInspectionMinutes,
            aircraft));

        // Minor Repair (if condition is below 90)
        if (aircraft.Condition < 90)
        {
            options.Add(CreateMaintenanceOption(
                MaintenanceType.MinorRepair,
                "Minor Repair",
                "Address minor issues and perform general maintenance to improve condition.",
                _config.MinorRepairBaseCost * conditionModifier,
                _config.MinorRepairDurationHours,
                _config.MinorRepairConditionImprovement,
                false,
                aircraft));
        }

        // Major Repair (if condition is below 70)
        if (aircraft.Condition < 70)
        {
            options.Add(CreateMaintenanceOption(
                MaintenanceType.MajorRepair,
                "Major Repair",
                "Comprehensive repair work to restore aircraft to good condition.",
                _config.MajorRepairBaseCost * conditionModifier,
                _config.MajorRepairDurationHours,
                _config.MajorRepairConditionImprovement,
                aircraft.Condition < _config.MinAirworthyCondition,
                aircraft));
        }

        // Component-specific options
        foreach (var component in aircraft.Components.Where(c => c.NeedsAttention))
        {
            var overhaul = GetComponentOverhaulOption(component, aircraft);
            if (overhaul != null)
            {
                options.Add(overhaul);
            }
        }

        return options;
    }

    public async Task<MaintenanceResult> ScheduleMaintenanceAsync(
        Guid ownedAircraftId,
        MaintenanceType maintenanceType,
        Guid userId,
        Guid? componentId = null,
        CancellationToken cancellationToken = default)
    {
        var aircraft = await _context.OwnedAircraft
            .Include(a => a.Owner)
            .Include(a => a.Components)
            .FirstOrDefaultAsync(a => a.Id == ownedAircraftId, cancellationToken);

        if (aircraft == null || aircraft.Owner.UserId != userId)
        {
            return new MaintenanceResult
            {
                Success = false,
                Message = "Aircraft not found or not owned by user"
            };
        }

        if (aircraft.IsInMaintenance)
        {
            return new MaintenanceResult
            {
                Success = false,
                Message = "Aircraft is already in maintenance"
            };
        }

        if (aircraft.IsInUse)
        {
            return new MaintenanceResult
            {
                Success = false,
                Message = "Aircraft is currently in use"
            };
        }

        // Get the player world for balance check
        var playerWorld = aircraft.Owner;
        var estimatedCost = CalculateMaintenanceCost(maintenanceType, aircraft, componentId);
        var (totalCost, outOfPocketCost) = CalculateCoverages(aircraft, maintenanceType, estimatedCost);

        if (playerWorld.Balance < outOfPocketCost)
        {
            return new MaintenanceResult
            {
                Success = false,
                Message = $"Insufficient funds. Maintenance costs ${outOfPocketCost:N0} but balance is ${playerWorld.Balance:N0}"
            };
        }

        var duration = GetMaintenanceDuration(maintenanceType);
        var component = componentId.HasValue
            ? aircraft.Components.FirstOrDefault(c => c.Id == componentId)
            : null;

        var maintenanceLog = new MaintenanceLog
        {
            WorldId = aircraft.WorldId,
            OwnedAircraftId = ownedAircraftId,
            AircraftComponentId = componentId,
            MaintenanceType = maintenanceType,
            Title = GetMaintenanceTitle(maintenanceType, component),
            Description = GetMaintenanceDescription(maintenanceType, component),
            PerformedAtAirport = aircraft.CurrentLocationIcao,
            AircraftFlightMinutesAtService = aircraft.TotalFlightMinutes,
            AircraftCyclesAtService = aircraft.TotalCycles,
            EstimatedDurationHours = duration,
            LaborCost = totalCost * 0.6m, // 60% labor
            PartsCost = totalCost * 0.4m, // 40% parts
            CoveredByWarranty = aircraft.HasWarranty && aircraft.WarrantyExpiresAt > DateTimeOffset.UtcNow,
            CoveredByInsurance = aircraft.HasInsurance && maintenanceType == MaintenanceType.DamageRepair,
            ConditionImprovement = GetConditionImprovement(maintenanceType)
        };

        _context.MaintenanceLogs.Add(maintenanceLog);

        // Update aircraft status
        aircraft.IsInMaintenance = true;

        // Deduct cost from player balance
        playerWorld.Balance -= outOfPocketCost;

        await _context.SaveChangesAsync(cancellationToken);

        var estimatedCompletion = DateTimeOffset.UtcNow.AddHours(duration);

        _logger.LogInformation(
            "Scheduled {MaintenanceType} for aircraft {AircraftId} at {Airport}. Cost: ${Cost:N0}, Duration: {Hours}h",
            maintenanceType, ownedAircraftId, aircraft.CurrentLocationIcao, outOfPocketCost, duration);

        return new MaintenanceResult
        {
            Success = true,
            MaintenanceLogId = maintenanceLog.Id,
            Message = $"Maintenance scheduled. Estimated completion: {estimatedCompletion:g}",
            EstimatedCompletion = estimatedCompletion,
            TotalCost = totalCost,
            OutOfPocketCost = outOfPocketCost
        };
    }

    public async Task<MaintenanceResult> CompleteMaintenanceAsync(
        Guid maintenanceLogId,
        CancellationToken cancellationToken = default)
    {
        var maintenanceLog = await _context.MaintenanceLogs
            .Include(m => m.OwnedAircraft)
                .ThenInclude(a => a.Components)
            .Include(m => m.AircraftComponent)
            .FirstOrDefaultAsync(m => m.Id == maintenanceLogId, cancellationToken);

        if (maintenanceLog == null)
        {
            return new MaintenanceResult
            {
                Success = false,
                Message = "Maintenance record not found"
            };
        }

        if (maintenanceLog.IsCompleted)
        {
            return new MaintenanceResult
            {
                Success = false,
                Message = "Maintenance is already completed"
            };
        }

        var aircraft = maintenanceLog.OwnedAircraft;

        // Apply condition improvements
        var improvement = maintenanceLog.ConditionImprovement;
        aircraft.Condition = Math.Min(100, aircraft.Condition + improvement);

        // Reset inspection counters if this was an inspection
        if (maintenanceLog.MaintenanceType == MaintenanceType.HundredHourInspection ||
            maintenanceLog.MaintenanceType == MaintenanceType.AnnualInspection)
        {
            aircraft.HoursSinceLastInspection = 0;
        }

        // Handle component-specific maintenance
        if (maintenanceLog.AircraftComponent != null)
        {
            var component = maintenanceLog.AircraftComponent;

            if (maintenanceLog.MaintenanceType == MaintenanceType.EngineOverhaul ||
                maintenanceLog.MaintenanceType == MaintenanceType.PropellerOverhaul)
            {
                // Reset TBO
                component.TimeSinceOverhaul = 0;
                component.Condition = 95; // Overhauled, not new
            }
            else if (maintenanceLog.MaintenanceType == MaintenanceType.ComponentReplacement)
            {
                // New component
                component.Condition = 100;
                component.OperatingMinutes = 0;
                component.TimeSinceOverhaul = 0;
                component.Cycles = 0;
            }
            else
            {
                // General improvement
                component.Condition = Math.Min(100, component.Condition + improvement);
            }

            component.IsServiceable = component.Condition >= _config.MinServiceableComponentCondition;
        }

        // Mark maintenance as complete
        maintenanceLog.IsCompleted = true;
        maintenanceLog.CompletedAt = DateTimeOffset.UtcNow;
        maintenanceLog.ActualDurationHours = (int)(DateTimeOffset.UtcNow - maintenanceLog.StartedAt).TotalHours;
        maintenanceLog.ResultingCondition = aircraft.Condition;

        // Clear in-maintenance flag
        aircraft.IsInMaintenance = false;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Completed {MaintenanceType} for aircraft {AircraftId}. New condition: {Condition}%",
            maintenanceLog.MaintenanceType, aircraft.Id, aircraft.Condition);

        return new MaintenanceResult
        {
            Success = true,
            MaintenanceLogId = maintenanceLogId,
            Message = $"Maintenance completed. Aircraft condition: {aircraft.Condition}%"
        };
    }

    public async Task<MaintenanceResult> CancelMaintenanceAsync(
        Guid maintenanceLogId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var maintenanceLog = await _context.MaintenanceLogs
            .Include(m => m.OwnedAircraft)
                .ThenInclude(a => a.Owner)
            .FirstOrDefaultAsync(m => m.Id == maintenanceLogId, cancellationToken);

        if (maintenanceLog == null)
        {
            return new MaintenanceResult
            {
                Success = false,
                Message = "Maintenance record not found"
            };
        }

        if (maintenanceLog.OwnedAircraft.Owner.UserId != userId)
        {
            return new MaintenanceResult
            {
                Success = false,
                Message = "Not authorized to cancel this maintenance"
            };
        }

        if (maintenanceLog.IsCompleted)
        {
            return new MaintenanceResult
            {
                Success = false,
                Message = "Cannot cancel completed maintenance"
            };
        }

        // Check if work has already started (more than 1 hour in)
        var hoursElapsed = (DateTimeOffset.UtcNow - maintenanceLog.StartedAt).TotalHours;
        if (hoursElapsed > 1)
        {
            return new MaintenanceResult
            {
                Success = false,
                Message = "Cannot cancel maintenance after work has begun. Wait for completion."
            };
        }

        // Refund 80% of cost (cancellation fee)
        var refund = maintenanceLog.TotalCost * 0.8m;
        maintenanceLog.OwnedAircraft.Owner.Balance += refund;

        // Clear maintenance
        maintenanceLog.OwnedAircraft.IsInMaintenance = false;
        _context.MaintenanceLogs.Remove(maintenanceLog);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cancelled maintenance {MaintenanceId} for aircraft {AircraftId}. Refunded ${Refund:N0}",
            maintenanceLogId, maintenanceLog.OwnedAircraftId, refund);

        return new MaintenanceResult
        {
            Success = true,
            Message = $"Maintenance cancelled. ${refund:N0} refunded (20% cancellation fee applied)"
        };
    }

    public async Task<IEnumerable<MaintenanceLog>> GetMaintenanceHistoryAsync(
        Guid ownedAircraftId,
        Guid userId,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var aircraft = await _context.OwnedAircraft
            .Include(a => a.Owner)
            .FirstOrDefaultAsync(a => a.Id == ownedAircraftId, cancellationToken);

        if (aircraft == null || aircraft.Owner.UserId != userId)
        {
            throw new InvalidOperationException("Aircraft not found or not owned by user");
        }

        return await _context.MaintenanceLogs
            .Include(m => m.AircraftComponent)
            .Where(m => m.OwnedAircraftId == ownedAircraftId)
            .OrderByDescending(m => m.StartedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task ApplyFlightWearAsync(
        Guid ownedAircraftId,
        int flightMinutes,
        int? landingRate,
        bool hadOverspeed = false,
        bool hadStallWarning = false,
        CancellationToken cancellationToken = default)
    {
        var aircraft = await _context.OwnedAircraft
            .Include(a => a.Components)
            .FirstOrDefaultAsync(a => a.Id == ownedAircraftId, cancellationToken);

        if (aircraft == null)
        {
            _logger.LogWarning("Cannot apply flight wear - aircraft {AircraftId} not found", ownedAircraftId);
            return;
        }

        var flightHours = flightMinutes / 60.0;

        // Calculate base degradation
        var baseDegradation = flightHours * _config.BaseConditionDegradationPerHour;

        // Add penalties
        var totalDegradation = baseDegradation;

        if (landingRate.HasValue && landingRate.Value < -600)
        {
            totalDegradation += _config.HardLandingConditionPenalty;
            _logger.LogInformation("Hard landing detected ({Rate} fpm) - applying {Penalty}% penalty",
                landingRate.Value, _config.HardLandingConditionPenalty);
        }

        if (hadOverspeed)
        {
            totalDegradation += _config.OverspeedConditionPenalty;
        }

        // Apply to aircraft
        aircraft.Condition = Math.Max(0, aircraft.Condition - (int)Math.Ceiling(totalDegradation));
        aircraft.HoursSinceLastInspection += flightMinutes;

        // Apply to components (slightly faster degradation)
        var componentDegradation = flightHours * _config.ComponentDegradationPerHour;
        foreach (var component in aircraft.Components)
        {
            component.Condition = Math.Max(0, component.Condition - (int)Math.Ceiling(componentDegradation));
            component.OperatingMinutes += flightMinutes;
            component.TimeSinceOverhaul += flightMinutes;

            // Hard landing affects landing gear more
            if (landingRate.HasValue && landingRate.Value < -600 &&
                component.ComponentType == ComponentType.LandingGear)
            {
                component.Condition = Math.Max(0, component.Condition - (int)_config.HardLandingConditionPenalty);
                component.Cycles++;
            }

            // Update serviceability
            component.IsServiceable = component.Condition >= _config.MinServiceableComponentCondition;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Applied flight wear to aircraft {AircraftId}: {Hours:F1}h flight, {Degradation:F1}% degradation. New condition: {Condition}%",
            ownedAircraftId, flightHours, totalDegradation, aircraft.Condition);
    }

    public async Task<MaintenanceLog?> GetActiveMaintenanceAsync(
        Guid ownedAircraftId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MaintenanceLogs
            .Include(m => m.AircraftComponent)
            .FirstOrDefaultAsync(m =>
                m.OwnedAircraftId == ownedAircraftId &&
                !m.IsCompleted,
                cancellationToken);
    }

    // ========================================
    // Private Helper Methods
    // ========================================

    private MaintenanceOption CreateMaintenanceOption(
        MaintenanceType type,
        string title,
        string description,
        decimal cost,
        int durationHours,
        int conditionImprovement,
        bool required,
        OwnedAircraft aircraft)
    {
        var (_, outOfPocket) = CalculateCoverages(aircraft, type, cost);

        return new MaintenanceOption
        {
            MaintenanceType = type,
            Title = title,
            Description = description,
            EstimatedCost = outOfPocket,
            EstimatedDurationHours = durationHours,
            ConditionImprovement = conditionImprovement,
            RequiredForAirworthy = required,
            WarrantyCoverage = aircraft.HasWarranty && aircraft.WarrantyExpiresAt > DateTimeOffset.UtcNow
                ? _config.WarrantyCoveragePercent
                : null,
            InsuranceCoverage = aircraft.HasInsurance && type == MaintenanceType.DamageRepair
                ? _config.InsuranceCoveragePercent
                : null
        };
    }

    private MaintenanceOption? GetComponentOverhaulOption(AircraftComponent component, OwnedAircraft aircraft)
    {
        var type = component.ComponentType switch
        {
            ComponentType.Engine1 or ComponentType.Engine2 or ComponentType.Engine3 or
            ComponentType.Engine4 or ComponentType.Engine5 or ComponentType.Engine6 => MaintenanceType.EngineOverhaul,
            ComponentType.Propeller => MaintenanceType.PropellerOverhaul,
            ComponentType.LandingGear => MaintenanceType.LandingGearService,
            ComponentType.Avionics => MaintenanceType.AvionicsUpdate,
            _ => MaintenanceType.ComponentReplacement
        };

        var baseCost = type switch
        {
            MaintenanceType.EngineOverhaul => 25000m,
            MaintenanceType.PropellerOverhaul => 8000m,
            MaintenanceType.LandingGearService => 5000m,
            MaintenanceType.AvionicsUpdate => 3000m,
            _ => 2000m
        };

        var duration = type switch
        {
            MaintenanceType.EngineOverhaul => _config.EngineOverhaulDurationHours,
            MaintenanceType.PropellerOverhaul => 40,
            MaintenanceType.LandingGearService => 16,
            MaintenanceType.AvionicsUpdate => 8,
            _ => 8
        };

        return new MaintenanceOption
        {
            MaintenanceType = type,
            Title = $"{component.ComponentType} Service",
            Description = $"Service or overhaul the {component.ComponentType}. Current condition: {component.Condition}%",
            EstimatedCost = baseCost,
            EstimatedDurationHours = duration,
            ConditionImprovement = 0, // Component-specific, not overall
            RequiredForAirworthy = !component.IsServiceable,
            ComponentId = component.Id
        };
    }

    private decimal CalculateMaintenanceCost(MaintenanceType type, OwnedAircraft aircraft, Guid? componentId)
    {
        var conditionModifier = 1 + (100 - aircraft.Condition) / 100m;

        return type switch
        {
            MaintenanceType.AnnualInspection => _config.AnnualInspectionBaseCost * conditionModifier,
            MaintenanceType.HundredHourInspection => _config.HundredHourInspectionBaseCost * conditionModifier,
            MaintenanceType.MinorRepair => _config.MinorRepairBaseCost * conditionModifier,
            MaintenanceType.MajorRepair => _config.MajorRepairBaseCost * conditionModifier,
            MaintenanceType.EngineOverhaul => 25000m,
            MaintenanceType.PropellerOverhaul => 8000m,
            MaintenanceType.LandingGearService => 5000m,
            MaintenanceType.AvionicsUpdate => 3000m,
            MaintenanceType.DamageRepair => _config.MajorRepairBaseCost * 1.5m,
            _ => _config.MinorRepairBaseCost
        };
    }

    private (decimal totalCost, decimal outOfPocket) CalculateCoverages(
        OwnedAircraft aircraft,
        MaintenanceType type,
        decimal cost)
    {
        var outOfPocket = cost;

        // Warranty covers defects (not damage)
        if (aircraft.HasWarranty &&
            aircraft.WarrantyExpiresAt > DateTimeOffset.UtcNow &&
            type != MaintenanceType.DamageRepair)
        {
            outOfPocket *= (1 - _config.WarrantyCoveragePercent / 100m);
        }

        // Insurance covers damage repairs
        if (aircraft.HasInsurance &&
            aircraft.InsuranceExpiresAt > DateTimeOffset.UtcNow &&
            type == MaintenanceType.DamageRepair)
        {
            outOfPocket *= (1 - _config.InsuranceCoveragePercent / 100m);
        }

        return (cost, outOfPocket);
    }

    private int GetMaintenanceDuration(MaintenanceType type) => type switch
    {
        MaintenanceType.AnnualInspection => _config.AnnualInspectionDurationHours,
        MaintenanceType.HundredHourInspection => _config.HundredHourInspectionDurationHours,
        MaintenanceType.MinorRepair => _config.MinorRepairDurationHours,
        MaintenanceType.MajorRepair => _config.MajorRepairDurationHours,
        MaintenanceType.EngineOverhaul => _config.EngineOverhaulDurationHours,
        MaintenanceType.PropellerOverhaul => 40,
        MaintenanceType.LandingGearService => 16,
        MaintenanceType.AvionicsUpdate => 8,
        MaintenanceType.DamageRepair => _config.MajorRepairDurationHours,
        _ => _config.MinorRepairDurationHours
    };

    private int GetConditionImprovement(MaintenanceType type) => type switch
    {
        MaintenanceType.AnnualInspection => _config.AnnualInspectionConditionImprovement,
        MaintenanceType.HundredHourInspection => _config.HundredHourInspectionConditionImprovement,
        MaintenanceType.MinorRepair => _config.MinorRepairConditionImprovement,
        MaintenanceType.MajorRepair => _config.MajorRepairConditionImprovement,
        MaintenanceType.DamageRepair => _config.MajorRepairConditionImprovement,
        _ => 0 // Component-specific maintenance doesn't improve overall condition
    };

    private static string GetMaintenanceTitle(MaintenanceType type, AircraftComponent? component)
    {
        if (component != null)
        {
            return $"{type} - {component.ComponentType}";
        }

        return type switch
        {
            MaintenanceType.AnnualInspection => "Annual Inspection",
            MaintenanceType.HundredHourInspection => "100-Hour Inspection",
            MaintenanceType.MinorRepair => "Minor Repair",
            MaintenanceType.MajorRepair => "Major Repair",
            MaintenanceType.EngineOverhaul => "Engine Overhaul",
            MaintenanceType.PropellerOverhaul => "Propeller Overhaul",
            MaintenanceType.LandingGearService => "Landing Gear Service",
            MaintenanceType.AvionicsUpdate => "Avionics Update",
            MaintenanceType.DamageRepair => "Damage Repair",
            _ => type.ToString()
        };
    }

    private static string GetMaintenanceDescription(MaintenanceType type, AircraftComponent? component)
    {
        if (component != null)
        {
            return $"Maintenance work on {component.ComponentType}";
        }

        return type switch
        {
            MaintenanceType.AnnualInspection => "Comprehensive inspection of all aircraft systems as required by regulations.",
            MaintenanceType.HundredHourInspection => "Periodic inspection required every 100 hours of flight time.",
            MaintenanceType.MinorRepair => "Minor maintenance and adjustments to improve aircraft condition.",
            MaintenanceType.MajorRepair => "Significant repair work to restore aircraft condition.",
            MaintenanceType.EngineOverhaul => "Complete engine teardown, inspection, and rebuild.",
            MaintenanceType.PropellerOverhaul => "Propeller inspection and overhaul.",
            MaintenanceType.LandingGearService => "Landing gear inspection, service, and repair.",
            MaintenanceType.AvionicsUpdate => "Avionics systems update and maintenance.",
            MaintenanceType.DamageRepair => "Repair of damage to aircraft.",
            _ => "General maintenance work."
        };
    }
}
