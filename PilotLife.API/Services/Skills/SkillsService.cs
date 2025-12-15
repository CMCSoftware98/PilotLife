using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PilotLife.Application.Skills;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services.Skills;

public class SkillsService : ISkillsService
{
    private readonly PilotLifeDbContext _context;
    private readonly SkillsConfiguration _config;
    private readonly ILogger<SkillsService> _logger;

    public SkillsService(
        PilotLifeDbContext context,
        IOptions<SkillsConfiguration> config,
        ILogger<SkillsService> logger)
    {
        _context = context;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<PlayerSkillStatus>> GetAllSkillsAsync(
        Guid playerWorldId,
        CancellationToken cancellationToken = default)
    {
        var skills = await _context.PlayerSkills
            .Where(s => s.PlayerWorldId == playerWorldId)
            .ToListAsync(cancellationToken);

        // If no skills exist, initialize them
        if (skills.Count == 0)
        {
            await InitializeSkillsAsync(playerWorldId, cancellationToken);
            skills = await _context.PlayerSkills
                .Where(s => s.PlayerWorldId == playerWorldId)
                .ToListAsync(cancellationToken);
        }

        return skills.Select(MapToStatus).OrderBy(s => s.SkillType);
    }

    public async Task<PlayerSkillStatus?> GetSkillAsync(
        Guid playerWorldId,
        SkillType skillType,
        CancellationToken cancellationToken = default)
    {
        var skill = await _context.PlayerSkills
            .FirstOrDefaultAsync(
                s => s.PlayerWorldId == playerWorldId && s.SkillType == skillType,
                cancellationToken);

        if (skill == null)
        {
            // Try to initialize and fetch again
            await InitializeSkillsAsync(playerWorldId, cancellationToken);
            skill = await _context.PlayerSkills
                .FirstOrDefaultAsync(
                    s => s.PlayerWorldId == playerWorldId && s.SkillType == skillType,
                    cancellationToken);
        }

        return skill != null ? MapToStatus(skill) : null;
    }

    public async Task<SkillXpResult> AddXpAsync(
        Guid playerWorldId,
        SkillType skillType,
        int xp,
        string source,
        string? description = null,
        Guid? relatedFlightId = null,
        Guid? relatedJobId = null,
        CancellationToken cancellationToken = default)
    {
        if (xp <= 0)
        {
            return new SkillXpResult
            {
                Success = false,
                SkillType = skillType,
                XpGained = 0,
                TotalXp = 0,
                Level = 0,
                LevelsGained = 0,
                Message = "XP must be positive"
            };
        }

        var skill = await _context.PlayerSkills
            .FirstOrDefaultAsync(
                s => s.PlayerWorldId == playerWorldId && s.SkillType == skillType,
                cancellationToken);

        if (skill == null)
        {
            // Initialize skills if they don't exist
            await InitializeSkillsAsync(playerWorldId, cancellationToken);
            skill = await _context.PlayerSkills
                .FirstOrDefaultAsync(
                    s => s.PlayerWorldId == playerWorldId && s.SkillType == skillType,
                    cancellationToken);

            if (skill == null)
            {
                return new SkillXpResult
                {
                    Success = false,
                    SkillType = skillType,
                    XpGained = 0,
                    TotalXp = 0,
                    Level = 0,
                    LevelsGained = 0,
                    Message = "Failed to initialize skill"
                };
            }
        }

        var oldLevel = skill.Level;
        var levelsGained = skill.AddXp(xp);

        // Create XP event
        var xpEvent = new SkillXpEvent
        {
            PlayerSkillId = skill.Id,
            XpGained = xp,
            ResultingXp = skill.CurrentXp,
            ResultingLevel = skill.Level,
            CausedLevelUp = levelsGained > 0,
            Source = source,
            Description = description,
            RelatedFlightId = relatedFlightId,
            RelatedJobId = relatedJobId
        };

        _context.SkillXpEvents.Add(xpEvent);
        await _context.SaveChangesAsync(cancellationToken);

        if (levelsGained > 0)
        {
            _logger.LogInformation(
                "Player {PlayerWorldId} gained {Levels} level(s) in {Skill} (now level {Level})",
                playerWorldId, levelsGained, skillType, skill.Level);
        }

        return new SkillXpResult
        {
            Success = true,
            SkillType = skillType,
            XpGained = xp,
            TotalXp = skill.CurrentXp,
            Level = skill.Level,
            LevelsGained = levelsGained,
            Message = levelsGained > 0
                ? $"Level up! {GetSkillName(skillType)} is now level {skill.Level} ({skill.LevelName})"
                : null
        };
    }

    public async Task<IEnumerable<SkillXpEvent>> GetXpHistoryAsync(
        Guid playerWorldId,
        SkillType? skillType = null,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SkillXpEvents
            .Include(e => e.PlayerSkill)
            .Where(e => e.PlayerSkill.PlayerWorldId == playerWorldId);

        if (skillType.HasValue)
        {
            query = query.Where(e => e.PlayerSkill.SkillType == skillType.Value);
        }

        return await query
            .OrderByDescending(e => e.OccurredAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task InitializeSkillsAsync(
        Guid playerWorldId,
        CancellationToken cancellationToken = default)
    {
        // Check if skills already exist
        var existingCount = await _context.PlayerSkills
            .CountAsync(s => s.PlayerWorldId == playerWorldId, cancellationToken);

        if (existingCount > 0)
        {
            return; // Already initialized
        }

        // Create all skills
        var skillTypes = Enum.GetValues<SkillType>();
        foreach (var skillType in skillTypes)
        {
            var skill = PlayerSkill.Create(playerWorldId, skillType);
            _context.PlayerSkills.Add(skill);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Initialized {Count} skills for player {PlayerWorldId}",
            skillTypes.Length, playerWorldId);
    }

    public async Task ProcessFlightCompletedAsync(
        Guid playerWorldId,
        TrackedFlight flight,
        CancellationToken cancellationToken = default)
    {
        var flightHours = flight.FlightTimeMinutes / 60.0;

        // Piloting XP from flight time
        if (flight.FlightTimeMinutes > 0)
        {
            var pilotingXp = (int)(flightHours * _config.XpPerFlightHour);
            if (pilotingXp > 0)
            {
                await AddXpAsync(
                    playerWorldId,
                    SkillType.Piloting,
                    pilotingXp,
                    "Flight time",
                    $"{flightHours:F1} hours of flight",
                    relatedFlightId: flight.Id,
                    cancellationToken: cancellationToken);
            }
        }

        // Navigation XP from distance
        if (flight.DistanceNm > 0)
        {
            var navXp = (int)(flight.DistanceNm / 10 * _config.XpPerTenNm);
            if (navXp > 0)
            {
                await AddXpAsync(
                    playerWorldId,
                    SkillType.Navigation,
                    navXp,
                    "Distance flown",
                    $"{flight.DistanceNm:F0} nm flown",
                    relatedFlightId: flight.Id,
                    cancellationToken: cancellationToken);
            }
        }

        // Landing quality XP
        if (flight.LandingRate.HasValue)
        {
            var landingRate = Math.Abs(flight.LandingRate.Value);

            if (landingRate < 100)
            {
                await AddXpAsync(
                    playerWorldId,
                    SkillType.Piloting,
                    _config.XpSmoothLanding,
                    "Smooth landing",
                    $"Landing at {landingRate:F0} fpm",
                    relatedFlightId: flight.Id,
                    cancellationToken: cancellationToken);
            }
            else if (landingRate < 200)
            {
                await AddXpAsync(
                    playerWorldId,
                    SkillType.Piloting,
                    _config.XpGoodLanding,
                    "Good landing",
                    $"Landing at {landingRate:F0} fpm",
                    relatedFlightId: flight.Id,
                    cancellationToken: cancellationToken);
            }
        }

        // Aircraft Knowledge XP (first time in aircraft type)
        if (!string.IsNullOrEmpty(flight.AircraftTitle))
        {
            var hasFlownBefore = await _context.TrackedFlights
                .AnyAsync(f =>
                    f.UserId == flight.UserId &&
                    f.AircraftTitle == flight.AircraftTitle &&
                    f.Id != flight.Id &&
                    f.State == FlightState.Shutdown,
                    cancellationToken);

            if (!hasFlownBefore)
            {
                await AddXpAsync(
                    playerWorldId,
                    SkillType.AircraftKnowledge,
                    _config.XpNewAircraftTypeBonus,
                    "New aircraft type",
                    $"First flight in {flight.AircraftTitle}",
                    relatedFlightId: flight.Id,
                    cancellationToken: cancellationToken);
            }
        }

        // TODO: Add Night Flying XP when we have time-of-day tracking
        // TODO: Add Weather Flying XP when we have weather/IFR tracking
        // TODO: Add Mountain Flying XP when we have airport elevation data
    }

    public async Task ProcessJobCompletedAsync(
        Guid playerWorldId,
        Job job,
        CancellationToken cancellationToken = default)
    {
        if (job.IsFailed || !job.IsCompleted)
        {
            return;
        }

        // Cargo or Passenger skill XP
        var skillType = job.Type == JobType.Cargo
            ? SkillType.CargoHandling
            : SkillType.PassengerService;

        var baseXp = job.Type == JobType.Cargo
            ? _config.XpPerCargoJob
            : _config.XpPerPassengerJob;

        await AddXpAsync(
            playerWorldId,
            skillType,
            baseXp,
            $"{job.Type} job completed",
            $"Delivered {job.Title}",
            relatedJobId: job.Id,
            cancellationToken: cancellationToken);

        // Bonus XP for high-risk jobs
        if (job.RiskLevel >= 4)
        {
            await AddXpAsync(
                playerWorldId,
                skillType,
                _config.XpHighRiskJobBonus,
                "High-risk job bonus",
                $"Completed risk level {job.RiskLevel} job",
                relatedJobId: job.Id,
                cancellationToken: cancellationToken);
        }

        // VIP passenger bonus
        if (job.Type == JobType.Passenger && job.PassengerClass == PassengerClass.First)
        {
            await AddXpAsync(
                playerWorldId,
                SkillType.PassengerService,
                _config.XpVipJobBonus,
                "VIP passenger bonus",
                "First class passenger service",
                relatedJobId: job.Id,
                cancellationToken: cancellationToken);
        }
    }

    public async Task<int> GetTotalSkillLevelAsync(
        Guid playerWorldId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PlayerSkills
            .Where(s => s.PlayerWorldId == playerWorldId)
            .SumAsync(s => s.Level, cancellationToken);
    }

    // Private helper methods

    private PlayerSkillStatus MapToStatus(PlayerSkill skill)
    {
        return new PlayerSkillStatus
        {
            SkillId = skill.Id,
            SkillType = skill.SkillType,
            SkillName = GetSkillName(skill.SkillType),
            Description = GetSkillDescription(skill.SkillType),
            CurrentXp = skill.CurrentXp,
            Level = skill.Level,
            LevelName = skill.LevelName,
            XpForNextLevel = skill.XpForNextLevel,
            XpForCurrentLevel = skill.XpForCurrentLevel,
            ProgressToNextLevel = skill.ProgressToNextLevel,
            IsMaxLevel = skill.Level >= _config.MaxLevel
        };
    }

    private static string GetSkillName(SkillType skillType) => skillType switch
    {
        SkillType.Piloting => "Piloting",
        SkillType.Navigation => "Navigation",
        SkillType.CargoHandling => "Cargo Handling",
        SkillType.PassengerService => "Passenger Service",
        SkillType.AircraftKnowledge => "Aircraft Knowledge",
        SkillType.WeatherFlying => "Weather Flying",
        SkillType.NightFlying => "Night Flying",
        SkillType.MountainFlying => "Mountain Flying",
        _ => skillType.ToString()
    };

    private static string GetSkillDescription(SkillType skillType) => skillType switch
    {
        SkillType.Piloting => "Overall flying proficiency - leveled by flight hours and smooth landings",
        SkillType.Navigation => "Route planning and efficiency - leveled by distance flown",
        SkillType.CargoHandling => "Expertise in cargo transport - leveled by completing cargo jobs",
        SkillType.PassengerService => "Expertise in passenger transport - leveled by completing passenger jobs",
        SkillType.AircraftKnowledge => "Familiarity with aircraft types - leveled by flying different aircraft",
        SkillType.WeatherFlying => "Competence in IFR and adverse weather - leveled by IFR flights",
        SkillType.NightFlying => "Competence in night operations - leveled by night flights",
        SkillType.MountainFlying => "High altitude and terrain operations - leveled by mountain airport flights",
        _ => "Skill description"
    };
}
