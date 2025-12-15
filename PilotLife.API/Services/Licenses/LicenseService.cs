using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services.Licenses;

public class LicenseService
{
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<LicenseService> _logger;

    public LicenseService(PilotLifeDbContext context, ILogger<LicenseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all license types available in the system.
    /// </summary>
    public async Task<List<LicenseType>> GetLicenseTypesAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.LicenseTypes.AsQueryable();

        if (!includeInactive)
            query = query.Where(lt => lt.IsActive);

        return await query
            .OrderBy(lt => lt.DisplayOrder)
            .ThenBy(lt => lt.Name)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets a license type by its code.
    /// </summary>
    public async Task<LicenseType?> GetLicenseTypeByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.LicenseTypes
            .FirstOrDefaultAsync(lt => lt.Code == code, ct);
    }

    /// <summary>
    /// Gets all licenses held by a player world.
    /// </summary>
    public async Task<List<UserLicense>> GetPlayerLicensesAsync(Guid playerWorldId, CancellationToken ct = default)
    {
        return await _context.UserLicenses
            .Include(ul => ul.LicenseType)
            .Where(ul => ul.PlayerWorldId == playerWorldId)
            .OrderBy(ul => ul.LicenseType.DisplayOrder)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets valid (non-expired, non-revoked) licenses for a player.
    /// </summary>
    public async Task<List<UserLicense>> GetValidPlayerLicensesAsync(Guid playerWorldId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _context.UserLicenses
            .Include(ul => ul.LicenseType)
            .Where(ul => ul.PlayerWorldId == playerWorldId
                && ul.IsValid
                && !ul.IsRevoked
                && (ul.ExpiresAt == null || ul.ExpiresAt > now))
            .OrderBy(ul => ul.LicenseType.DisplayOrder)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Checks if a player has a valid license of a specific type.
    /// </summary>
    public async Task<bool> HasValidLicenseAsync(Guid playerWorldId, string licenseCode, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _context.UserLicenses
            .Include(ul => ul.LicenseType)
            .AnyAsync(ul => ul.PlayerWorldId == playerWorldId
                && ul.LicenseType.Code == licenseCode
                && ul.IsValid
                && !ul.IsRevoked
                && (ul.ExpiresAt == null || ul.ExpiresAt > now), ct);
    }

    /// <summary>
    /// Checks if a player has all prerequisite licenses for a given license type.
    /// </summary>
    public async Task<(bool HasAll, List<string> Missing)> CheckPrerequisitesAsync(
        Guid playerWorldId,
        LicenseType licenseType,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(licenseType.PrerequisiteLicensesJson))
            return (true, new List<string>());

        var prerequisites = JsonSerializer.Deserialize<List<string>>(licenseType.PrerequisiteLicensesJson)
            ?? new List<string>();

        if (prerequisites.Count == 0)
            return (true, new List<string>());

        var validLicenses = await GetValidPlayerLicensesAsync(playerWorldId, ct);
        var validCodes = validLicenses.Select(ul => ul.LicenseType.Code).ToHashSet();

        var missing = prerequisites.Where(p => !validCodes.Contains(p)).ToList();
        return (missing.Count == 0, missing);
    }

    /// <summary>
    /// Grants a license to a player after passing an exam.
    /// </summary>
    public async Task<UserLicense> GrantLicenseAsync(
        Guid playerWorldId,
        Guid licenseTypeId,
        LicenseExam passedExam,
        CancellationToken ct = default)
    {
        var licenseType = await _context.LicenseTypes.FindAsync(new object[] { licenseTypeId }, ct)
            ?? throw new InvalidOperationException($"License type {licenseTypeId} not found");

        var playerWorld = await _context.PlayerWorlds
            .Include(pw => pw.World)
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId, ct)
            ?? throw new InvalidOperationException($"Player world {playerWorldId} not found");

        // Check if player already has this license
        var existingLicense = await _context.UserLicenses
            .FirstOrDefaultAsync(ul => ul.PlayerWorldId == playerWorldId && ul.LicenseTypeId == licenseTypeId, ct);

        if (existingLicense != null)
        {
            // Renew existing license
            existingLicense.LastRenewedAt = DateTimeOffset.UtcNow;
            existingLicense.RenewalCount++;
            existingLicense.IsValid = true;
            existingLicense.IsRevoked = false;
            existingLicense.RevocationReason = null;
            existingLicense.RevokedAt = null;

            if (licenseType.ValidityGameDays.HasValue)
            {
                existingLicense.ExpiresAt = DateTimeOffset.UtcNow.AddDays(licenseType.ValidityGameDays.Value);
            }

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Renewed license {Code} for player {PlayerWorldId}",
                licenseType.Code, playerWorldId);

            return existingLicense;
        }

        // Calculate expiry date based on world time multiplier
        DateTimeOffset? expiresAt = null;
        if (licenseType.ValidityGameDays.HasValue)
        {
            // Convert game days to real time based on world settings
            // Default: 1 game day = 6 real hours (4x multiplier)
            var realDays = licenseType.ValidityGameDays.Value / 4.0;
            expiresAt = DateTimeOffset.UtcNow.AddDays(realDays);
        }

        // Count previous exam attempts for this license
        var attemptCount = await _context.LicenseExams
            .CountAsync(le => le.PlayerWorldId == playerWorldId && le.LicenseTypeId == licenseTypeId, ct);

        var userLicense = new UserLicense
        {
            PlayerWorldId = playerWorldId,
            LicenseTypeId = licenseTypeId,
            EarnedAt = DateTimeOffset.UtcNow,
            ExpiresAt = expiresAt,
            IsValid = true,
            ExamScore = passedExam.Score,
            ExamAttempts = attemptCount,
            TotalPaid = passedExam.FeePaid,
            PassedExamId = passedExam.Id
        };

        _context.UserLicenses.Add(userLicense);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Granted license {Code} to player {PlayerWorldId} with score {Score}",
            licenseType.Code, playerWorldId, passedExam.Score);

        return userLicense;
    }

    /// <summary>
    /// Renews a license for a player.
    /// </summary>
    public async Task<(bool Success, string Message, decimal? Cost)> RenewLicenseAsync(
        Guid playerWorldId,
        Guid licenseId,
        CancellationToken ct = default)
    {
        var license = await _context.UserLicenses
            .Include(ul => ul.LicenseType)
            .Include(ul => ul.PlayerWorld)
                .ThenInclude(pw => pw.World)
            .FirstOrDefaultAsync(ul => ul.Id == licenseId && ul.PlayerWorldId == playerWorldId, ct);

        if (license == null)
            return (false, "License not found", null);

        if (license.IsRevoked)
            return (false, "Cannot renew a revoked license", null);

        if (!license.LicenseType.BaseRenewalCost.HasValue)
            return (false, "This license cannot be renewed (permanent)", null);

        // Calculate renewal cost with world multiplier
        var renewalCost = license.LicenseType.BaseRenewalCost.Value
            * license.PlayerWorld.World.LicenseCostMultiplier;

        // Check if player has enough balance
        if (license.PlayerWorld.Balance < renewalCost)
            return (false, $"Insufficient funds. Required: ${renewalCost:N2}", renewalCost);

        // Deduct cost
        license.PlayerWorld.Balance -= renewalCost;

        // Update license
        license.LastRenewedAt = DateTimeOffset.UtcNow;
        license.RenewalCount++;
        license.IsValid = true;
        license.TotalPaid += renewalCost;

        if (license.LicenseType.ValidityGameDays.HasValue)
        {
            var realDays = license.LicenseType.ValidityGameDays.Value / 4.0;
            license.ExpiresAt = DateTimeOffset.UtcNow.AddDays(realDays);
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Renewed license {LicenseId} for player {PlayerWorldId}, cost: ${Cost}",
            licenseId, playerWorldId, renewalCost);

        return (true, "License renewed successfully", renewalCost);
    }

    /// <summary>
    /// Revokes a license from a player.
    /// </summary>
    public async Task<bool> RevokeLicenseAsync(
        Guid licenseId,
        string reason,
        CancellationToken ct = default)
    {
        var license = await _context.UserLicenses.FindAsync(new object[] { licenseId }, ct);
        if (license == null)
            return false;

        license.IsValid = false;
        license.IsRevoked = true;
        license.RevocationReason = reason;
        license.RevokedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogWarning("Revoked license {LicenseId} for reason: {Reason}", licenseId, reason);

        return true;
    }

    /// <summary>
    /// Expires licenses that have passed their expiry date.
    /// Should be called periodically by a background service.
    /// </summary>
    public async Task<int> ExpireOverdueLicensesAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expiredLicenses = await _context.UserLicenses
            .Where(ul => ul.IsValid
                && !ul.IsRevoked
                && ul.ExpiresAt != null
                && ul.ExpiresAt < now)
            .ToListAsync(ct);

        foreach (var license in expiredLicenses)
        {
            license.IsValid = false;
        }

        if (expiredLicenses.Count > 0)
        {
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Expired {Count} overdue licenses", expiredLicenses.Count);
        }

        return expiredLicenses.Count;
    }

    /// <summary>
    /// Gets license shop data for a player showing all available licenses
    /// and their qualification status.
    /// </summary>
    public async Task<List<LicenseShopItem>> GetLicenseShopAsync(
        Guid playerWorldId,
        CancellationToken ct = default)
    {
        var licenseTypes = await GetLicenseTypesAsync(false, ct);
        var playerLicenses = await GetValidPlayerLicensesAsync(playerWorldId, ct);
        var playerLicensesCodes = playerLicenses.Select(ul => ul.LicenseType.Code).ToHashSet();

        var playerWorld = await _context.PlayerWorlds
            .Include(pw => pw.World)
            .FirstOrDefaultAsync(pw => pw.Id == playerWorldId, ct);

        if (playerWorld == null)
            return new List<LicenseShopItem>();

        var result = new List<LicenseShopItem>();

        foreach (var licenseType in licenseTypes)
        {
            var hasLicense = playerLicensesCodes.Contains(licenseType.Code);
            var (hasPrerequisites, missingPrerequisites) = await CheckPrerequisitesAsync(playerWorldId, licenseType, ct);

            // Check for cooldown from failed exams
            var lastFailedExam = await _context.LicenseExams
                .Where(le => le.PlayerWorldId == playerWorldId
                    && le.LicenseTypeId == licenseType.Id
                    && le.Status == ExamStatus.Failed)
                .OrderByDescending(le => le.CompletedAt)
                .FirstOrDefaultAsync(ct);

            var isOnCooldown = lastFailedExam?.EligibleForRetakeAt > DateTimeOffset.UtcNow;
            var cooldownEnds = lastFailedExam?.EligibleForRetakeAt;

            // Calculate costs with world multiplier
            var examCost = licenseType.BaseExamCost * playerWorld.World.LicenseCostMultiplier;
            var renewalCost = licenseType.BaseRenewalCost.HasValue
                ? licenseType.BaseRenewalCost.Value * playerWorld.World.LicenseCostMultiplier
                : (decimal?)null;

            // Find existing license if any
            var existingLicense = playerLicenses.FirstOrDefault(ul => ul.LicenseType.Code == licenseType.Code);

            result.Add(new LicenseShopItem
            {
                LicenseType = licenseType,
                IsOwned = hasLicense,
                ExpiresAt = existingLicense?.ExpiresAt,
                CanTakeExam = !hasLicense && hasPrerequisites && !isOnCooldown,
                HasPrerequisites = hasPrerequisites,
                MissingPrerequisites = missingPrerequisites,
                IsOnCooldown = isOnCooldown,
                CooldownEndsAt = cooldownEnds,
                ExamCost = examCost,
                RenewalCost = renewalCost,
                CanRenew = hasLicense && existingLicense?.ExpiresAt != null
            });
        }

        return result;
    }
}

/// <summary>
/// Data transfer object for license shop display.
/// </summary>
public class LicenseShopItem
{
    public LicenseType LicenseType { get; set; } = null!;
    public bool IsOwned { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool CanTakeExam { get; set; }
    public bool HasPrerequisites { get; set; }
    public List<string> MissingPrerequisites { get; set; } = new();
    public bool IsOnCooldown { get; set; }
    public DateTimeOffset? CooldownEndsAt { get; set; }
    public decimal ExamCost { get; set; }
    public decimal? RenewalCost { get; set; }
    public bool CanRenew { get; set; }
}
