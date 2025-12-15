using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.API.Services;

/// <summary>
/// Seeds the database with initial data including default worlds and system roles.
/// </summary>
public class DatabaseSeeder
{
    private readonly PilotLifeDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(PilotLifeDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds all initial data if not already present.
    /// </summary>
    public async Task SeedAsync()
    {
        await SeedWorldsAsync();
        await SeedRolesAsync();
        await SeedAdminUserAsync();
        await SeedCargoTypesAsync();
        await SeedLicenseTypesAsync();
    }

    /// <summary>
    /// Seeds the default worlds if they don't exist.
    /// </summary>
    private async Task SeedWorldsAsync()
    {
        if (await _context.Worlds.AnyAsync())
        {
            _logger.LogInformation("Worlds already seeded, skipping.");
            return;
        }

        _logger.LogInformation("Seeding default worlds...");

        var worlds = new List<World>
        {
            CreateWorldWithSettings(World.CreateEasy()),
            CreateWorldWithSettings(World.CreateMedium()),
            CreateWorldWithSettings(World.CreateHard())
        };

        _context.Worlds.AddRange(worlds);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} default worlds.", worlds.Count);
    }

    /// <summary>
    /// Creates a world with associated default settings.
    /// </summary>
    private static World CreateWorldWithSettings(World world)
    {
        world.Settings = new WorldSettings
        {
            WorldId = world.Id,
            AllowNewPlayers = true,
            AllowIllegalCargo = true,
            EnableAuctions = true,
            EnableAICrews = true,
            EnableAircraftRental = true,
            MaxAircraftPerPlayer = 0, // unlimited
            MaxLoansPerPlayer = 3,
            MaxWorkersPerPlayer = 10,
            MaxActiveJobsPerPlayer = 5,
            RequireApprovalToJoin = false,
            EnableChat = true,
            EnableReporting = true
        };
        return world;
    }

    /// <summary>
    /// Seeds the system roles if they don't exist.
    /// </summary>
    private async Task SeedRolesAsync()
    {
        if (await _context.Roles.AnyAsync())
        {
            _logger.LogInformation("Roles already seeded, skipping.");
            return;
        }

        _logger.LogInformation("Seeding system roles...");

        var roles = new List<Role>
        {
            CreateSuperAdminRole(),
            CreateAdminRole(),
            CreateModeratorRole(),
            CreatePlayerRole()
        };

        _context.Roles.AddRange(roles);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} system roles.", roles.Count);
    }

    /// <summary>
    /// Seeds the default admin user if it doesn't exist.
    /// </summary>
    private async Task SeedAdminUserAsync()
    {
        var adminEmail = "admin@pilotlife.com";

        if (await _context.Users.AnyAsync(u => u.Email == adminEmail))
        {
            _logger.LogInformation("Admin user already exists, skipping.");
            return;
        }

        _logger.LogInformation("Seeding admin user...");

        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (superAdminRole == null)
        {
            _logger.LogWarning("SuperAdmin role not found, cannot create admin user.");
            return;
        }

        var adminUser = new User
        {
            FirstName = "Admin",
            LastName = "User",
            Email = adminEmail,
            PasswordHash = HashPassword("123456"),
            EmailVerified = true,
            Balance = 1000000m
        };

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        var userRole = new UserRole
        {
            UserId = adminUser.Id,
            RoleId = superAdminRole.Id,
            WorldId = null, // Global
            GrantedAt = DateTimeOffset.UtcNow,
            GrantedByUserId = adminUser.Id // Self-granted for system user
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded admin user with SuperAdmin role.");
    }

    /// <summary>
    /// Hashes a password using SHA256.
    /// </summary>
    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Creates the SuperAdmin role with all permissions.
    /// </summary>
    private static Role CreateSuperAdminRole()
    {
        var role = new Role
        {
            Name = "SuperAdmin",
            Description = "Full system access - all permissions in all worlds.",
            Priority = 1000,
            IsSystemRole = true,
            IsGlobal = true
        };

        // SuperAdmin gets all permissions
        foreach (var permission in Enum.GetValues<PermissionCategory>())
        {
            role.Permissions.Add(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission,
                IsGranted = true
            });
        }

        return role;
    }

    /// <summary>
    /// Creates the Admin role with administrative permissions.
    /// </summary>
    private static Role CreateAdminRole()
    {
        var role = new Role
        {
            Name = "Admin",
            Description = "World administrator - can manage users, economy, and moderation.",
            Priority = 100,
            IsSystemRole = true,
            IsGlobal = false // Can be assigned per-world
        };

        // Admin permissions (excludes system-level permissions)
        var adminPermissions = new[]
        {
            PermissionCategory.Users_View,
            PermissionCategory.Users_Edit,
            PermissionCategory.Users_Ban,
            PermissionCategory.Worlds_View,
            PermissionCategory.Worlds_Edit,
            PermissionCategory.Worlds_Settings,
            PermissionCategory.Economy_View,
            PermissionCategory.Economy_Adjust,
            PermissionCategory.Economy_Audit,
            PermissionCategory.Jobs_View,
            PermissionCategory.Jobs_Create,
            PermissionCategory.Jobs_Edit,
            PermissionCategory.Jobs_Delete,
            PermissionCategory.Jobs_ForceComplete,
            PermissionCategory.Aircraft_View,
            PermissionCategory.Aircraft_Spawn,
            PermissionCategory.Marketplace_Manage,
            PermissionCategory.Auctions_Moderate,
            PermissionCategory.Licenses_View,
            PermissionCategory.Licenses_Grant,
            PermissionCategory.Licenses_Revoke,
            PermissionCategory.Exams_Override,
            PermissionCategory.Chat_Moderate,
            PermissionCategory.Reports_View,
            PermissionCategory.Reports_Resolve,
            PermissionCategory.Violations_Issue,
            PermissionCategory.Violations_Clear,
            PermissionCategory.Logs_View,
            PermissionCategory.Analytics_View
        };

        foreach (var permission in adminPermissions)
        {
            role.Permissions.Add(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission,
                IsGranted = true
            });
        }

        return role;
    }

    /// <summary>
    /// Creates the Moderator role with moderation permissions.
    /// </summary>
    private static Role CreateModeratorRole()
    {
        var role = new Role
        {
            Name = "Moderator",
            Description = "Community moderator - can moderate users and view reports.",
            Priority = 50,
            IsSystemRole = true,
            IsGlobal = false // Can be assigned per-world
        };

        var moderatorPermissions = new[]
        {
            PermissionCategory.Users_View,
            PermissionCategory.Jobs_View,
            PermissionCategory.Aircraft_View,
            PermissionCategory.Licenses_View,
            PermissionCategory.Chat_Moderate,
            PermissionCategory.Reports_View,
            PermissionCategory.Reports_Resolve,
            PermissionCategory.Violations_Issue
        };

        foreach (var permission in moderatorPermissions)
        {
            role.Permissions.Add(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission,
                IsGranted = true
            });
        }

        return role;
    }

    /// <summary>
    /// Creates the default Player role with basic permissions.
    /// </summary>
    private static Role CreatePlayerRole()
    {
        var role = new Role
        {
            Name = "Player",
            Description = "Default role for all players.",
            Priority = 0,
            IsSystemRole = true,
            IsGlobal = true
        };

        var playerPermissions = new[]
        {
            PermissionCategory.Jobs_View,
            PermissionCategory.Aircraft_View,
            PermissionCategory.Licenses_View
        };

        foreach (var permission in playerPermissions)
        {
            role.Permissions.Add(new RolePermission
            {
                RoleId = role.Id,
                Permission = permission,
                IsGranted = true
            });
        }

        return role;
    }

    /// <summary>
    /// Seeds the default cargo types if they don't exist.
    /// </summary>
    private async Task SeedCargoTypesAsync()
    {
        if (await _context.CargoTypes.AnyAsync())
        {
            _logger.LogInformation("Cargo types already seeded, skipping.");
            return;
        }

        _logger.LogInformation("Seeding cargo types...");

        var cargoTypes = GetDefaultCargoTypes();

        _context.CargoTypes.AddRange(cargoTypes);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} cargo types.", cargoTypes.Count);
    }

    /// <summary>
    /// Returns the default cargo types to seed.
    /// </summary>
    private static List<CargoType> GetDefaultCargoTypes()
    {
        var cargoTypes = new List<CargoType>();

        // General Cargo
        cargoTypes.AddRange(new[]
        {
            new CargoType
            {
                Category = CargoCategory.GeneralCargo,
                Subcategory = "Industrial",
                Name = "Industrial Equipment",
                Description = "Heavy machinery and industrial equipment",
                BaseRatePerLb = 0.45m,
                MinWeightLbs = 500,
                MaxWeightLbs = 25000,
                DensityFactor = 0.08m
            },
            new CargoType
            {
                Category = CargoCategory.GeneralCargo,
                Subcategory = "Consumer Goods",
                Name = "Electronics",
                Description = "Consumer electronics and technology products",
                BaseRatePerLb = 0.75m,
                MinWeightLbs = 100,
                MaxWeightLbs = 5000,
                DensityFactor = 0.15m
            },
            new CargoType
            {
                Category = CargoCategory.GeneralCargo,
                Subcategory = "Consumer Goods",
                Name = "Clothing",
                Description = "Apparel and textile products",
                BaseRatePerLb = 0.55m,
                MinWeightLbs = 200,
                MaxWeightLbs = 8000,
                DensityFactor = 0.25m
            },
            new CargoType
            {
                Category = CargoCategory.GeneralCargo,
                Subcategory = "Manufacturing",
                Name = "Auto Parts",
                Description = "Automotive parts and components",
                BaseRatePerLb = 0.50m,
                MinWeightLbs = 300,
                MaxWeightLbs = 15000,
                DensityFactor = 0.10m
            },
            new CargoType
            {
                Category = CargoCategory.GeneralCargo,
                Subcategory = "Construction",
                Name = "Building Materials",
                Description = "Construction and building supplies",
                BaseRatePerLb = 0.35m,
                MinWeightLbs = 1000,
                MaxWeightLbs = 30000,
                DensityFactor = 0.06m
            }
        });

        // Perishable Cargo
        cargoTypes.AddRange(new[]
        {
            new CargoType
            {
                Category = CargoCategory.Perishable,
                Subcategory = "Food",
                Name = "Fresh Produce",
                Description = "Fresh fruits and vegetables",
                BaseRatePerLb = 0.85m,
                MinWeightLbs = 200,
                MaxWeightLbs = 10000,
                DensityFactor = 0.20m,
                IsTemperatureSensitive = true,
                IsTimeCritical = true
            },
            new CargoType
            {
                Category = CargoCategory.Perishable,
                Subcategory = "Food",
                Name = "Seafood",
                Description = "Fresh fish and seafood",
                BaseRatePerLb = 1.10m,
                MinWeightLbs = 100,
                MaxWeightLbs = 5000,
                DensityFactor = 0.15m,
                IsTemperatureSensitive = true,
                IsTimeCritical = true,
                PayoutMultiplier = 1.2m
            },
            new CargoType
            {
                Category = CargoCategory.Perishable,
                Subcategory = "Floral",
                Name = "Fresh Flowers",
                Description = "Cut flowers and floral arrangements",
                BaseRatePerLb = 1.50m,
                MinWeightLbs = 50,
                MaxWeightLbs = 2000,
                DensityFactor = 0.40m,
                IsTemperatureSensitive = true,
                IsTimeCritical = true,
                PayoutMultiplier = 1.3m
            },
            new CargoType
            {
                Category = CargoCategory.Perishable,
                Subcategory = "Food",
                Name = "Frozen Goods",
                Description = "Frozen food products",
                BaseRatePerLb = 0.70m,
                MinWeightLbs = 300,
                MaxWeightLbs = 15000,
                DensityFactor = 0.12m,
                IsTemperatureSensitive = true
            }
        });

        // Medical Cargo
        cargoTypes.AddRange(new[]
        {
            new CargoType
            {
                Category = CargoCategory.Medical,
                Subcategory = "Pharmaceuticals",
                Name = "Medications",
                Description = "Prescription and OTC medications",
                BaseRatePerLb = 2.50m,
                MinWeightLbs = 50,
                MaxWeightLbs = 3000,
                DensityFactor = 0.20m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "Medical",
                IsTemperatureSensitive = true
            },
            new CargoType
            {
                Category = CargoCategory.Medical,
                Subcategory = "Emergency",
                Name = "Medical Supplies",
                Description = "Emergency medical supplies and equipment",
                BaseRatePerLb = 3.00m,
                MinWeightLbs = 25,
                MaxWeightLbs = 2000,
                DensityFactor = 0.25m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "Medical",
                IsTimeCritical = true,
                PayoutMultiplier = 1.5m
            },
            new CargoType
            {
                Category = CargoCategory.Medical,
                Subcategory = "Emergency",
                Name = "Organ Transport",
                Description = "Human organs for transplant - extremely time critical",
                BaseRatePerLb = 50.00m,
                MinWeightLbs = 5,
                MaxWeightLbs = 100,
                DensityFactor = 0.30m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "Medical-Critical",
                IsTemperatureSensitive = true,
                IsTimeCritical = true,
                PayoutMultiplier = 3.0m
            },
            new CargoType
            {
                Category = CargoCategory.Medical,
                Subcategory = "Laboratory",
                Name = "Lab Samples",
                Description = "Medical laboratory samples",
                BaseRatePerLb = 5.00m,
                MinWeightLbs = 10,
                MaxWeightLbs = 500,
                DensityFactor = 0.30m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "Medical",
                IsTemperatureSensitive = true,
                IsTimeCritical = true
            }
        });

        // Hazardous Cargo
        cargoTypes.AddRange(new[]
        {
            new CargoType
            {
                Category = CargoCategory.Hazardous,
                Subcategory = "Chemicals",
                Name = "Industrial Chemicals",
                Description = "Non-flammable industrial chemicals",
                BaseRatePerLb = 1.25m,
                MinWeightLbs = 200,
                MaxWeightLbs = 8000,
                DensityFactor = 0.10m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "DG-Class8",
                PayoutMultiplier = 1.3m
            },
            new CargoType
            {
                Category = CargoCategory.Hazardous,
                Subcategory = "Flammable",
                Name = "Fuel Additives",
                Description = "Aviation and automotive fuel additives",
                BaseRatePerLb = 1.50m,
                MinWeightLbs = 100,
                MaxWeightLbs = 5000,
                DensityFactor = 0.12m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "DG-Class3",
                PayoutMultiplier = 1.4m
            },
            new CargoType
            {
                Category = CargoCategory.Hazardous,
                Subcategory = "Radioactive",
                Name = "Medical Isotopes",
                Description = "Radioactive materials for medical use",
                BaseRatePerLb = 25.00m,
                MinWeightLbs = 5,
                MaxWeightLbs = 200,
                DensityFactor = 0.50m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "DG-Class7",
                IsTimeCritical = true,
                PayoutMultiplier = 2.0m
            }
        });

        // High Value Cargo
        cargoTypes.AddRange(new[]
        {
            new CargoType
            {
                Category = CargoCategory.HighValue,
                Subcategory = "Currency",
                Name = "Cash Transport",
                Description = "Bank notes and currency transfers",
                BaseRatePerLb = 5.00m,
                MinWeightLbs = 50,
                MaxWeightLbs = 2000,
                DensityFactor = 0.05m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "Security"
            },
            new CargoType
            {
                Category = CargoCategory.HighValue,
                Subcategory = "Jewelry",
                Name = "Precious Metals",
                Description = "Gold, silver, and precious metals",
                BaseRatePerLb = 8.00m,
                MinWeightLbs = 20,
                MaxWeightLbs = 500,
                DensityFactor = 0.02m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "Security"
            },
            new CargoType
            {
                Category = CargoCategory.HighValue,
                Subcategory = "Art",
                Name = "Fine Art",
                Description = "Paintings, sculptures, and artwork",
                BaseRatePerLb = 10.00m,
                MinWeightLbs = 25,
                MaxWeightLbs = 1000,
                DensityFactor = 0.50m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "Fragile-Security"
            },
            new CargoType
            {
                Category = CargoCategory.HighValue,
                Subcategory = "Technology",
                Name = "Server Equipment",
                Description = "High-end computing and server hardware",
                BaseRatePerLb = 3.00m,
                MinWeightLbs = 100,
                MaxWeightLbs = 3000,
                DensityFactor = 0.15m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "Fragile"
            }
        });

        // Live Animals
        cargoTypes.AddRange(new[]
        {
            new CargoType
            {
                Category = CargoCategory.LiveAnimals,
                Subcategory = "Pets",
                Name = "Pet Transport",
                Description = "Domestic pets - dogs, cats, etc.",
                BaseRatePerLb = 1.50m,
                MinWeightLbs = 10,
                MaxWeightLbs = 500,
                DensityFactor = 0.40m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "LiveAnimals",
                IsTemperatureSensitive = true
            },
            new CargoType
            {
                Category = CargoCategory.LiveAnimals,
                Subcategory = "Livestock",
                Name = "Livestock Transport",
                Description = "Cattle, sheep, and farm animals",
                BaseRatePerLb = 0.80m,
                MinWeightLbs = 500,
                MaxWeightLbs = 15000,
                DensityFactor = 0.35m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "LiveAnimals",
                IsTemperatureSensitive = true
            },
            new CargoType
            {
                Category = CargoCategory.LiveAnimals,
                Subcategory = "Exotic",
                Name = "Zoo Animals",
                Description = "Exotic animals for zoos and sanctuaries",
                BaseRatePerLb = 3.00m,
                MinWeightLbs = 50,
                MaxWeightLbs = 5000,
                DensityFactor = 0.50m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "LiveAnimals-Exotic",
                IsTemperatureSensitive = true,
                PayoutMultiplier = 1.5m
            },
            new CargoType
            {
                Category = CargoCategory.LiveAnimals,
                Subcategory = "Racing",
                Name = "Racehorses",
                Description = "Thoroughbred horses for racing",
                BaseRatePerLb = 2.50m,
                MinWeightLbs = 800,
                MaxWeightLbs = 3000,
                DensityFactor = 0.60m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "LiveAnimals-Equine",
                IsTemperatureSensitive = true,
                PayoutMultiplier = 1.8m
            }
        });

        // Fragile Cargo
        cargoTypes.AddRange(new[]
        {
            new CargoType
            {
                Category = CargoCategory.Fragile,
                Subcategory = "Instruments",
                Name = "Musical Instruments",
                Description = "Professional musical instruments",
                BaseRatePerLb = 2.00m,
                MinWeightLbs = 20,
                MaxWeightLbs = 1000,
                DensityFactor = 0.35m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "Fragile"
            },
            new CargoType
            {
                Category = CargoCategory.Fragile,
                Subcategory = "Antiques",
                Name = "Antiques",
                Description = "Antique furniture and collectibles",
                BaseRatePerLb = 3.50m,
                MinWeightLbs = 50,
                MaxWeightLbs = 2000,
                DensityFactor = 0.30m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "Fragile"
            },
            new CargoType
            {
                Category = CargoCategory.Fragile,
                Subcategory = "Glass",
                Name = "Glassware",
                Description = "Scientific and specialty glassware",
                BaseRatePerLb = 1.80m,
                MinWeightLbs = 50,
                MaxWeightLbs = 1500,
                DensityFactor = 0.40m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "Fragile"
            }
        });

        // Mail and Parcels
        cargoTypes.AddRange(new[]
        {
            new CargoType
            {
                Category = CargoCategory.Mail,
                Subcategory = "Priority",
                Name = "Priority Mail",
                Description = "Priority postal service mail",
                BaseRatePerLb = 0.65m,
                MinWeightLbs = 100,
                MaxWeightLbs = 5000,
                DensityFactor = 0.20m,
                IsTimeCritical = true
            },
            new CargoType
            {
                Category = CargoCategory.Mail,
                Subcategory = "Express",
                Name = "Express Documents",
                Description = "Time-critical documents and legal papers",
                BaseRatePerLb = 1.20m,
                MinWeightLbs = 20,
                MaxWeightLbs = 500,
                DensityFactor = 0.30m,
                IsTimeCritical = true,
                PayoutMultiplier = 1.3m
            },
            new CargoType
            {
                Category = CargoCategory.Parcels,
                Subcategory = "E-commerce",
                Name = "E-Commerce Packages",
                Description = "Online retail shipments",
                BaseRatePerLb = 0.70m,
                MinWeightLbs = 200,
                MaxWeightLbs = 10000,
                DensityFactor = 0.22m
            },
            new CargoType
            {
                Category = CargoCategory.Parcels,
                Subcategory = "Same-Day",
                Name = "Same-Day Delivery",
                Description = "Urgent same-day parcel delivery",
                BaseRatePerLb = 1.50m,
                MinWeightLbs = 50,
                MaxWeightLbs = 2000,
                DensityFactor = 0.25m,
                IsTimeCritical = true,
                PayoutMultiplier = 1.5m
            }
        });

        // Oversized Cargo
        cargoTypes.AddRange(new[]
        {
            new CargoType
            {
                Category = CargoCategory.Oversized,
                Subcategory = "Vehicles",
                Name = "Vehicle Transport",
                Description = "Cars, motorcycles, and small vehicles",
                BaseRatePerLb = 0.60m,
                MinWeightLbs = 1000,
                MaxWeightLbs = 10000,
                DensityFactor = 0.08m
            },
            new CargoType
            {
                Category = CargoCategory.Oversized,
                Subcategory = "Machinery",
                Name = "Heavy Machinery",
                Description = "Construction and agricultural equipment",
                BaseRatePerLb = 0.45m,
                MinWeightLbs = 2000,
                MaxWeightLbs = 50000,
                DensityFactor = 0.06m
            },
            new CargoType
            {
                Category = CargoCategory.Oversized,
                Subcategory = "Aircraft Parts",
                Name = "Aircraft Components",
                Description = "Aircraft engines and large components",
                BaseRatePerLb = 0.80m,
                MinWeightLbs = 500,
                MaxWeightLbs = 20000,
                DensityFactor = 0.10m,
                RequiresSpecialHandling = true,
                SpecialHandlingType = "Oversized"
            }
        });

        return cargoTypes;
    }

    /// <summary>
    /// Seeds the default license types if they don't exist.
    /// </summary>
    private async Task SeedLicenseTypesAsync()
    {
        if (await _context.LicenseTypes.AnyAsync())
        {
            _logger.LogInformation("License types already seeded, skipping.");
            return;
        }

        _logger.LogInformation("Seeding license types...");

        var licenseTypes = GetDefaultLicenseTypes();

        _context.LicenseTypes.AddRange(licenseTypes);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} license types.", licenseTypes.Count);
    }

    /// <summary>
    /// Returns the default license types to seed.
    /// </summary>
    private static List<LicenseType> GetDefaultLicenseTypes()
    {
        var licenseTypes = new List<LicenseType>();

        // Core Licenses
        licenseTypes.AddRange(new[]
        {
            // Discovery Flight - Entry point license (permanent)
            new LicenseType
            {
                Code = "DISCOVERY",
                Name = "Discovery Flight",
                Description = "Basic flight certification proving fundamental aircraft control skills. Required before advancing to higher licenses.",
                Category = LicenseCategory.Core,
                BaseExamCost = 500m,
                ExamDurationMinutes = 10,
                PassingScore = 70,
                RequiredAircraftCategory = AircraftCategory.SEP,
                ValidityGameDays = null, // Permanent
                BaseRenewalCost = null,
                PrerequisiteLicensesJson = null,
                DisplayOrder = 1,
                IsActive = true
            },

            // PPL - Private Pilot License
            new LicenseType
            {
                Code = "PPL",
                Name = "Private Pilot License",
                Description = "Authorizes private flight operations. Unlocks access to CPL, Night Rating, Instrument Rating, and Multi-Engine Rating exams.",
                Category = LicenseCategory.Core,
                BaseExamCost = 2000m,
                ExamDurationMinutes = 25,
                PassingScore = 70,
                RequiredAircraftCategory = AircraftCategory.SEP,
                ValidityGameDays = 180, // 6 game months
                BaseRenewalCost = 500m,
                PrerequisiteLicensesJson = "[\"DISCOVERY\"]",
                DisplayOrder = 2,
                IsActive = true
            },

            // CPL - Commercial Pilot License
            new LicenseType
            {
                Code = "CPL",
                Name = "Commercial Pilot License",
                Description = "Authorizes commercial flight operations for hire. Required for most paying jobs and to pursue type ratings.",
                Category = LicenseCategory.Core,
                BaseExamCost = 5000m,
                ExamDurationMinutes = 30,
                PassingScore = 70,
                RequiredAircraftCategory = AircraftCategory.SEP,
                ValidityGameDays = 90, // 3 game months
                BaseRenewalCost = 1500m,
                PrerequisiteLicensesJson = "[\"PPL\"]",
                DisplayOrder = 3,
                IsActive = true
            },

            // ATPL - Airline Transport Pilot License
            new LicenseType
            {
                Code = "ATPL",
                Name = "Airline Transport Pilot License",
                Description = "The highest pilot certification. Required for airline operations and command of large transport aircraft.",
                Category = LicenseCategory.Core,
                BaseExamCost = 10000m,
                ExamDurationMinutes = 90,
                PassingScore = 70,
                RequiredAircraftCategory = AircraftCategory.MEP,
                ValidityGameDays = 90, // 3 game months
                BaseRenewalCost = 3000m,
                PrerequisiteLicensesJson = "[\"CPL\", \"IR\", \"MEP\"]",
                DisplayOrder = 4,
                IsActive = true
            }
        });

        // Endorsements
        licenseTypes.AddRange(new[]
        {
            // Night Rating
            new LicenseType
            {
                Code = "NIGHT",
                Name = "Night Rating",
                Description = "Authorizes flight operations during night hours. Unlocks night jobs with premium pay.",
                Category = LicenseCategory.Endorsement,
                BaseExamCost = 3000m,
                ExamDurationMinutes = 30,
                PassingScore = 70,
                RequiredAircraftCategory = AircraftCategory.SEP,
                ValidityGameDays = 180, // 6 game months
                BaseRenewalCost = 1000m,
                PrerequisiteLicensesJson = "[\"PPL\"]",
                DisplayOrder = 10,
                IsActive = true
            },

            // Instrument Rating
            new LicenseType
            {
                Code = "IR",
                Name = "Instrument Rating",
                Description = "Authorizes IFR (Instrument Flight Rules) operations. Required for most professional flying and ATPL.",
                Category = LicenseCategory.Endorsement,
                BaseExamCost = 5000m,
                ExamDurationMinutes = 45,
                PassingScore = 70,
                RequiredAircraftCategory = AircraftCategory.SEP,
                ValidityGameDays = 90, // 3 game months
                BaseRenewalCost = 1500m,
                PrerequisiteLicensesJson = "[\"PPL\"]",
                DisplayOrder = 11,
                IsActive = true
            },

            // Multi-Engine Piston Rating
            new LicenseType
            {
                Code = "MEP",
                Name = "Multi-Engine Rating",
                Description = "Authorizes operation of multi-engine piston aircraft. Required for twin-engine aircraft and ATPL.",
                Category = LicenseCategory.Endorsement,
                BaseExamCost = 4000m,
                ExamDurationMinutes = 45,
                PassingScore = 70,
                RequiredAircraftCategory = AircraftCategory.MEP,
                ValidityGameDays = 180, // 6 game months
                BaseRenewalCost = 1200m,
                PrerequisiteLicensesJson = "[\"PPL\"]",
                DisplayOrder = 12,
                IsActive = true
            },

            // Dangerous Goods
            new LicenseType
            {
                Code = "DG",
                Name = "Dangerous Goods Certification",
                Description = "Authorizes transport of hazardous materials. Required for high-paying hazmat cargo jobs.",
                Category = LicenseCategory.Certification,
                BaseExamCost = 2000m,
                ExamDurationMinutes = 30,
                PassingScore = 70,
                RequiredAircraftCategory = null, // Any aircraft
                ValidityGameDays = 180, // 6 game months
                BaseRenewalCost = 600m,
                PrerequisiteLicensesJson = "[\"CPL\"]",
                DisplayOrder = 20,
                IsActive = true
            }
        });

        // Type Ratings - Basic examples (more can be added dynamically)
        licenseTypes.AddRange(new[]
        {
            new LicenseType
            {
                Code = "TYPE-TURBOPROP",
                Name = "Turboprop Type Rating",
                Description = "Authorizes operation of turboprop aircraft. Required for aircraft like the King Air, TBM, and Pilatus PC-12.",
                Category = LicenseCategory.TypeRating,
                BaseExamCost = 5000m,
                ExamDurationMinutes = 45,
                PassingScore = 70,
                RequiredAircraftCategory = AircraftCategory.Turboprop,
                ValidityGameDays = 90, // 3 game months
                BaseRenewalCost = 1250m, // 25% of exam cost
                PrerequisiteLicensesJson = "[\"CPL\"]",
                DisplayOrder = 30,
                IsActive = true
            },

            new LicenseType
            {
                Code = "TYPE-RJ",
                Name = "Regional Jet Type Rating",
                Description = "Authorizes operation of regional jet aircraft. Required for aircraft like the CRJ and ERJ series.",
                Category = LicenseCategory.TypeRating,
                BaseExamCost = 10000m,
                ExamDurationMinutes = 60,
                PassingScore = 70,
                RequiredAircraftCategory = AircraftCategory.RegionalJet,
                ValidityGameDays = 90, // 3 game months
                BaseRenewalCost = 2500m, // 25% of exam cost
                PrerequisiteLicensesJson = "[\"CPL\", \"IR\"]",
                DisplayOrder = 31,
                IsActive = true
            },

            new LicenseType
            {
                Code = "TYPE-NARROWBODY",
                Name = "Narrowbody Type Rating",
                Description = "Authorizes operation of narrowbody airliners. Required for aircraft like the A320 and B737 families.",
                Category = LicenseCategory.TypeRating,
                BaseExamCost = 15000m,
                ExamDurationMinutes = 75,
                PassingScore = 70,
                RequiredAircraftCategory = AircraftCategory.NarrowBody,
                ValidityGameDays = 90, // 3 game months
                BaseRenewalCost = 3750m, // 25% of exam cost
                PrerequisiteLicensesJson = "[\"ATPL\"]",
                DisplayOrder = 32,
                IsActive = true
            },

            new LicenseType
            {
                Code = "TYPE-WIDEBODY",
                Name = "Widebody Type Rating",
                Description = "Authorizes operation of widebody airliners. Required for aircraft like the A350, B777, and B787.",
                Category = LicenseCategory.TypeRating,
                BaseExamCost = 20000m,
                ExamDurationMinutes = 90,
                PassingScore = 70,
                RequiredAircraftCategory = AircraftCategory.WideBody,
                ValidityGameDays = 90, // 3 game months
                BaseRenewalCost = 5000m, // 25% of exam cost
                PrerequisiteLicensesJson = "[\"ATPL\"]",
                DisplayOrder = 33,
                IsActive = true
            }
        });

        return licenseTypes;
    }
}
