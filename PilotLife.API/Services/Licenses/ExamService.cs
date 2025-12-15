using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services.Licenses;

public class ExamService
{
    private readonly PilotLifeDbContext _context;
    private readonly LicenseService _licenseService;
    private readonly ILogger<ExamService> _logger;

    public ExamService(
        PilotLifeDbContext context,
        LicenseService licenseService,
        ILogger<ExamService> logger)
    {
        _context = context;
        _licenseService = licenseService;
        _logger = logger;
    }

    /// <summary>
    /// Schedules a new exam for a player.
    /// </summary>
    public async Task<(bool Success, string Message, LicenseExam? Exam)> ScheduleExamAsync(
        Guid playerWorldId,
        string licenseCode,
        string departureIcao,
        CancellationToken ct = default)
    {
        var licenseType = await _licenseService.GetLicenseTypeByCodeAsync(licenseCode, ct);
        if (licenseType == null)
            return (false, $"License type '{licenseCode}' not found", null);

        var playerWorld = await _context.PlayerWorlds
            .Include(pw => pw.World)
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId, ct);

        if (playerWorld == null)
            return (false, "Player world not found", null);

        // Check if player already has this license
        if (await _licenseService.HasValidLicenseAsync(playerWorldId, licenseCode, ct))
            return (false, "You already have this license", null);

        // Check prerequisites
        var (hasPrereqs, missing) = await _licenseService.CheckPrerequisitesAsync(playerWorldId, licenseType, ct);
        if (!hasPrereqs)
            return (false, $"Missing prerequisite licenses: {string.Join(", ", missing)}", null);

        // Check cooldown from previous failures
        var lastFailedExam = await _context.LicenseExams
            .Where(le => le.PlayerWorldId == playerWorldId
                && le.LicenseTypeId == licenseType.Id
                && le.Status == ExamStatus.Failed)
            .OrderByDescending(le => le.CompletedAt)
            .FirstOrDefaultAsync(ct);

        if (lastFailedExam?.EligibleForRetakeAt > DateTimeOffset.UtcNow)
        {
            var remaining = lastFailedExam.EligibleForRetakeAt.Value - DateTimeOffset.UtcNow;
            return (false, $"On cooldown. Eligible for retake in {remaining.Hours}h {remaining.Minutes}m", null);
        }

        // Check if already has a scheduled/in-progress exam
        var existingExam = await _context.LicenseExams
            .Where(le => le.PlayerWorldId == playerWorldId
                && le.LicenseTypeId == licenseType.Id
                && (le.Status == ExamStatus.Scheduled || le.Status == ExamStatus.InProgress))
            .FirstOrDefaultAsync(ct);

        if (existingExam != null)
            return (false, "You already have a scheduled exam for this license", null);

        // Calculate exam cost with world multiplier
        var examCost = licenseType.BaseExamCost * playerWorld.World.LicenseCostMultiplier;

        // Calculate retake fee if applicable
        var attemptNumber = await _context.LicenseExams
            .CountAsync(le => le.PlayerWorldId == playerWorldId && le.LicenseTypeId == licenseType.Id, ct) + 1;

        if (attemptNumber > 1 && lastFailedExam != null)
        {
            // Apply retake fee multiplier
            var retakeMultiplier = attemptNumber switch
            {
                2 => lastFailedExam.Score >= 60 ? 0.5m : 0.75m,  // First retake
                3 => 1.0m,  // Second retake
                _ => 1.5m   // Third+ retake
            };
            examCost *= retakeMultiplier;
        }

        // Check if player can afford
        if (playerWorld.Balance < examCost)
            return (false, $"Insufficient funds. Required: ${examCost:N2}, Available: ${playerWorld.Balance:N2}", null);

        // Validate departure airport
        var airport = await _context.Airports
            .FirstOrDefaultAsync(a => a.Ident == departureIcao.ToUpper(), ct);

        if (airport == null)
            return (false, $"Airport '{departureIcao}' not found", null);

        // Deduct exam fee
        playerWorld.Balance -= examCost;

        // Generate exam route
        var route = await GenerateExamRouteAsync(licenseType, departureIcao, ct);

        // Create exam
        var exam = new LicenseExam
        {
            PlayerWorldId = playerWorldId,
            WorldId = playerWorld.WorldId,
            LicenseTypeId = licenseType.Id,
            Status = ExamStatus.Scheduled,
            ScheduledAt = DateTimeOffset.UtcNow,
            TimeLimitMinutes = licenseType.ExamDurationMinutes,
            RequiredAircraftCategory = licenseType.RequiredAircraftCategory,
            RequiredAircraftType = licenseType.RequiredAircraftType,
            DepartureIcao = departureIcao.ToUpper(),
            RouteJson = JsonSerializer.Serialize(route),
            AssignedAltitudeFt = GetAltitudeForCategory(licenseType.RequiredAircraftCategory),
            PassingScore = licenseType.PassingScore,
            AttemptNumber = attemptNumber,
            FeePaid = examCost
        };

        // Create maneuvers for the exam
        var maneuvers = GenerateExamManeuvers(licenseType);
        foreach (var maneuver in maneuvers)
        {
            maneuver.ExamId = exam.Id;
            exam.Maneuvers.Add(maneuver);
        }

        // Create checkpoints
        foreach (var checkpoint in route.Waypoints.Select((wp, i) => new ExamCheckpoint
        {
            ExamId = exam.Id,
            Order = i + 1,
            Name = wp.Name,
            Latitude = wp.Latitude,
            Longitude = wp.Longitude,
            RequiredAltitudeFt = exam.AssignedAltitudeFt,
            RadiusNm = wp.IsAirport ? 2.0 : 1.0,
            MaxPoints = 10
        }))
        {
            exam.Checkpoints.Add(checkpoint);
        }

        _context.LicenseExams.Add(exam);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Scheduled exam {ExamId} for player {PlayerWorldId}, license: {License}, cost: ${Cost}",
            exam.Id, playerWorldId, licenseCode, examCost);

        return (true, "Exam scheduled successfully", exam);
    }

    /// <summary>
    /// Starts an exam when player is ready to begin.
    /// </summary>
    public async Task<(bool Success, string Message, LicenseExam? Exam)> StartExamAsync(
        Guid examId,
        string aircraftUsed,
        CancellationToken ct = default)
    {
        var exam = await _context.LicenseExams
            .Include(le => le.LicenseType)
            .FirstOrDefaultAsync(le => le.Id == examId, ct);

        if (exam == null)
            return (false, "Exam not found", null);

        if (exam.Status != ExamStatus.Scheduled)
            return (false, $"Cannot start exam in {exam.Status} status", null);

        // Check if exam has expired (scheduled more than 24 hours ago)
        if (exam.ScheduledAt.AddHours(24) < DateTimeOffset.UtcNow)
        {
            exam.Status = ExamStatus.Expired;
            await _context.SaveChangesAsync(ct);
            return (false, "Exam has expired. Please schedule a new one.", null);
        }

        exam.Status = ExamStatus.InProgress;
        exam.StartedAt = DateTimeOffset.UtcNow;
        exam.AircraftUsed = aircraftUsed;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Started exam {ExamId}, aircraft: {Aircraft}", examId, aircraftUsed);

        return (true, "Exam started", exam);
    }

    /// <summary>
    /// Records a violation during an exam.
    /// </summary>
    public async Task<ExamViolation?> RecordViolationAsync(
        Guid examId,
        ViolationType type,
        float value,
        float threshold,
        double latitude,
        double longitude,
        int? altitude,
        CancellationToken ct = default)
    {
        var exam = await _context.LicenseExams.FindAsync(new object[] { examId }, ct);
        if (exam == null || exam.Status != ExamStatus.InProgress)
            return null;

        var pointsDeducted = CalculateViolationPoints(type, value);
        var causedFailure = IsCriticalViolation(type, value);

        var violation = new ExamViolation
        {
            ExamId = examId,
            OccurredAt = DateTimeOffset.UtcNow,
            Type = type,
            Value = value,
            Threshold = threshold,
            PointsDeducted = pointsDeducted,
            CausedFailure = causedFailure,
            LatitudeAtViolation = latitude,
            LongitudeAtViolation = longitude,
            AltitudeAtViolation = altitude,
            Description = GenerateViolationDescription(type, value, threshold)
        };

        _context.ExamViolations.Add(violation);

        if (causedFailure)
        {
            await FailExamAsync(examId, $"Critical violation: {type}", ct);
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Recorded violation for exam {ExamId}: {Type} ({Value}), points: -{Points}",
            examId, type, value, pointsDeducted);

        return violation;
    }

    /// <summary>
    /// Records a landing during an exam.
    /// </summary>
    public async Task<ExamLanding?> RecordLandingAsync(
        Guid examId,
        ExamLandingData data,
        CancellationToken ct = default)
    {
        var exam = await _context.LicenseExams
            .Include(le => le.Landings)
            .FirstOrDefaultAsync(le => le.Id == examId, ct);

        if (exam == null || exam.Status != ExamStatus.InProgress)
            return null;

        var order = exam.Landings.Count + 1;
        var points = CalculateLandingScore(data);

        var landing = new ExamLanding
        {
            ExamId = examId,
            Order = order,
            AirportIcao = data.AirportIcao,
            Type = data.Type,
            VerticalSpeedFpm = data.VerticalSpeedFpm,
            CenterlineDeviationFt = data.CenterlineDeviationFt,
            TouchdownZoneDistanceFt = data.TouchdownZoneDistanceFt,
            GroundSpeedKts = data.GroundSpeedKts,
            PitchDeg = data.PitchDeg,
            BankDeg = data.BankDeg,
            GearDown = data.GearDown,
            RunwayUsed = data.RunwayUsed,
            PointsAwarded = points,
            MaxPoints = 25,
            LandedAt = DateTimeOffset.UtcNow,
            Notes = GenerateLandingNotes(data, points)
        };

        _context.ExamLandings.Add(landing);

        // Check for gear up landing (critical failure)
        if (!data.GearDown)
        {
            await RecordViolationAsync(examId, ViolationType.GearUpLanding, 0, 0,
                data.Latitude, data.Longitude, (int?)data.AltitudeAtTouchdown, ct);
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Recorded landing {Order} for exam {ExamId} at {Airport}: {Points}/{MaxPoints}",
            order, examId, data.AirportIcao, points, 25);

        return landing;
    }

    /// <summary>
    /// Marks a checkpoint as reached during an exam.
    /// </summary>
    public async Task<bool> ReachCheckpointAsync(
        Guid examId,
        int checkpointOrder,
        int altitude,
        int speed,
        CancellationToken ct = default)
    {
        var checkpoint = await _context.ExamCheckpoints
            .FirstOrDefaultAsync(ec => ec.ExamId == examId && ec.Order == checkpointOrder, ct);

        if (checkpoint == null || checkpoint.WasReached)
            return false;

        checkpoint.WasReached = true;
        checkpoint.ReachedAt = DateTimeOffset.UtcNow;
        checkpoint.AltitudeAtReach = altitude;
        checkpoint.SpeedAtReachKts = speed;

        // Calculate points based on altitude adherence
        var altitudeDeviation = checkpoint.RequiredAltitudeFt.HasValue
            ? Math.Abs(altitude - checkpoint.RequiredAltitudeFt.Value)
            : 0;

        checkpoint.PointsAwarded = altitudeDeviation switch
        {
            <= 100 => checkpoint.MaxPoints,
            <= 200 => (int)(checkpoint.MaxPoints * 0.8),
            <= 300 => (int)(checkpoint.MaxPoints * 0.6),
            _ => (int)(checkpoint.MaxPoints * 0.4)
        };

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Checkpoint {Order} reached for exam {ExamId}, points: {Points}/{Max}",
            checkpointOrder, examId, checkpoint.PointsAwarded, checkpoint.MaxPoints);

        return true;
    }

    /// <summary>
    /// Completes an exam and calculates the final score.
    /// </summary>
    public async Task<(bool Success, ExamResult? Result)> CompleteExamAsync(
        Guid examId,
        CancellationToken ct = default)
    {
        var exam = await _context.LicenseExams
            .Include(le => le.LicenseType)
            .Include(le => le.Maneuvers)
            .Include(le => le.Checkpoints)
            .Include(le => le.Landings)
            .Include(le => le.Violations)
            .FirstOrDefaultAsync(le => le.Id == examId, ct);

        if (exam == null)
            return (false, null);

        if (exam.Status != ExamStatus.InProgress)
            return (false, null);

        // Calculate final score
        var score = CalculateFinalScore(exam);
        var passed = score >= exam.PassingScore;

        exam.Score = score;
        exam.Status = passed ? ExamStatus.Passed : ExamStatus.Failed;
        exam.CompletedAt = DateTimeOffset.UtcNow;
        exam.FlightTimeMinutes = exam.StartedAt.HasValue
            ? (int)(DateTimeOffset.UtcNow - exam.StartedAt.Value).TotalMinutes
            : null;
        exam.ExaminerNotes = GenerateExaminerNotes(exam, score, passed);

        // If failed, set cooldown
        if (!passed)
        {
            var cooldownHours = score >= 60 ? 6 : 12;
            if (exam.AttemptNumber >= 3) cooldownHours *= 2;
            exam.EligibleForRetakeAt = DateTimeOffset.UtcNow.AddHours(cooldownHours);
            exam.FailureReason = score < exam.PassingScore
                ? $"Score {score}/100 below passing threshold of {exam.PassingScore}"
                : "Critical violation";
        }

        await _context.SaveChangesAsync(ct);

        // Grant license if passed
        UserLicense? license = null;
        if (passed)
        {
            license = await _licenseService.GrantLicenseAsync(
                exam.PlayerWorldId, exam.LicenseTypeId, exam, ct);
        }

        _logger.LogInformation("Completed exam {ExamId}: {Status} with score {Score}/100",
            examId, exam.Status, score);

        return (true, new ExamResult
        {
            Exam = exam,
            Score = score,
            Passed = passed,
            License = license
        });
    }

    /// <summary>
    /// Fails an exam immediately due to a critical violation.
    /// </summary>
    public async Task<bool> FailExamAsync(Guid examId, string reason, CancellationToken ct = default)
    {
        var exam = await _context.LicenseExams.FindAsync(new object[] { examId }, ct);
        if (exam == null || exam.Status != ExamStatus.InProgress)
            return false;

        exam.Status = ExamStatus.Failed;
        exam.Score = 0;
        exam.FailureReason = reason;
        exam.CompletedAt = DateTimeOffset.UtcNow;
        exam.EligibleForRetakeAt = DateTimeOffset.UtcNow.AddHours(12);

        await _context.SaveChangesAsync(ct);

        _logger.LogWarning("Exam {ExamId} failed: {Reason}", examId, reason);

        return true;
    }

    /// <summary>
    /// Abandons an exam.
    /// </summary>
    public async Task<bool> AbandonExamAsync(Guid examId, CancellationToken ct = default)
    {
        var exam = await _context.LicenseExams.FindAsync(new object[] { examId }, ct);
        if (exam == null)
            return false;

        if (exam.Status == ExamStatus.Passed || exam.Status == ExamStatus.Failed)
            return false;

        exam.Status = ExamStatus.Abandoned;
        exam.CompletedAt = DateTimeOffset.UtcNow;
        exam.FailureReason = "Abandoned by player";

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Exam {ExamId} abandoned", examId);

        return true;
    }

    /// <summary>
    /// Gets exam details including all tracking data.
    /// </summary>
    public async Task<LicenseExam?> GetExamAsync(Guid examId, CancellationToken ct = default)
    {
        return await _context.LicenseExams
            .Include(le => le.LicenseType)
            .Include(le => le.Maneuvers.OrderBy(m => m.Order))
            .Include(le => le.Checkpoints.OrderBy(c => c.Order))
            .Include(le => le.Landings.OrderBy(l => l.Order))
            .Include(le => le.Violations.OrderBy(v => v.OccurredAt))
            .FirstOrDefaultAsync(le => le.Id == examId, ct);
    }

    /// <summary>
    /// Gets player's exam history.
    /// </summary>
    public async Task<List<LicenseExam>> GetPlayerExamsAsync(
        Guid playerWorldId,
        int limit = 20,
        CancellationToken ct = default)
    {
        return await _context.LicenseExams
            .Include(le => le.LicenseType)
            .Where(le => le.PlayerWorldId == playerWorldId)
            .OrderByDescending(le => le.ScheduledAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    #region Private Methods

    private async Task<ExamRoute> GenerateExamRouteAsync(
        LicenseType licenseType,
        string departureIcao,
        CancellationToken ct)
    {
        // Get departure airport
        var departure = await _context.Airports
            .FirstOrDefaultAsync(a => a.Ident == departureIcao, ct);

        if (departure == null)
            throw new InvalidOperationException($"Airport {departureIcao} not found");

        // Determine search parameters based on license type
        var (minDistance, maxDistance, airportCount) = licenseType.Code switch
        {
            "DISCOVERY" => (5, 15, 1),
            "PPL" => (10, 30, 2),
            "CPL" => (15, 40, 3),
            "NIGHT" => (10, 25, 2),
            "IR" => (20, 50, 3),
            "MEP" => (15, 35, 2),
            "ATPL" => (30, 80, 4),
            _ when licenseType.Category == LicenseCategory.TypeRating => (15, 50, 3),
            _ => (10, 30, 2)
        };

        // Find nearby airports
        var latRange = maxDistance / 60.0; // approximate degrees
        var lonRange = maxDistance / (60.0 * Math.Cos(departure.Latitude * Math.PI / 180));

        var nearbyAirports = await _context.Airports
            .Where(a => a.Id != departure.Id
                && a.Latitude >= departure.Latitude - latRange
                && a.Latitude <= departure.Latitude + latRange
                && a.Longitude >= departure.Longitude - lonRange
                && a.Longitude <= departure.Longitude + lonRange
                && (a.Type == "large_airport" || a.Type == "medium_airport" || a.Type == "small_airport"))
            .OrderBy(a => Math.Pow(a.Latitude - departure.Latitude, 2) + Math.Pow(a.Longitude - departure.Longitude, 2))
            .Take(airportCount * 2) // Get more than needed for selection
            .ToListAsync(ct);

        // Select diverse airports
        var waypoints = new List<ExamWaypoint>
        {
            new() { Name = departure.Ident, Latitude = departure.Latitude, Longitude = departure.Longitude, IsAirport = true }
        };

        foreach (var airport in nearbyAirports.Take(airportCount))
        {
            waypoints.Add(new ExamWaypoint
            {
                Name = airport.Ident,
                Latitude = airport.Latitude,
                Longitude = airport.Longitude,
                IsAirport = true
            });
        }

        // Return to departure
        waypoints.Add(new ExamWaypoint
        {
            Name = departure.Ident,
            Latitude = departure.Latitude,
            Longitude = departure.Longitude,
            IsAirport = true
        });

        return new ExamRoute
        {
            DepartureIcao = departureIcao,
            Waypoints = waypoints
        };
    }

    private static int? GetAltitudeForCategory(AircraftCategory? category)
    {
        return category switch
        {
            AircraftCategory.SEP => 3000,
            AircraftCategory.MEP => 4000,
            AircraftCategory.Turboprop => 5000,
            AircraftCategory.RegionalJet => 6000,
            AircraftCategory.NarrowBody => 8000,
            AircraftCategory.WideBody => 10000,
            _ => 3000
        };
    }

    private static List<ExamManeuver> GenerateExamManeuvers(LicenseType licenseType)
    {
        var maneuvers = new List<ExamManeuver>
        {
            new() { ManeuverType = "Takeoff", Order = 1, IsRequired = true, MaxPoints = 15, AltitudeToleranceFt = 200 },
            new() { ManeuverType = "Climb", Order = 2, IsRequired = true, MaxPoints = 10, AltitudeToleranceFt = 200 },
            new() { ManeuverType = "Cruise", Order = 3, IsRequired = true, MaxPoints = 20, AltitudeToleranceFt = 200, HeadingToleranceDeg = 10 }
        };

        // Add specific maneuvers based on license type
        if (licenseType.Code is "PPL" or "CPL" or "ATPL")
        {
            maneuvers.Add(new ExamManeuver
            {
                ManeuverType = "SteepTurn",
                Order = 4,
                IsRequired = licenseType.Code != "PPL",
                MaxPoints = 15,
                AltitudeToleranceFt = 100,
                HeadingToleranceDeg = 5
            });
        }

        maneuvers.Add(new ExamManeuver
        {
            ManeuverType = "Approach",
            Order = maneuvers.Count + 1,
            IsRequired = true,
            MaxPoints = 10,
            AltitudeToleranceFt = 100
        });

        maneuvers.Add(new ExamManeuver
        {
            ManeuverType = "Landing",
            Order = maneuvers.Count + 1,
            IsRequired = true,
            MaxPoints = 30
        });

        return maneuvers;
    }

    private static int CalculateViolationPoints(ViolationType type, float value)
    {
        return type switch
        {
            ViolationType.SpeedExcess => 5,
            ViolationType.AltitudeDeviation => 3,
            ViolationType.HeadingDeviation => 2,
            ViolationType.GForceExcess => value > 3.0f ? 10 : 5,
            ViolationType.HardLanding => value > -900 ? 10 : 5,
            ViolationType.CenterlineDeviation => 3,
            ViolationType.MissedCheckpoint => 10,
            ViolationType.TimeExceeded => 1,
            ViolationType.Crash => 100,
            ViolationType.GearUpLanding => 100,
            ViolationType.Stall => 5,
            ViolationType.Spin => 15,
            _ => 5
        };
    }

    private static bool IsCriticalViolation(ViolationType type, float value)
    {
        return type switch
        {
            ViolationType.GForceExcess => value > 3.0f,
            ViolationType.Crash => true,
            ViolationType.GearUpLanding => true,
            _ => false
        };
    }

    private static string GenerateViolationDescription(ViolationType type, float value, float threshold)
    {
        return type switch
        {
            ViolationType.SpeedExcess => $"Speed exceeded {threshold} kts (actual: {value} kts)",
            ViolationType.AltitudeDeviation => $"Altitude deviated {Math.Abs(value - threshold):F0} ft from assigned",
            ViolationType.HeadingDeviation => $"Heading deviated {Math.Abs(value - threshold):F0}° from assigned",
            ViolationType.GForceExcess => $"G-force exceeded {threshold}G (actual: {value:F1}G)",
            ViolationType.HardLanding => $"Hard landing: {value:F0} fpm",
            ViolationType.CenterlineDeviation => $"Centerline deviation: {value:F0} ft",
            ViolationType.MissedCheckpoint => "Missed required checkpoint",
            ViolationType.TimeExceeded => $"Time limit exceeded by {value:F0} minutes",
            ViolationType.Crash => "Aircraft crashed",
            ViolationType.GearUpLanding => "Landed with gear up",
            ViolationType.Stall => "Stall warning activated",
            ViolationType.Spin => "Entered spin",
            _ => $"{type}: {value}"
        };
    }

    private static int CalculateLandingScore(ExamLandingData data)
    {
        var score = 20; // Base points

        // Vertical speed scoring
        score += data.VerticalSpeedFpm switch
        {
            > -100 => 5,   // Greaser
            > -200 => 3,   // Smooth
            > -400 => 0,   // Normal
            > -600 => -5,  // Firm
            > -900 => -10, // Hard
            _ => -20       // Very hard
        };

        // Centerline scoring
        score += Math.Abs(data.CenterlineDeviationFt) switch
        {
            < 10 => 3,
            < 25 => 1,
            < 50 => 0,
            _ => -3
        };

        // Touchdown zone
        score += data.TouchdownZoneDistanceFt switch
        {
            < 500 => 2,
            < 1000 => 0,
            _ => -2
        };

        return Math.Max(0, Math.Min(25, score));
    }

    private static string GenerateLandingNotes(ExamLandingData data, int score)
    {
        var notes = new List<string>();

        if (data.VerticalSpeedFpm > -100)
            notes.Add("Excellent smooth touchdown");
        else if (data.VerticalSpeedFpm < -600)
            notes.Add($"Hard landing ({data.VerticalSpeedFpm:F0} fpm)");

        if (Math.Abs(data.CenterlineDeviationFt) < 10)
            notes.Add("Perfect centerline");
        else if (Math.Abs(data.CenterlineDeviationFt) > 50)
            notes.Add($"Off centerline by {data.CenterlineDeviationFt:F0} ft");

        if (!data.GearDown)
            notes.Add("GEAR UP LANDING");

        return string.Join(". ", notes);
    }

    private static int CalculateFinalScore(LicenseExam exam)
    {
        var totalPoints = 0;
        var maxPoints = 0;

        // Maneuver points
        foreach (var maneuver in exam.Maneuvers)
        {
            totalPoints += maneuver.PointsAwarded;
            maxPoints += maneuver.MaxPoints;
        }

        // Checkpoint points
        foreach (var checkpoint in exam.Checkpoints)
        {
            totalPoints += checkpoint.PointsAwarded;
            maxPoints += checkpoint.MaxPoints;
        }

        // Landing points
        foreach (var landing in exam.Landings)
        {
            totalPoints += landing.PointsAwarded;
            maxPoints += landing.MaxPoints;
        }

        // Violation deductions
        var deductions = exam.Violations.Sum(v => v.PointsDeducted);
        totalPoints -= deductions;

        // Calculate percentage score
        if (maxPoints == 0)
            return 0;

        var score = (int)Math.Round((double)totalPoints / maxPoints * 100);
        return Math.Max(0, Math.Min(100, score));
    }

    private static string GenerateExaminerNotes(LicenseExam exam, int score, bool passed)
    {
        var notes = new List<string>();

        if (passed)
        {
            notes.Add(score switch
            {
                >= 90 => "Excellent performance. Well above standards.",
                >= 80 => "Good performance. Meets all requirements.",
                _ => "Satisfactory performance. Requirements met."
            });
        }
        else
        {
            notes.Add("Performance below acceptable standards.");
        }

        // Landing feedback
        var landings = exam.Landings.ToList();
        if (landings.Any())
        {
            var avgVerticalSpeed = landings.Average(l => l.VerticalSpeedFpm);
            if (avgVerticalSpeed < -600)
                notes.Add("Practice flare timing to improve landing quality.");
        }

        // Violation feedback
        var violationTypes = exam.Violations.Select(v => v.Type).Distinct().ToList();
        if (violationTypes.Contains(ViolationType.AltitudeDeviation))
            notes.Add("Work on altitude discipline during cruise.");
        if (violationTypes.Contains(ViolationType.HeadingDeviation))
            notes.Add("Improve heading awareness and correction.");
        if (violationTypes.Contains(ViolationType.GForceExcess))
            notes.Add("Avoid abrupt control inputs.");

        return string.Join(" ", notes);
    }

    #endregion
}

#region DTOs

public class ExamRoute
{
    public string DepartureIcao { get; set; } = string.Empty;
    public List<ExamWaypoint> Waypoints { get; set; } = new();
}

public class ExamWaypoint
{
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsAirport { get; set; }
}

public class ExamLandingData
{
    public string AirportIcao { get; set; } = string.Empty;
    public LandingType Type { get; set; }
    public float VerticalSpeedFpm { get; set; }
    public float CenterlineDeviationFt { get; set; }
    public float TouchdownZoneDistanceFt { get; set; }
    public float? GroundSpeedKts { get; set; }
    public float? PitchDeg { get; set; }
    public float? BankDeg { get; set; }
    public bool GearDown { get; set; } = true;
    public string? RunwayUsed { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public float? AltitudeAtTouchdown { get; set; }
}

public class ExamResult
{
    public LicenseExam Exam { get; set; } = null!;
    public int Score { get; set; }
    public bool Passed { get; set; }
    public UserLicense? License { get; set; }
}

#endregion
