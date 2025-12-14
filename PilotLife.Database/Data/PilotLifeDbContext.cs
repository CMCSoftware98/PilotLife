using Microsoft.EntityFrameworkCore;
using PilotLife.Domain.Common;
using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.Database.Data;

public class PilotLifeDbContext : DbContext
{
    public PilotLifeDbContext(DbContextOptions<PilotLifeDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Airport> Airports => Set<Airport>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Aircraft> Aircraft => Set<Aircraft>();
    public DbSet<AircraftRequest> AircraftRequests => Set<AircraftRequest>();
    public DbSet<TrackedFlight> TrackedFlights => Set<TrackedFlight>();
    public DbSet<FlightJob> FlightJobs => Set<FlightJob>();
    public DbSet<FlightFinancials> FlightFinancials => Set<FlightFinancials>();

    // World entities
    public DbSet<World> Worlds => Set<World>();
    public DbSet<WorldSettings> WorldSettings => Set<WorldSettings>();
    public DbSet<PlayerWorld> PlayerWorlds => Set<PlayerWorld>();

    // IAM entities
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    // Aircraft ownership entities
    public DbSet<OwnedAircraft> OwnedAircraft => Set<OwnedAircraft>();
    public DbSet<AircraftComponent> AircraftComponents => Set<AircraftComponent>();
    public DbSet<AircraftModification> AircraftModifications => Set<AircraftModification>();
    public DbSet<MaintenanceLog> MaintenanceLogs => Set<MaintenanceLog>();

    // Marketplace entities
    public DbSet<AircraftDealer> AircraftDealers => Set<AircraftDealer>();
    public DbSet<DealerInventory> DealerInventory => Set<DealerInventory>();
    public DbSet<AircraftPurchase> AircraftPurchases => Set<AircraftPurchase>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = DateTimeOffset.UtcNow;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply base entity configuration to all BaseEntity-derived types
        ConfigureBaseEntity<User>(modelBuilder);
        ConfigureBaseEntity<Job>(modelBuilder);
        ConfigureBaseEntity<RefreshToken>(modelBuilder);
        ConfigureBaseEntity<Aircraft>(modelBuilder);
        ConfigureBaseEntity<AircraftRequest>(modelBuilder);
        ConfigureBaseEntity<TrackedFlight>(modelBuilder);
        ConfigureBaseEntity<FlightJob>(modelBuilder);
        ConfigureBaseEntity<FlightFinancials>(modelBuilder);
        ConfigureBaseEntity<World>(modelBuilder);
        ConfigureBaseEntity<WorldSettings>(modelBuilder);
        ConfigureBaseEntity<PlayerWorld>(modelBuilder);
        ConfigureBaseEntity<Role>(modelBuilder);
        ConfigureBaseEntity<UserRole>(modelBuilder);
        ConfigureBaseEntity<OwnedAircraft>(modelBuilder);
        ConfigureBaseEntity<AircraftComponent>(modelBuilder);
        ConfigureBaseEntity<AircraftModification>(modelBuilder);
        ConfigureBaseEntity<MaintenanceLog>(modelBuilder);
        ConfigureBaseEntity<AircraftDealer>(modelBuilder);
        ConfigureBaseEntity<DealerInventory>(modelBuilder);
        ConfigureBaseEntity<AircraftPurchase>(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureAirport(modelBuilder);
        ConfigureJob(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
        ConfigureAircraft(modelBuilder);
        ConfigureAircraftRequest(modelBuilder);
        ConfigureTrackedFlight(modelBuilder);
        ConfigureFlightJob(modelBuilder);
        ConfigureFlightFinancials(modelBuilder);
        ConfigureWorld(modelBuilder);
        ConfigureWorldSettings(modelBuilder);
        ConfigurePlayerWorld(modelBuilder);
        ConfigureRole(modelBuilder);
        ConfigureRolePermission(modelBuilder);
        ConfigureUserRole(modelBuilder);
        ConfigureOwnedAircraft(modelBuilder);
        ConfigureAircraftComponent(modelBuilder);
        ConfigureAircraftModification(modelBuilder);
        ConfigureMaintenanceLog(modelBuilder);
        ConfigureAircraftDealer(modelBuilder);
        ConfigureDealerInventory(modelBuilder);
        ConfigureAircraftPurchase(modelBuilder);
    }

    private static void ConfigureBaseEntity<T>(ModelBuilder modelBuilder) where T : BaseEntity
    {
        modelBuilder.Entity<T>(entity =>
        {
            entity.HasKey(e => e.Id);

            // UUID v7 generated on application side
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            entity.Property(e => e.ModifiedAt)
                .HasColumnName("modified_at")
                .HasColumnType("timestamptz");
        });
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.Property(e => e.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash")
                .IsRequired();

            entity.Property(e => e.ExperienceLevel)
                .HasColumnName("experience_level")
                .HasMaxLength(50);

            entity.Property(e => e.EmailVerified)
                .HasColumnName("email_verified")
                .HasDefaultValue(false);

            entity.Property(e => e.NewsletterSubscribed)
                .HasColumnName("newsletter_subscribed")
                .HasDefaultValue(false);

            entity.Property(e => e.LastLoginAt)
                .HasColumnName("last_login_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.CurrentAirportId)
                .HasColumnName("current_airport_id");

            entity.Property(e => e.HomeAirportId)
                .HasColumnName("home_airport_id");

            entity.Property(e => e.Balance)
                .HasColumnName("balance")
                .HasDefaultValue(0m);

            entity.Property(e => e.TotalFlightMinutes)
                .HasColumnName("total_flight_minutes")
                .HasDefaultValue(0);

            entity.HasOne(e => e.CurrentAirport)
                .WithMany()
                .HasForeignKey(e => e.CurrentAirportId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.HomeAirport)
                .WithMany()
                .HasForeignKey(e => e.HomeAirportId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.RefreshTokens)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAirport(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Airport>(entity =>
        {
            entity.ToTable("airports");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Ident)
                .HasColumnName("ident")
                .HasMaxLength(10)
                .IsRequired();

            entity.HasIndex(e => e.Ident)
                .IsUnique();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.IataCode)
                .HasColumnName("iata_code")
                .HasMaxLength(3);

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Latitude)
                .HasColumnName("latitude");

            entity.Property(e => e.Longitude)
                .HasColumnName("longitude");

            entity.Property(e => e.ElevationFt)
                .HasColumnName("elevation_ft");

            entity.Property(e => e.Country)
                .HasColumnName("country")
                .HasMaxLength(10);

            entity.Property(e => e.Municipality)
                .HasColumnName("municipality")
                .HasMaxLength(100);

            entity.HasIndex(e => e.IataCode);
            entity.HasIndex(e => e.Name);
        });
    }

    private static void ConfigureJob(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("jobs");

            entity.Property(e => e.DepartureAirportId)
                .HasColumnName("departure_airport_id")
                .IsRequired();

            entity.Property(e => e.ArrivalAirportId)
                .HasColumnName("arrival_airport_id")
                .IsRequired();

            entity.Property(e => e.CargoType)
                .HasColumnName("cargo_type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Weight)
                .HasColumnName("weight");

            entity.Property(e => e.Payout)
                .HasColumnName("payout")
                .HasPrecision(18, 2);

            entity.Property(e => e.DistanceNm)
                .HasColumnName("distance_nm");

            entity.Property(e => e.EstimatedFlightTimeMinutes)
                .HasColumnName("estimated_flight_time_minutes");

            entity.Property(e => e.RequiredAircraftType)
                .HasColumnName("required_aircraft_type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.IsCompleted)
                .HasColumnName("is_completed")
                .HasDefaultValue(false);

            entity.Property(e => e.AssignedToUserId)
                .HasColumnName("assigned_to_user_id");

            entity.HasOne(e => e.DepartureAirport)
                .WithMany()
                .HasForeignKey(e => e.DepartureAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ArrivalAirport)
                .WithMany()
                .HasForeignKey(e => e.ArrivalAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedToUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.DepartureAirportId);
            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => e.AssignedToUserId);
        });
    }

    private static void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");

            entity.Property(e => e.Token)
                .HasColumnName("token")
                .IsRequired();

            entity.HasIndex(e => e.Token)
                .IsUnique();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            entity.Property(e => e.RevokedAt)
                .HasColumnName("revoked_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.ReplacedByToken)
                .HasColumnName("replaced_by_token");

            entity.HasIndex(e => e.UserId);

            // Ignore computed properties
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsRevoked);
            entity.Ignore(e => e.IsActive);
        });
    }

    private static void ConfigureAircraft(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aircraft>(entity =>
        {
            entity.ToTable("aircraft");

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(256)
                .IsRequired();

            entity.HasIndex(e => e.Title)
                .IsUnique();

            entity.Property(e => e.AtcType)
                .HasColumnName("atc_type")
                .HasMaxLength(64);

            entity.Property(e => e.AtcModel)
                .HasColumnName("atc_model")
                .HasMaxLength(64);

            entity.Property(e => e.Category)
                .HasColumnName("category")
                .HasMaxLength(256);

            entity.Property(e => e.EngineType)
                .HasColumnName("engine_type");

            entity.Property(e => e.EngineTypeStr)
                .HasColumnName("engine_type_str")
                .HasMaxLength(50);

            entity.Property(e => e.NumberOfEngines)
                .HasColumnName("number_of_engines");

            entity.Property(e => e.MaxGrossWeightLbs)
                .HasColumnName("max_gross_weight_lbs");

            entity.Property(e => e.EmptyWeightLbs)
                .HasColumnName("empty_weight_lbs");

            entity.Property(e => e.CruiseSpeedKts)
                .HasColumnName("cruise_speed_kts");

            entity.Property(e => e.SimulatorVersion)
                .HasColumnName("simulator_version")
                .HasMaxLength(20);

            entity.Property(e => e.IsApproved)
                .HasColumnName("is_approved")
                .HasDefaultValue(false);
        });
    }

    private static void ConfigureAircraftRequest(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AircraftRequest>(entity =>
        {
            entity.ToTable("aircraft_requests");

            entity.Property(e => e.AircraftTitle)
                .HasColumnName("aircraft_title")
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(e => e.AtcType)
                .HasColumnName("atc_type")
                .HasMaxLength(64);

            entity.Property(e => e.AtcModel)
                .HasColumnName("atc_model")
                .HasMaxLength(64);

            entity.Property(e => e.Category)
                .HasColumnName("category")
                .HasMaxLength(256);

            entity.Property(e => e.EngineType)
                .HasColumnName("engine_type");

            entity.Property(e => e.EngineTypeStr)
                .HasColumnName("engine_type_str")
                .HasMaxLength(50);

            entity.Property(e => e.NumberOfEngines)
                .HasColumnName("number_of_engines");

            entity.Property(e => e.MaxGrossWeightLbs)
                .HasColumnName("max_gross_weight_lbs");

            entity.Property(e => e.EmptyWeightLbs)
                .HasColumnName("empty_weight_lbs");

            entity.Property(e => e.CruiseSpeedKts)
                .HasColumnName("cruise_speed_kts");

            entity.Property(e => e.SimulatorVersion)
                .HasColumnName("simulator_version")
                .HasMaxLength(20);

            entity.Property(e => e.RequestedByUserId)
                .HasColumnName("requested_by_user_id")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(AircraftRequestStatus.Pending);

            entity.Property(e => e.ReviewNotes)
                .HasColumnName("review_notes")
                .HasMaxLength(1000);

            entity.Property(e => e.ReviewedByUserId)
                .HasColumnName("reviewed_by_user_id");

            entity.Property(e => e.ReviewedAt)
                .HasColumnName("reviewed_at")
                .HasColumnType("timestamptz");

            entity.HasOne(e => e.RequestedByUser)
                .WithMany()
                .HasForeignKey(e => e.RequestedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ReviewedByUser)
                .WithMany()
                .HasForeignKey(e => e.ReviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequestedByUserId);
            entity.HasIndex(e => e.AircraftTitle);
        });
    }

    private static void ConfigureTrackedFlight(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrackedFlight>(entity =>
        {
            entity.ToTable("tracked_flights");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.State)
                .HasColumnName("state")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.AircraftId)
                .HasColumnName("aircraft_id");

            entity.Property(e => e.AircraftTitle)
                .HasColumnName("aircraft_title")
                .HasMaxLength(256);

            entity.Property(e => e.AircraftIcaoType)
                .HasColumnName("aircraft_icao_type")
                .HasMaxLength(10);

            entity.Property(e => e.DepartureAirportId)
                .HasColumnName("departure_airport_id");

            entity.Property(e => e.DepartureIcao)
                .HasColumnName("departure_icao")
                .HasMaxLength(10);

            entity.Property(e => e.DepartureTime)
                .HasColumnName("departure_time")
                .HasColumnType("timestamptz");

            entity.Property(e => e.ArrivalAirportId)
                .HasColumnName("arrival_airport_id");

            entity.Property(e => e.ArrivalIcao)
                .HasColumnName("arrival_icao")
                .HasMaxLength(10);

            entity.Property(e => e.ArrivalTime)
                .HasColumnName("arrival_time")
                .HasColumnType("timestamptz");

            entity.Property(e => e.CurrentLatitude)
                .HasColumnName("current_latitude");

            entity.Property(e => e.CurrentLongitude)
                .HasColumnName("current_longitude");

            entity.Property(e => e.CurrentAltitude)
                .HasColumnName("current_altitude");

            entity.Property(e => e.CurrentHeading)
                .HasColumnName("current_heading");

            entity.Property(e => e.CurrentGroundSpeed)
                .HasColumnName("current_ground_speed");

            entity.Property(e => e.LastPositionUpdate)
                .HasColumnName("last_position_update")
                .HasColumnType("timestamptz");

            entity.Property(e => e.FlightTimeMinutes)
                .HasColumnName("flight_time_minutes");

            entity.Property(e => e.DistanceNm)
                .HasColumnName("distance_nm");

            entity.Property(e => e.MaxAltitude)
                .HasColumnName("max_altitude");

            entity.Property(e => e.MaxGroundSpeed)
                .HasColumnName("max_ground_speed");

            entity.Property(e => e.HardLandingCount)
                .HasColumnName("hard_landing_count");

            entity.Property(e => e.OverspeedCount)
                .HasColumnName("overspeed_count");

            entity.Property(e => e.StallWarningCount)
                .HasColumnName("stall_warning_count");

            entity.Property(e => e.LandingRate)
                .HasColumnName("landing_rate");

            entity.Property(e => e.FuelUsedGallons)
                .HasColumnName("fuel_used_gallons");

            entity.Property(e => e.StartFuelGallons)
                .HasColumnName("start_fuel_gallons");

            entity.Property(e => e.EndFuelGallons)
                .HasColumnName("end_fuel_gallons");

            entity.Property(e => e.PayloadWeightLbs)
                .HasColumnName("payload_weight_lbs");

            entity.Property(e => e.TotalWeightLbs)
                .HasColumnName("total_weight_lbs");

            entity.Property(e => e.CompletedAt)
                .HasColumnName("completed_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.Notes)
                .HasColumnName("notes")
                .HasMaxLength(1000);

            entity.Property(e => e.ConnectorSessionId)
                .HasColumnName("connector_session_id")
                .HasMaxLength(100);

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Aircraft)
                .WithMany()
                .HasForeignKey(e => e.AircraftId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.DepartureAirport)
                .WithMany()
                .HasForeignKey(e => e.DepartureAirportId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ArrivalAirport)
                .WithMany()
                .HasForeignKey(e => e.ArrivalAirportId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Financials)
                .WithOne(f => f.TrackedFlight)
                .HasForeignKey<FlightFinancials>(f => f.TrackedFlightId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.State);
            entity.HasIndex(e => e.ConnectorSessionId);
            entity.HasIndex(e => new { e.UserId, e.State });
        });
    }

    private static void ConfigureFlightJob(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FlightJob>(entity =>
        {
            entity.ToTable("flight_jobs");

            entity.Property(e => e.TrackedFlightId)
                .HasColumnName("tracked_flight_id")
                .IsRequired();

            entity.Property(e => e.JobId)
                .HasColumnName("job_id")
                .IsRequired();

            entity.Property(e => e.IsCompleted)
                .HasColumnName("is_completed")
                .HasDefaultValue(false);

            entity.Property(e => e.IsFailed)
                .HasColumnName("is_failed")
                .HasDefaultValue(false);

            entity.Property(e => e.FailureReason)
                .HasColumnName("failure_reason")
                .HasMaxLength(500);

            entity.Property(e => e.ActualPayout)
                .HasColumnName("actual_payout")
                .HasPrecision(18, 2);

            entity.Property(e => e.XpEarned)
                .HasColumnName("xp_earned");

            entity.Property(e => e.ReputationChange)
                .HasColumnName("reputation_change");

            // Relationships
            entity.HasOne(e => e.TrackedFlight)
                .WithMany(f => f.FlightJobs)
                .HasForeignKey(e => e.TrackedFlightId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Job)
                .WithMany()
                .HasForeignKey(e => e.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.TrackedFlightId);
            entity.HasIndex(e => e.JobId);
            entity.HasIndex(e => new { e.TrackedFlightId, e.JobId }).IsUnique();
        });
    }

    private static void ConfigureFlightFinancials(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FlightFinancials>(entity =>
        {
            entity.ToTable("flight_financials");

            entity.Property(e => e.TrackedFlightId)
                .HasColumnName("tracked_flight_id")
                .IsRequired();

            // Revenue
            entity.Property(e => e.JobRevenue)
                .HasColumnName("job_revenue")
                .HasPrecision(18, 2);

            entity.Property(e => e.OnTimeBonus)
                .HasColumnName("on_time_bonus")
                .HasPrecision(18, 2);

            entity.Property(e => e.LandingBonus)
                .HasColumnName("landing_bonus")
                .HasPrecision(18, 2);

            entity.Property(e => e.FuelEfficiencyBonus)
                .HasColumnName("fuel_efficiency_bonus")
                .HasPrecision(18, 2);

            entity.Property(e => e.OtherBonuses)
                .HasColumnName("other_bonuses")
                .HasPrecision(18, 2);

            // Costs
            entity.Property(e => e.FuelCost)
                .HasColumnName("fuel_cost")
                .HasPrecision(18, 2);

            entity.Property(e => e.LandingFees)
                .HasColumnName("landing_fees")
                .HasPrecision(18, 2);

            entity.Property(e => e.HandlingFees)
                .HasColumnName("handling_fees")
                .HasPrecision(18, 2);

            entity.Property(e => e.NavigationFees)
                .HasColumnName("navigation_fees")
                .HasPrecision(18, 2);

            entity.Property(e => e.MaintenanceCost)
                .HasColumnName("maintenance_cost")
                .HasPrecision(18, 2);

            entity.Property(e => e.InsuranceCost)
                .HasColumnName("insurance_cost")
                .HasPrecision(18, 2);

            entity.Property(e => e.CrewCost)
                .HasColumnName("crew_cost")
                .HasPrecision(18, 2);

            // Penalties
            entity.Property(e => e.LatePenalty)
                .HasColumnName("late_penalty")
                .HasPrecision(18, 2);

            entity.Property(e => e.DamagePenalty)
                .HasColumnName("damage_penalty")
                .HasPrecision(18, 2);

            entity.Property(e => e.IncidentPenalty)
                .HasColumnName("incident_penalty")
                .HasPrecision(18, 2);

            // Ignore computed properties (not stored in DB)
            entity.Ignore(e => e.TotalRevenue);
            entity.Ignore(e => e.TotalCosts);
            entity.Ignore(e => e.TotalPenalties);
            entity.Ignore(e => e.NetProfit);
            entity.Ignore(e => e.IsProfitable);

            // Indexes
            entity.HasIndex(e => e.TrackedFlightId).IsUnique();
        });
    }

    private static void ConfigureWorld(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<World>(entity =>
        {
            entity.ToTable("worlds");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Slug)
                .HasColumnName("slug")
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.Slug).IsUnique();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            entity.Property(e => e.Difficulty)
                .HasColumnName("difficulty")
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.StartingCapital)
                .HasColumnName("starting_capital")
                .HasPrecision(18, 2);

            entity.Property(e => e.JobPayoutMultiplier)
                .HasColumnName("job_payout_multiplier")
                .HasPrecision(5, 2);

            entity.Property(e => e.AircraftPriceMultiplier)
                .HasColumnName("aircraft_price_multiplier")
                .HasPrecision(5, 2);

            entity.Property(e => e.MaintenanceCostMultiplier)
                .HasColumnName("maintenance_cost_multiplier")
                .HasPrecision(5, 2);

            entity.Property(e => e.LicenseCostMultiplier)
                .HasColumnName("license_cost_multiplier")
                .HasPrecision(5, 2);

            entity.Property(e => e.LoanInterestMultiplier)
                .HasColumnName("loan_interest_multiplier")
                .HasPrecision(5, 2);

            entity.Property(e => e.DetectionRiskMultiplier)
                .HasColumnName("detection_risk_multiplier")
                .HasPrecision(5, 2);

            entity.Property(e => e.FineMultiplier)
                .HasColumnName("fine_multiplier")
                .HasPrecision(5, 2);

            entity.Property(e => e.JobExpiryMultiplier)
                .HasColumnName("job_expiry_multiplier")
                .HasPrecision(5, 2);

            entity.Property(e => e.CreditRecoveryMultiplier)
                .HasColumnName("credit_recovery_multiplier")
                .HasPrecision(5, 2);

            entity.Property(e => e.WorkerSalaryMultiplier)
                .HasColumnName("worker_salary_multiplier")
                .HasPrecision(5, 2);

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.IsDefault)
                .HasColumnName("is_default")
                .HasDefaultValue(false);

            entity.Property(e => e.MaxPlayers)
                .HasColumnName("max_players")
                .HasDefaultValue(0);

            entity.HasOne(e => e.Settings)
                .WithOne(s => s.World)
                .HasForeignKey<WorldSettings>(s => s.WorldId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureWorldSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorldSettings>(entity =>
        {
            entity.ToTable("world_settings");

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.JobPayoutMultiplierOverride)
                .HasColumnName("job_payout_multiplier_override")
                .HasPrecision(5, 2);

            entity.Property(e => e.AircraftPriceMultiplierOverride)
                .HasColumnName("aircraft_price_multiplier_override")
                .HasPrecision(5, 2);

            entity.Property(e => e.MaintenanceCostMultiplierOverride)
                .HasColumnName("maintenance_cost_multiplier_override")
                .HasPrecision(5, 2);

            entity.Property(e => e.AllowNewPlayers)
                .HasColumnName("allow_new_players")
                .HasDefaultValue(true);

            entity.Property(e => e.AllowIllegalCargo)
                .HasColumnName("allow_illegal_cargo")
                .HasDefaultValue(true);

            entity.Property(e => e.EnableAuctions)
                .HasColumnName("enable_auctions")
                .HasDefaultValue(true);

            entity.Property(e => e.EnableAICrews)
                .HasColumnName("enable_ai_crews")
                .HasDefaultValue(true);

            entity.Property(e => e.EnableAircraftRental)
                .HasColumnName("enable_aircraft_rental")
                .HasDefaultValue(true);

            entity.Property(e => e.MaxAircraftPerPlayer)
                .HasColumnName("max_aircraft_per_player")
                .HasDefaultValue(0);

            entity.Property(e => e.MaxLoansPerPlayer)
                .HasColumnName("max_loans_per_player")
                .HasDefaultValue(3);

            entity.Property(e => e.MaxWorkersPerPlayer)
                .HasColumnName("max_workers_per_player")
                .HasDefaultValue(10);

            entity.Property(e => e.MaxActiveJobsPerPlayer)
                .HasColumnName("max_active_jobs_per_player")
                .HasDefaultValue(5);

            entity.Property(e => e.RequireApprovalToJoin)
                .HasColumnName("require_approval_to_join")
                .HasDefaultValue(false);

            entity.Property(e => e.EnableChat)
                .HasColumnName("enable_chat")
                .HasDefaultValue(true);

            entity.Property(e => e.EnableReporting)
                .HasColumnName("enable_reporting")
                .HasDefaultValue(true);

            entity.Property(e => e.LastModifiedByUserId)
                .HasColumnName("last_modified_by_user_id");

            entity.HasOne(e => e.LastModifiedByUser)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.WorldId).IsUnique();
        });
    }

    private static void ConfigurePlayerWorld(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerWorld>(entity =>
        {
            entity.ToTable("player_worlds");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.Balance)
                .HasColumnName("balance")
                .HasPrecision(18, 2);

            entity.Property(e => e.CreditScore)
                .HasColumnName("credit_score")
                .HasDefaultValue(650);

            entity.Property(e => e.TotalFlightMinutes)
                .HasColumnName("total_flight_minutes")
                .HasDefaultValue(0);

            entity.Property(e => e.TotalFlights)
                .HasColumnName("total_flights")
                .HasDefaultValue(0);

            entity.Property(e => e.TotalJobsCompleted)
                .HasColumnName("total_jobs_completed")
                .HasDefaultValue(0);

            entity.Property(e => e.TotalEarnings)
                .HasColumnName("total_earnings")
                .HasPrecision(18, 2);

            entity.Property(e => e.TotalSpent)
                .HasColumnName("total_spent")
                .HasPrecision(18, 2);

            entity.Property(e => e.ReputationScore)
                .HasColumnName("reputation_score")
                .HasPrecision(3, 1)
                .HasDefaultValue(3.0m);

            entity.Property(e => e.OnTimeDeliveries)
                .HasColumnName("on_time_deliveries")
                .HasDefaultValue(0);

            entity.Property(e => e.LateDeliveries)
                .HasColumnName("late_deliveries")
                .HasDefaultValue(0);

            entity.Property(e => e.FailedDeliveries)
                .HasColumnName("failed_deliveries")
                .HasDefaultValue(0);

            entity.Property(e => e.ViolationPoints)
                .HasColumnName("violation_points")
                .HasDefaultValue(0);

            entity.Property(e => e.LastViolationAt)
                .HasColumnName("last_violation_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.JoinedAt)
                .HasColumnName("joined_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.LastActiveAt)
                .HasColumnName("last_active_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.CurrentAirportId)
                .HasColumnName("current_airport_id");

            entity.Property(e => e.HomeAirportId)
                .HasColumnName("home_airport_id");

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(u => u.PlayerWorlds)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.World)
                .WithMany(w => w.Players)
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CurrentAirport)
                .WithMany()
                .HasForeignKey(e => e.CurrentAirportId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.HomeAirport)
                .WithMany()
                .HasForeignKey(e => e.HomeAirportId)
                .OnDelete(DeleteBehavior.SetNull);

            // Unique constraint: user can only join a world once
            entity.HasIndex(e => new { e.UserId, e.WorldId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.WorldId);
        });
    }

    private static void ConfigureRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(255);

            entity.Property(e => e.Priority)
                .HasColumnName("priority")
                .HasDefaultValue(0);

            entity.Property(e => e.IsSystemRole)
                .HasColumnName("is_system_role")
                .HasDefaultValue(false);

            entity.Property(e => e.IsGlobal)
                .HasColumnName("is_global")
                .HasDefaultValue(false);
        });
    }

    private static void ConfigureRolePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");

            // Composite primary key
            entity.HasKey(e => new { e.RoleId, e.Permission });

            entity.Property(e => e.RoleId)
                .HasColumnName("role_id");

            entity.Property(e => e.Permission)
                .HasColumnName("permission")
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.IsGranted)
                .HasColumnName("is_granted")
                .HasDefaultValue(true);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Permissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureUserRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.RoleId)
                .HasColumnName("role_id")
                .IsRequired();

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id");

            entity.Property(e => e.GrantedAt)
                .HasColumnName("granted_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.GrantedByUserId)
                .HasColumnName("granted_by_user_id")
                .IsRequired();

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamptz");

            // Ignore computed property
            entity.Ignore(e => e.IsActive);

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.World)
                .WithMany()
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.GrantedByUser)
                .WithMany()
                .HasForeignKey(e => e.GrantedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.RoleId);
            entity.HasIndex(e => new { e.UserId, e.RoleId, e.WorldId });
        });
    }

    private static void ConfigureOwnedAircraft(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OwnedAircraft>(entity =>
        {
            entity.ToTable("owned_aircraft");

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.PlayerWorldId)
                .HasColumnName("player_world_id")
                .IsRequired();

            entity.Property(e => e.AircraftId)
                .HasColumnName("aircraft_id")
                .IsRequired();

            entity.Property(e => e.Registration)
                .HasColumnName("registration")
                .HasMaxLength(20);

            entity.Property(e => e.Nickname)
                .HasColumnName("nickname")
                .HasMaxLength(100);

            entity.Property(e => e.Condition)
                .HasColumnName("condition")
                .HasDefaultValue(100);

            entity.Property(e => e.TotalFlightMinutes)
                .HasColumnName("total_flight_minutes")
                .HasDefaultValue(0);

            entity.Property(e => e.TotalCycles)
                .HasColumnName("total_cycles")
                .HasDefaultValue(0);

            entity.Property(e => e.HoursSinceLastInspection)
                .HasColumnName("hours_since_last_inspection")
                .HasDefaultValue(0);

            entity.Property(e => e.CurrentLocationIcao)
                .HasColumnName("current_location_icao")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(e => e.LastMovedAt)
                .HasColumnName("last_moved_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.IsInMaintenance)
                .HasColumnName("is_in_maintenance")
                .HasDefaultValue(false);

            entity.Property(e => e.IsInUse)
                .HasColumnName("is_in_use")
                .HasDefaultValue(false);

            entity.Property(e => e.IsListedForRental)
                .HasColumnName("is_listed_for_rental")
                .HasDefaultValue(false);

            entity.Property(e => e.IsListedForSale)
                .HasColumnName("is_listed_for_sale")
                .HasDefaultValue(false);

            entity.Property(e => e.PurchasePrice)
                .HasColumnName("purchase_price")
                .HasPrecision(18, 2);

            entity.Property(e => e.PurchasedAt)
                .HasColumnName("purchased_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.WasPurchasedNew)
                .HasColumnName("was_purchased_new")
                .HasDefaultValue(false);

            entity.Property(e => e.HasWarranty)
                .HasColumnName("has_warranty")
                .HasDefaultValue(false);

            entity.Property(e => e.WarrantyExpiresAt)
                .HasColumnName("warranty_expires_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.HasInsurance)
                .HasColumnName("has_insurance")
                .HasDefaultValue(false);

            entity.Property(e => e.InsuranceExpiresAt)
                .HasColumnName("insurance_expires_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.InsurancePremium)
                .HasColumnName("insurance_premium")
                .HasPrecision(18, 2);

            // Ignore computed properties
            entity.Ignore(e => e.IsAirworthy);
            entity.Ignore(e => e.TotalFlightHours);
            entity.Ignore(e => e.InspectionDue);

            // Relationships
            entity.HasOne(e => e.World)
                .WithMany()
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.PlayerWorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Aircraft)
                .WithMany()
                .HasForeignKey(e => e.AircraftId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.PlayerWorldId);
            entity.HasIndex(e => e.AircraftId);
            entity.HasIndex(e => e.CurrentLocationIcao);
            entity.HasIndex(e => new { e.WorldId, e.PlayerWorldId });
        });
    }

    private static void ConfigureAircraftComponent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AircraftComponent>(entity =>
        {
            entity.ToTable("aircraft_components");

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.OwnedAircraftId)
                .HasColumnName("owned_aircraft_id")
                .IsRequired();

            entity.Property(e => e.ComponentType)
                .HasColumnName("component_type")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.SerialNumber)
                .HasColumnName("serial_number")
                .HasMaxLength(50);

            entity.Property(e => e.Manufacturer)
                .HasColumnName("manufacturer")
                .HasMaxLength(100);

            entity.Property(e => e.Model)
                .HasColumnName("model")
                .HasMaxLength(100);

            entity.Property(e => e.Condition)
                .HasColumnName("condition")
                .HasDefaultValue(100);

            entity.Property(e => e.OperatingMinutes)
                .HasColumnName("operating_minutes")
                .HasDefaultValue(0);

            entity.Property(e => e.Cycles)
                .HasColumnName("cycles")
                .HasDefaultValue(0);

            entity.Property(e => e.TimeSinceOverhaul)
                .HasColumnName("time_since_overhaul")
                .HasDefaultValue(0);

            entity.Property(e => e.TimeBetweenOverhaul)
                .HasColumnName("time_between_overhaul");

            entity.Property(e => e.IsLifeLimited)
                .HasColumnName("is_life_limited")
                .HasDefaultValue(false);

            entity.Property(e => e.LifeLimitMinutes)
                .HasColumnName("life_limit_minutes");

            entity.Property(e => e.LifeLimitCycles)
                .HasColumnName("life_limit_cycles");

            entity.Property(e => e.InstalledAt)
                .HasColumnName("installed_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.AircraftHoursAtInstall)
                .HasColumnName("aircraft_hours_at_install")
                .HasDefaultValue(0);

            entity.Property(e => e.IsServiceable)
                .HasColumnName("is_serviceable")
                .HasDefaultValue(true);

            entity.Property(e => e.Notes)
                .HasColumnName("notes")
                .HasMaxLength(1000);

            // Ignore computed properties
            entity.Ignore(e => e.OperatingHours);
            entity.Ignore(e => e.TimeSinceOverhaulHours);
            entity.Ignore(e => e.TboPercentUsed);
            entity.Ignore(e => e.IsTboApproaching);
            entity.Ignore(e => e.IsTboExceeded);
            entity.Ignore(e => e.LifePercentUsed);
            entity.Ignore(e => e.NeedsAttention);

            // Relationships
            entity.HasOne(e => e.World)
                .WithMany()
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.OwnedAircraft)
                .WithMany(a => a.Components)
                .HasForeignKey(e => e.OwnedAircraftId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.OwnedAircraftId);
            entity.HasIndex(e => new { e.OwnedAircraftId, e.ComponentType });
        });
    }

    private static void ConfigureAircraftModification(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AircraftModification>(entity =>
        {
            entity.ToTable("aircraft_modifications");

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.OwnedAircraftId)
                .HasColumnName("owned_aircraft_id")
                .IsRequired();

            entity.Property(e => e.ModificationType)
                .HasColumnName("modification_type")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            entity.Property(e => e.InstalledAt)
                .HasColumnName("installed_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.InstallationCost)
                .HasColumnName("installation_cost")
                .HasPrecision(18, 2);

            entity.Property(e => e.InstalledAtAirport)
                .HasColumnName("installed_at_airport")
                .HasMaxLength(10);

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.IsRemovable)
                .HasColumnName("is_removable")
                .HasDefaultValue(true);

            entity.Property(e => e.RemovalCost)
                .HasColumnName("removal_cost")
                .HasPrecision(18, 2);

            entity.Property(e => e.CargoCapacityChangeLbs)
                .HasColumnName("cargo_capacity_change_lbs");

            entity.Property(e => e.PassengerCapacityChange)
                .HasColumnName("passenger_capacity_change");

            entity.Property(e => e.RangeChangeNm)
                .HasColumnName("range_change_nm");

            entity.Property(e => e.CruiseSpeedChangeKts)
                .HasColumnName("cruise_speed_change_kts");

            entity.Property(e => e.FuelConsumptionMultiplier)
                .HasColumnName("fuel_consumption_multiplier")
                .HasPrecision(5, 2)
                .HasDefaultValue(1.0m);

            entity.Property(e => e.TakeoffDistanceMultiplier)
                .HasColumnName("takeoff_distance_multiplier")
                .HasPrecision(5, 2)
                .HasDefaultValue(1.0m);

            entity.Property(e => e.LandingDistanceMultiplier)
                .HasColumnName("landing_distance_multiplier")
                .HasPrecision(5, 2)
                .HasDefaultValue(1.0m);

            entity.Property(e => e.EmptyWeightChangeLbs)
                .HasColumnName("empty_weight_change_lbs");

            entity.Property(e => e.MaxGrossWeightChangeLbs)
                .HasColumnName("max_gross_weight_change_lbs");

            entity.Property(e => e.EnablesWaterOperations)
                .HasColumnName("enables_water_operations")
                .HasDefaultValue(false);

            entity.Property(e => e.EnablesSnowOperations)
                .HasColumnName("enables_snow_operations")
                .HasDefaultValue(false);

            entity.Property(e => e.EnablesStolOperations)
                .HasColumnName("enables_stol_operations")
                .HasDefaultValue(false);

            entity.Property(e => e.EnablesIfrOperations)
                .HasColumnName("enables_ifr_operations")
                .HasDefaultValue(false);

            entity.Property(e => e.EnablesNightOperations)
                .HasColumnName("enables_night_operations")
                .HasDefaultValue(false);

            entity.Property(e => e.EnablesKnownIcing)
                .HasColumnName("enables_known_icing")
                .HasDefaultValue(false);

            entity.Property(e => e.AdditionalMaintenanceCostPerHour)
                .HasColumnName("additional_maintenance_cost_per_hour")
                .HasPrecision(18, 2);

            entity.Property(e => e.SpecialInspectionIntervalMinutes)
                .HasColumnName("special_inspection_interval_minutes");

            entity.Property(e => e.Notes)
                .HasColumnName("notes")
                .HasMaxLength(1000);

            // Relationships
            entity.HasOne(e => e.World)
                .WithMany()
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.OwnedAircraft)
                .WithMany(a => a.Modifications)
                .HasForeignKey(e => e.OwnedAircraftId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.OwnedAircraftId);
            entity.HasIndex(e => new { e.OwnedAircraftId, e.ModificationType });
        });
    }

    private static void ConfigureMaintenanceLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaintenanceLog>(entity =>
        {
            entity.ToTable("maintenance_logs");

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.OwnedAircraftId)
                .HasColumnName("owned_aircraft_id")
                .IsRequired();

            entity.Property(e => e.AircraftComponentId)
                .HasColumnName("aircraft_component_id");

            entity.Property(e => e.MaintenanceType)
                .HasColumnName("maintenance_type")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(2000);

            entity.Property(e => e.StartedAt)
                .HasColumnName("started_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.CompletedAt)
                .HasColumnName("completed_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.EstimatedDurationHours)
                .HasColumnName("estimated_duration_hours");

            entity.Property(e => e.ActualDurationHours)
                .HasColumnName("actual_duration_hours");

            entity.Property(e => e.AircraftFlightMinutesAtService)
                .HasColumnName("aircraft_flight_minutes_at_service");

            entity.Property(e => e.AircraftCyclesAtService)
                .HasColumnName("aircraft_cycles_at_service");

            entity.Property(e => e.PerformedAtAirport)
                .HasColumnName("performed_at_airport")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(e => e.FacilityName)
                .HasColumnName("facility_name")
                .HasMaxLength(200);

            entity.Property(e => e.LaborCost)
                .HasColumnName("labor_cost")
                .HasPrecision(18, 2);

            entity.Property(e => e.PartsCost)
                .HasColumnName("parts_cost")
                .HasPrecision(18, 2);

            entity.Property(e => e.CoveredByWarranty)
                .HasColumnName("covered_by_warranty")
                .HasDefaultValue(false);

            entity.Property(e => e.CoveredByInsurance)
                .HasColumnName("covered_by_insurance")
                .HasDefaultValue(false);

            entity.Property(e => e.IsCompleted)
                .HasColumnName("is_completed")
                .HasDefaultValue(false);

            entity.Property(e => e.ConditionImprovement)
                .HasColumnName("condition_improvement");

            entity.Property(e => e.ResultingCondition)
                .HasColumnName("resulting_condition");

            entity.Property(e => e.PartsReplaced)
                .HasColumnName("parts_replaced")
                .HasMaxLength(1000);

            entity.Property(e => e.SquawksFound)
                .HasColumnName("squawks_found")
                .HasMaxLength(2000);

            entity.Property(e => e.DeferredItems)
                .HasColumnName("deferred_items")
                .HasMaxLength(1000);

            entity.Property(e => e.AirworthinessDirectiveNumber)
                .HasColumnName("airworthiness_directive_number")
                .HasMaxLength(50);

            entity.Property(e => e.ServiceBulletinNumber)
                .HasColumnName("service_bulletin_number")
                .HasMaxLength(50);

            entity.Property(e => e.MechanicName)
                .HasColumnName("mechanic_name")
                .HasMaxLength(100);

            entity.Property(e => e.MechanicLicense)
                .HasColumnName("mechanic_license")
                .HasMaxLength(50);

            entity.Property(e => e.InspectorName)
                .HasColumnName("inspector_name")
                .HasMaxLength(100);

            entity.Property(e => e.Notes)
                .HasColumnName("notes")
                .HasMaxLength(2000);

            entity.Property(e => e.NextServiceDueMinutes)
                .HasColumnName("next_service_due_minutes");

            entity.Property(e => e.NextServiceDueDate)
                .HasColumnName("next_service_due_date")
                .HasColumnType("timestamptz");

            // Ignore computed property
            entity.Ignore(e => e.TotalCost);

            // Relationships
            entity.HasOne(e => e.World)
                .WithMany()
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.OwnedAircraft)
                .WithMany(a => a.MaintenanceLogs)
                .HasForeignKey(e => e.OwnedAircraftId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AircraftComponent)
                .WithMany()
                .HasForeignKey(e => e.AircraftComponentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.OwnedAircraftId);
            entity.HasIndex(e => e.AircraftComponentId);
            entity.HasIndex(e => e.MaintenanceType);
            entity.HasIndex(e => new { e.OwnedAircraftId, e.StartedAt });
        });
    }

    private static void ConfigureAircraftDealer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AircraftDealer>(entity =>
        {
            entity.ToTable("aircraft_dealers");

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.AirportIcao)
                .HasColumnName("airport_icao")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(e => e.DealerType)
                .HasColumnName("dealer_type")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            entity.Property(e => e.MinInventory)
                .HasColumnName("min_inventory")
                .HasDefaultValue(5);

            entity.Property(e => e.MaxInventory)
                .HasColumnName("max_inventory")
                .HasDefaultValue(20);

            entity.Property(e => e.InventoryRefreshDays)
                .HasColumnName("inventory_refresh_days")
                .HasDefaultValue(7);

            entity.Property(e => e.PriceMultiplier)
                .HasColumnName("price_multiplier")
                .HasPrecision(5, 2)
                .HasDefaultValue(1.0m);

            entity.Property(e => e.OffersFinancing)
                .HasColumnName("offers_financing")
                .HasDefaultValue(false);

            entity.Property(e => e.FinancingDownPaymentPercent)
                .HasColumnName("financing_down_payment_percent")
                .HasPrecision(5, 2);

            entity.Property(e => e.FinancingInterestRate)
                .HasColumnName("financing_interest_rate")
                .HasPrecision(5, 4);

            entity.Property(e => e.MinCondition)
                .HasColumnName("min_condition")
                .HasDefaultValue(60);

            entity.Property(e => e.MaxCondition)
                .HasColumnName("max_condition")
                .HasDefaultValue(100);

            entity.Property(e => e.MinHours)
                .HasColumnName("min_hours")
                .HasDefaultValue(0);

            entity.Property(e => e.MaxHours)
                .HasColumnName("max_hours")
                .HasDefaultValue(10000);

            entity.Property(e => e.ReputationScore)
                .HasColumnName("reputation_score")
                .HasDefaultValue(3.0);

            entity.Property(e => e.TotalSales)
                .HasColumnName("total_sales")
                .HasDefaultValue(0);

            entity.Property(e => e.LastInventoryRefresh)
                .HasColumnName("last_inventory_refresh")
                .HasColumnType("timestamptz");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            // Relationships
            entity.HasOne(e => e.World)
                .WithMany()
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.AirportIcao);
            entity.HasIndex(e => new { e.WorldId, e.AirportIcao });
            entity.HasIndex(e => e.DealerType);
        });
    }

    private static void ConfigureDealerInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DealerInventory>(entity =>
        {
            entity.ToTable("dealer_inventory");

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.DealerId)
                .HasColumnName("dealer_id")
                .IsRequired();

            entity.Property(e => e.AircraftId)
                .HasColumnName("aircraft_id")
                .IsRequired();

            entity.Property(e => e.Registration)
                .HasColumnName("registration")
                .HasMaxLength(20);

            entity.Property(e => e.Condition)
                .HasColumnName("condition")
                .HasDefaultValue(100);

            entity.Property(e => e.TotalFlightMinutes)
                .HasColumnName("total_flight_minutes")
                .HasDefaultValue(0);

            entity.Property(e => e.TotalCycles)
                .HasColumnName("total_cycles")
                .HasDefaultValue(0);

            entity.Property(e => e.BasePrice)
                .HasColumnName("base_price")
                .HasPrecision(18, 2);

            entity.Property(e => e.ListPrice)
                .HasColumnName("list_price")
                .HasPrecision(18, 2);

            entity.Property(e => e.IsNew)
                .HasColumnName("is_new")
                .HasDefaultValue(false);

            entity.Property(e => e.HasWarranty)
                .HasColumnName("has_warranty")
                .HasDefaultValue(false);

            entity.Property(e => e.WarrantyMonths)
                .HasColumnName("warranty_months");

            entity.Property(e => e.AvionicsPackage)
                .HasColumnName("avionics_package")
                .HasMaxLength(500);

            entity.Property(e => e.IncludedModifications)
                .HasColumnName("included_modifications")
                .HasMaxLength(1000);

            entity.Property(e => e.Notes)
                .HasColumnName("notes")
                .HasMaxLength(2000);

            entity.Property(e => e.ListedAt)
                .HasColumnName("listed_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.IsSold)
                .HasColumnName("is_sold")
                .HasDefaultValue(false);

            entity.Property(e => e.SoldAt)
                .HasColumnName("sold_at")
                .HasColumnType("timestamptz");

            // Ignore computed properties
            entity.Ignore(e => e.IsActive);
            entity.Ignore(e => e.TotalFlightHours);
            entity.Ignore(e => e.DiscountPercent);

            // Relationships
            entity.HasOne(e => e.World)
                .WithMany()
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Dealer)
                .WithMany(d => d.Inventory)
                .HasForeignKey(e => e.DealerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Aircraft)
                .WithMany()
                .HasForeignKey(e => e.AircraftId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.AircraftId);
            entity.HasIndex(e => e.IsSold);
            entity.HasIndex(e => new { e.DealerId, e.IsSold });
        });
    }

    private static void ConfigureAircraftPurchase(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AircraftPurchase>(entity =>
        {
            entity.ToTable("aircraft_purchases");

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.PlayerWorldId)
                .HasColumnName("player_world_id")
                .IsRequired();

            entity.Property(e => e.OwnedAircraftId)
                .HasColumnName("owned_aircraft_id")
                .IsRequired();

            entity.Property(e => e.DealerId)
                .HasColumnName("dealer_id");

            entity.Property(e => e.DealerInventoryId)
                .HasColumnName("dealer_inventory_id");

            entity.Property(e => e.SellerPlayerWorldId)
                .HasColumnName("seller_player_world_id");

            entity.Property(e => e.PurchasePrice)
                .HasColumnName("purchase_price")
                .HasPrecision(18, 2);

            entity.Property(e => e.DownPayment)
                .HasColumnName("down_payment")
                .HasPrecision(18, 2);

            entity.Property(e => e.TradeInValue)
                .HasColumnName("trade_in_value")
                .HasPrecision(18, 2);

            entity.Property(e => e.TradeInAircraftId)
                .HasColumnName("trade_in_aircraft_id");

            entity.Property(e => e.IsFinanced)
                .HasColumnName("is_financed")
                .HasDefaultValue(false);

            entity.Property(e => e.LoanId)
                .HasColumnName("loan_id");

            entity.Property(e => e.FinancingInterestRate)
                .HasColumnName("financing_interest_rate")
                .HasPrecision(5, 4);

            entity.Property(e => e.FinancingTermMonths)
                .HasColumnName("financing_term_months");

            entity.Property(e => e.MonthlyPayment)
                .HasColumnName("monthly_payment")
                .HasPrecision(18, 2);

            entity.Property(e => e.PurchaseLocationIcao)
                .HasColumnName("purchase_location_icao")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(e => e.PurchasedAt)
                .HasColumnName("purchased_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.ConditionAtPurchase)
                .HasColumnName("condition_at_purchase");

            entity.Property(e => e.FlightMinutesAtPurchase)
                .HasColumnName("flight_minutes_at_purchase");

            entity.Property(e => e.IncludedWarranty)
                .HasColumnName("included_warranty")
                .HasDefaultValue(false);

            entity.Property(e => e.WarrantyMonths)
                .HasColumnName("warranty_months");

            // Ignore computed properties
            entity.Ignore(e => e.NetAmount);
            entity.Ignore(e => e.AmountFinanced);

            // Relationships
            entity.HasOne(e => e.World)
                .WithMany()
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PlayerWorld)
                .WithMany()
                .HasForeignKey(e => e.PlayerWorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.OwnedAircraft)
                .WithMany()
                .HasForeignKey(e => e.OwnedAircraftId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Dealer)
                .WithMany()
                .HasForeignKey(e => e.DealerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.DealerInventory)
                .WithMany()
                .HasForeignKey(e => e.DealerInventoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.SellerPlayerWorld)
                .WithMany()
                .HasForeignKey(e => e.SellerPlayerWorldId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TradeInAircraft)
                .WithMany()
                .HasForeignKey(e => e.TradeInAircraftId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.PlayerWorldId);
            entity.HasIndex(e => e.OwnedAircraftId);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.PurchasedAt);
            entity.HasIndex(e => new { e.PlayerWorldId, e.PurchasedAt });
        });
    }
}
