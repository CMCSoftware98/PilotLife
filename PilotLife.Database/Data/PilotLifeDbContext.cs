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

    // Cargo entities
    public DbSet<CargoType> CargoTypes => Set<CargoType>();

    // License entities
    public DbSet<LicenseType> LicenseTypes => Set<LicenseType>();
    public DbSet<UserLicense> UserLicenses => Set<UserLicense>();
    public DbSet<LicenseExam> LicenseExams => Set<LicenseExam>();
    public DbSet<ExamManeuver> ExamManeuvers => Set<ExamManeuver>();
    public DbSet<ExamCheckpoint> ExamCheckpoints => Set<ExamCheckpoint>();
    public DbSet<ExamLanding> ExamLandings => Set<ExamLanding>();
    public DbSet<ExamViolation> ExamViolations => Set<ExamViolation>();

    // Banking entities
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanPayment> LoanPayments => Set<LoanPayment>();
    public DbSet<CreditScoreEvent> CreditScoreEvents => Set<CreditScoreEvent>();

    // Reputation and skills entities
    public DbSet<ReputationEvent> ReputationEvents => Set<ReputationEvent>();
    public DbSet<PlayerSkill> PlayerSkills => Set<PlayerSkill>();
    public DbSet<SkillXpEvent> SkillXpEvents => Set<SkillXpEvent>();

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
        ConfigureBaseEntity<CargoType>(modelBuilder);
        ConfigureBaseEntity<LicenseType>(modelBuilder);
        ConfigureBaseEntity<UserLicense>(modelBuilder);
        ConfigureBaseEntity<LicenseExam>(modelBuilder);
        ConfigureBaseEntity<ExamManeuver>(modelBuilder);
        ConfigureBaseEntity<ExamCheckpoint>(modelBuilder);
        ConfigureBaseEntity<ExamLanding>(modelBuilder);
        ConfigureBaseEntity<ExamViolation>(modelBuilder);
        ConfigureBaseEntity<Bank>(modelBuilder);
        ConfigureBaseEntity<Loan>(modelBuilder);
        ConfigureBaseEntity<LoanPayment>(modelBuilder);
        ConfigureBaseEntity<CreditScoreEvent>(modelBuilder);
        ConfigureBaseEntity<ReputationEvent>(modelBuilder);
        ConfigureBaseEntity<PlayerSkill>(modelBuilder);
        ConfigureBaseEntity<SkillXpEvent>(modelBuilder);

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
        ConfigureCargoType(modelBuilder);
        ConfigureLicenseType(modelBuilder);
        ConfigureUserLicense(modelBuilder);
        ConfigureLicenseExam(modelBuilder);
        ConfigureExamManeuver(modelBuilder);
        ConfigureExamCheckpoint(modelBuilder);
        ConfigureExamLanding(modelBuilder);
        ConfigureExamViolation(modelBuilder);
        ConfigureBank(modelBuilder);
        ConfigureLoan(modelBuilder);
        ConfigureLoanPayment(modelBuilder);
        ConfigureCreditScoreEvent(modelBuilder);
        ConfigureReputationEvent(modelBuilder);
        ConfigurePlayerSkill(modelBuilder);
        ConfigureSkillXpEvent(modelBuilder);
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

            // World
            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            // Route
            entity.Property(e => e.DepartureAirportId)
                .HasColumnName("departure_airport_id")
                .IsRequired();

            entity.Property(e => e.DepartureIcao)
                .HasColumnName("departure_icao")
                .HasMaxLength(10);

            entity.Property(e => e.ArrivalAirportId)
                .HasColumnName("arrival_airport_id")
                .IsRequired();

            entity.Property(e => e.ArrivalIcao)
                .HasColumnName("arrival_icao")
                .HasMaxLength(10);

            entity.Property(e => e.DistanceNm)
                .HasColumnName("distance_nm");

            entity.Property(e => e.DistanceCategory)
                .HasColumnName("distance_category")
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.RouteDifficulty)
                .HasColumnName("route_difficulty")
                .HasConversion<string>()
                .HasMaxLength(20);

            // Job type and status
            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.Urgency)
                .HasColumnName("urgency")
                .HasConversion<string>()
                .HasMaxLength(20);

            // Cargo details
            entity.Property(e => e.CargoTypeId)
                .HasColumnName("cargo_type_id");

            entity.Property(e => e.CargoType)
                .HasColumnName("cargo_type")
                .HasMaxLength(100);

            entity.Property(e => e.WeightLbs)
                .HasColumnName("weight_lbs");

            entity.Property(e => e.VolumeCuFt)
                .HasColumnName("volume_cu_ft")
                .HasPrecision(12, 2);

            // Passenger details
            entity.Property(e => e.PassengerCount)
                .HasColumnName("passenger_count");

            entity.Property(e => e.PassengerClass)
                .HasColumnName("passenger_class")
                .HasConversion<string>()
                .HasMaxLength(20);

            // Requirements
            entity.Property(e => e.RequiredAircraftType)
                .HasColumnName("required_aircraft_type")
                .HasMaxLength(100);

            entity.Property(e => e.MinCrewCount)
                .HasColumnName("min_crew_count");

            entity.Property(e => e.RequiresSpecialCertification)
                .HasColumnName("requires_special_certification")
                .HasDefaultValue(false);

            entity.Property(e => e.RequiredCertification)
                .HasColumnName("required_certification")
                .HasMaxLength(100);

            entity.Property(e => e.RiskLevel)
                .HasColumnName("risk_level")
                .HasDefaultValue(1);

            // Payout
            entity.Property(e => e.BasePayout)
                .HasColumnName("base_payout")
                .HasPrecision(18, 2);

            entity.Property(e => e.Payout)
                .HasColumnName("payout")
                .HasPrecision(18, 2);

            entity.Property(e => e.BonusPayout)
                .HasColumnName("bonus_payout")
                .HasPrecision(18, 2);

            entity.Property(e => e.ActualPayout)
                .HasColumnName("actual_payout")
                .HasPrecision(18, 2);

            // Timing
            entity.Property(e => e.EstimatedFlightTimeMinutes)
                .HasColumnName("estimated_flight_time_minutes");

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.AcceptedAt)
                .HasColumnName("accepted_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.StartedAt)
                .HasColumnName("started_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.CompletedAt)
                .HasColumnName("completed_at")
                .HasColumnType("timestamptz");

            // Assignment
            entity.Property(e => e.AssignedToUserId)
                .HasColumnName("assigned_to_user_id");

            entity.Property(e => e.AssignedToPlayerWorldId)
                .HasColumnName("assigned_to_player_world_id");

            // Completion
            entity.Property(e => e.IsCompleted)
                .HasColumnName("is_completed")
                .HasDefaultValue(false);

            entity.Property(e => e.IsFailed)
                .HasColumnName("is_failed")
                .HasDefaultValue(false);

            entity.Property(e => e.FailureReason)
                .HasColumnName("failure_reason")
                .HasMaxLength(500);

            // Display
            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            // Ignore computed properties
            entity.Ignore(e => e.IsAvailable);
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.TimeRemaining);

            // Relationships
            entity.HasOne(e => e.World)
                .WithMany()
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.DepartureAirport)
                .WithMany()
                .HasForeignKey(e => e.DepartureAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ArrivalAirport)
                .WithMany()
                .HasForeignKey(e => e.ArrivalAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CargoTypeRef)
                .WithMany()
                .HasForeignKey(e => e.CargoTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AssignedToUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AssignedToPlayerWorld)
                .WithMany()
                .HasForeignKey(e => e.AssignedToPlayerWorldId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.DepartureAirportId);
            entity.HasIndex(e => e.ArrivalAirportId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => e.AssignedToUserId);
            entity.HasIndex(e => e.AssignedToPlayerWorldId);
            entity.HasIndex(e => new { e.WorldId, e.Status });
            entity.HasIndex(e => new { e.WorldId, e.DepartureIcao, e.Status });
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

    private static void ConfigureCargoType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CargoType>(entity =>
        {
            entity.ToTable("cargo_types");

            entity.Property(e => e.Category)
                .HasColumnName("category")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Subcategory)
                .HasColumnName("subcategory")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            entity.Property(e => e.BaseRatePerLb)
                .HasColumnName("base_rate_per_lb")
                .HasPrecision(10, 4);

            entity.Property(e => e.MinWeightLbs)
                .HasColumnName("min_weight_lbs")
                .HasDefaultValue(100);

            entity.Property(e => e.MaxWeightLbs)
                .HasColumnName("max_weight_lbs")
                .HasDefaultValue(5000);

            entity.Property(e => e.DensityFactor)
                .HasColumnName("density_factor")
                .HasPrecision(6, 4)
                .HasDefaultValue(0.1m);

            entity.Property(e => e.RequiresSpecialHandling)
                .HasColumnName("requires_special_handling")
                .HasDefaultValue(false);

            entity.Property(e => e.SpecialHandlingType)
                .HasColumnName("special_handling_type")
                .HasMaxLength(50);

            entity.Property(e => e.IsTemperatureSensitive)
                .HasColumnName("is_temperature_sensitive")
                .HasDefaultValue(false);

            entity.Property(e => e.IsTimeCritical)
                .HasColumnName("is_time_critical")
                .HasDefaultValue(false);

            entity.Property(e => e.IsIllegal)
                .HasColumnName("is_illegal")
                .HasDefaultValue(false);

            entity.Property(e => e.IllegalRiskLevel)
                .HasColumnName("illegal_risk_level");

            entity.Property(e => e.PayoutMultiplier)
                .HasColumnName("payout_multiplier")
                .HasPrecision(5, 2)
                .HasDefaultValue(1.0m);

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            // Ignore computed properties
            entity.Ignore(e => e.EffectiveRatePerLb);

            // Indexes
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Subcategory);
            entity.HasIndex(e => new { e.Category, e.Subcategory });
            entity.HasIndex(e => e.IsActive);
        });
    }

    private static void ConfigureLicenseType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LicenseType>(entity =>
        {
            entity.ToTable("license_types");

            entity.Property(e => e.Code)
                .HasColumnName("code")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(e => e.Code).IsUnique();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            entity.Property(e => e.Category)
                .HasColumnName("category")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.BaseExamCost)
                .HasColumnName("base_exam_cost")
                .HasPrecision(18, 2);

            entity.Property(e => e.ExamDurationMinutes)
                .HasColumnName("exam_duration_minutes");

            entity.Property(e => e.PassingScore)
                .HasColumnName("passing_score")
                .HasDefaultValue(70);

            entity.Property(e => e.RequiredAircraftCategory)
                .HasColumnName("required_aircraft_category")
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(e => e.RequiredAircraftType)
                .HasColumnName("required_aircraft_type")
                .HasMaxLength(50);

            entity.Property(e => e.ValidityGameDays)
                .HasColumnName("validity_game_days");

            entity.Property(e => e.BaseRenewalCost)
                .HasColumnName("base_renewal_cost")
                .HasPrecision(18, 2);

            entity.Property(e => e.PrerequisiteLicensesJson)
                .HasColumnName("prerequisite_licenses_json")
                .HasMaxLength(500);

            entity.Property(e => e.DisplayOrder)
                .HasColumnName("display_order")
                .HasDefaultValue(0);

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            // Indexes
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.DisplayOrder);
        });
    }

    private static void ConfigureUserLicense(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserLicense>(entity =>
        {
            entity.ToTable("user_licenses");

            entity.Property(e => e.PlayerWorldId)
                .HasColumnName("player_world_id")
                .IsRequired();

            entity.Property(e => e.LicenseTypeId)
                .HasColumnName("license_type_id")
                .IsRequired();

            entity.Property(e => e.EarnedAt)
                .HasColumnName("earned_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.LastRenewedAt)
                .HasColumnName("last_renewed_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.RenewalCount)
                .HasColumnName("renewal_count")
                .HasDefaultValue(0);

            entity.Property(e => e.IsValid)
                .HasColumnName("is_valid")
                .HasDefaultValue(true);

            entity.Property(e => e.IsRevoked)
                .HasColumnName("is_revoked")
                .HasDefaultValue(false);

            entity.Property(e => e.RevocationReason)
                .HasColumnName("revocation_reason")
                .HasMaxLength(500);

            entity.Property(e => e.RevokedAt)
                .HasColumnName("revoked_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.ExamScore)
                .HasColumnName("exam_score");

            entity.Property(e => e.ExamAttempts)
                .HasColumnName("exam_attempts")
                .HasDefaultValue(1);

            entity.Property(e => e.TotalPaid)
                .HasColumnName("total_paid")
                .HasPrecision(18, 2);

            entity.Property(e => e.PassedExamId)
                .HasColumnName("passed_exam_id");

            // Relationships
            entity.HasOne(e => e.PlayerWorld)
                .WithMany()
                .HasForeignKey(e => e.PlayerWorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LicenseType)
                .WithMany(lt => lt.UserLicenses)
                .HasForeignKey(e => e.LicenseTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PassedExam)
                .WithOne(le => le.EarnedLicense)
                .HasForeignKey<UserLicense>(e => e.PassedExamId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.PlayerWorldId);
            entity.HasIndex(e => e.LicenseTypeId);
            entity.HasIndex(e => e.IsValid);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => new { e.PlayerWorldId, e.LicenseTypeId }).IsUnique();
        });
    }

    private static void ConfigureLicenseExam(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LicenseExam>(entity =>
        {
            entity.ToTable("license_exams");

            entity.Property(e => e.PlayerWorldId)
                .HasColumnName("player_world_id")
                .IsRequired();

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.LicenseTypeId)
                .HasColumnName("license_type_id")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.ScheduledAt)
                .HasColumnName("scheduled_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.StartedAt)
                .HasColumnName("started_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.CompletedAt)
                .HasColumnName("completed_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.TimeLimitMinutes)
                .HasColumnName("time_limit_minutes");

            entity.Property(e => e.RequiredAircraftCategory)
                .HasColumnName("required_aircraft_category")
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(e => e.RequiredAircraftType)
                .HasColumnName("required_aircraft_type")
                .HasMaxLength(50);

            entity.Property(e => e.DepartureIcao)
                .HasColumnName("departure_icao")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(e => e.RouteJson)
                .HasColumnName("route_json")
                .HasMaxLength(4000);

            entity.Property(e => e.AssignedAltitudeFt)
                .HasColumnName("assigned_altitude_ft");

            entity.Property(e => e.Score)
                .HasColumnName("score")
                .HasDefaultValue(0);

            entity.Property(e => e.PassingScore)
                .HasColumnName("passing_score")
                .HasDefaultValue(70);

            entity.Property(e => e.FailureReason)
                .HasColumnName("failure_reason")
                .HasMaxLength(500);

            entity.Property(e => e.ExaminerNotes)
                .HasColumnName("examiner_notes")
                .HasMaxLength(2000);

            entity.Property(e => e.AttemptNumber)
                .HasColumnName("attempt_number")
                .HasDefaultValue(1);

            entity.Property(e => e.FeePaid)
                .HasColumnName("fee_paid")
                .HasPrecision(18, 2);

            entity.Property(e => e.EligibleForRetakeAt)
                .HasColumnName("eligible_for_retake_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.FlightTimeMinutes)
                .HasColumnName("flight_time_minutes");

            entity.Property(e => e.DistanceFlownNm)
                .HasColumnName("distance_flown_nm");

            entity.Property(e => e.AircraftUsed)
                .HasColumnName("aircraft_used")
                .HasMaxLength(50);

            // Relationships
            entity.HasOne(e => e.PlayerWorld)
                .WithMany()
                .HasForeignKey(e => e.PlayerWorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.World)
                .WithMany()
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LicenseType)
                .WithMany(lt => lt.Exams)
                .HasForeignKey(e => e.LicenseTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.PlayerWorldId);
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.LicenseTypeId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.PlayerWorldId, e.Status });
            entity.HasIndex(e => new { e.PlayerWorldId, e.LicenseTypeId, e.Status });
        });
    }

    private static void ConfigureExamManeuver(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExamManeuver>(entity =>
        {
            entity.ToTable("exam_maneuvers");

            entity.Property(e => e.ExamId)
                .HasColumnName("exam_id")
                .IsRequired();

            entity.Property(e => e.ManeuverType)
                .HasColumnName("maneuver_type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Order)
                .HasColumnName("order");

            entity.Property(e => e.IsRequired)
                .HasColumnName("is_required")
                .HasDefaultValue(true);

            entity.Property(e => e.MaxPoints)
                .HasColumnName("max_points");

            entity.Property(e => e.PointsAwarded)
                .HasColumnName("points_awarded")
                .HasDefaultValue(0);

            entity.Property(e => e.Result)
                .HasColumnName("result")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ManeuverResult.NotAttempted);

            entity.Property(e => e.AltitudeToleranceFt)
                .HasColumnName("altitude_tolerance_ft");

            entity.Property(e => e.HeadingToleranceDeg)
                .HasColumnName("heading_tolerance_deg");

            entity.Property(e => e.SpeedToleranceKts)
                .HasColumnName("speed_tolerance_kts");

            entity.Property(e => e.AltitudeDeviationFt)
                .HasColumnName("altitude_deviation_ft");

            entity.Property(e => e.HeadingDeviationDeg)
                .HasColumnName("heading_deviation_deg");

            entity.Property(e => e.SpeedDeviationKts)
                .HasColumnName("speed_deviation_kts");

            entity.Property(e => e.StartedAt)
                .HasColumnName("started_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.CompletedAt)
                .HasColumnName("completed_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.Notes)
                .HasColumnName("notes")
                .HasMaxLength(500);

            // Relationships
            entity.HasOne(e => e.Exam)
                .WithMany(ex => ex.Maneuvers)
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.ExamId);
            entity.HasIndex(e => new { e.ExamId, e.Order });
        });
    }

    private static void ConfigureExamCheckpoint(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExamCheckpoint>(entity =>
        {
            entity.ToTable("exam_checkpoints");

            entity.Property(e => e.ExamId)
                .HasColumnName("exam_id")
                .IsRequired();

            entity.Property(e => e.Order)
                .HasColumnName("order");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Latitude)
                .HasColumnName("latitude");

            entity.Property(e => e.Longitude)
                .HasColumnName("longitude");

            entity.Property(e => e.RequiredAltitudeFt)
                .HasColumnName("required_altitude_ft");

            entity.Property(e => e.RadiusNm)
                .HasColumnName("radius_nm")
                .HasDefaultValue(1.0);

            entity.Property(e => e.WasReached)
                .HasColumnName("was_reached")
                .HasDefaultValue(false);

            entity.Property(e => e.ReachedAt)
                .HasColumnName("reached_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.AltitudeAtReach)
                .HasColumnName("altitude_at_reach");

            entity.Property(e => e.SpeedAtReachKts)
                .HasColumnName("speed_at_reach_kts");

            entity.Property(e => e.PointsAwarded)
                .HasColumnName("points_awarded")
                .HasDefaultValue(0);

            entity.Property(e => e.MaxPoints)
                .HasColumnName("max_points");

            // Relationships
            entity.HasOne(e => e.Exam)
                .WithMany(ex => ex.Checkpoints)
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.ExamId);
            entity.HasIndex(e => new { e.ExamId, e.Order });
        });
    }

    private static void ConfigureExamLanding(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExamLanding>(entity =>
        {
            entity.ToTable("exam_landings");

            entity.Property(e => e.ExamId)
                .HasColumnName("exam_id")
                .IsRequired();

            entity.Property(e => e.Order)
                .HasColumnName("order");

            entity.Property(e => e.AirportIcao)
                .HasColumnName("airport_icao")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.VerticalSpeedFpm)
                .HasColumnName("vertical_speed_fpm");

            entity.Property(e => e.CenterlineDeviationFt)
                .HasColumnName("centerline_deviation_ft");

            entity.Property(e => e.TouchdownZoneDistanceFt)
                .HasColumnName("touchdown_zone_distance_ft");

            entity.Property(e => e.GroundSpeedKts)
                .HasColumnName("ground_speed_kts");

            entity.Property(e => e.PitchDeg)
                .HasColumnName("pitch_deg");

            entity.Property(e => e.BankDeg)
                .HasColumnName("bank_deg");

            entity.Property(e => e.GearDown)
                .HasColumnName("gear_down")
                .HasDefaultValue(true);

            entity.Property(e => e.RunwayUsed)
                .HasColumnName("runway_used")
                .HasMaxLength(10);

            entity.Property(e => e.PointsAwarded)
                .HasColumnName("points_awarded")
                .HasDefaultValue(0);

            entity.Property(e => e.MaxPoints)
                .HasColumnName("max_points");

            entity.Property(e => e.LandedAt)
                .HasColumnName("landed_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.Notes)
                .HasColumnName("notes")
                .HasMaxLength(500);

            // Relationships
            entity.HasOne(e => e.Exam)
                .WithMany(ex => ex.Landings)
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.ExamId);
            entity.HasIndex(e => new { e.ExamId, e.Order });
        });
    }

    private static void ConfigureExamViolation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExamViolation>(entity =>
        {
            entity.ToTable("exam_violations");

            entity.Property(e => e.ExamId)
                .HasColumnName("exam_id")
                .IsRequired();

            entity.Property(e => e.OccurredAt)
                .HasColumnName("occurred_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Value)
                .HasColumnName("value");

            entity.Property(e => e.Threshold)
                .HasColumnName("threshold");

            entity.Property(e => e.PointsDeducted)
                .HasColumnName("points_deducted");

            entity.Property(e => e.CausedFailure)
                .HasColumnName("caused_failure")
                .HasDefaultValue(false);

            entity.Property(e => e.LatitudeAtViolation)
                .HasColumnName("latitude_at_violation");

            entity.Property(e => e.LongitudeAtViolation)
                .HasColumnName("longitude_at_violation");

            entity.Property(e => e.AltitudeAtViolation)
                .HasColumnName("altitude_at_violation");

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            // Relationships
            entity.HasOne(e => e.Exam)
                .WithMany(ex => ex.Violations)
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.ExamId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => new { e.ExamId, e.OccurredAt });
        });
    }

    private static void ConfigureBank(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bank>(entity =>
        {
            entity.ToTable("banks");

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            entity.Property(e => e.OffersStarterLoan)
                .HasColumnName("offers_starter_loan")
                .HasDefaultValue(false);

            entity.Property(e => e.StarterLoanMaxAmount)
                .HasColumnName("starter_loan_max_amount")
                .HasPrecision(18, 2)
                .HasDefaultValue(250000m);

            entity.Property(e => e.StarterLoanInterestRate)
                .HasColumnName("starter_loan_interest_rate")
                .HasPrecision(8, 6)
                .HasDefaultValue(0.015m);

            entity.Property(e => e.BaseInterestRate)
                .HasColumnName("base_interest_rate")
                .HasPrecision(8, 6)
                .HasDefaultValue(0.02m);

            entity.Property(e => e.MaxInterestRate)
                .HasColumnName("max_interest_rate")
                .HasPrecision(8, 6)
                .HasDefaultValue(0.08m);

            entity.Property(e => e.MinCreditScore)
                .HasColumnName("min_credit_score")
                .HasDefaultValue(500);

            entity.Property(e => e.MaxLoanToNetWorthRatio)
                .HasColumnName("max_loan_to_net_worth_ratio")
                .HasPrecision(5, 2)
                .HasDefaultValue(3.0m);

            entity.Property(e => e.MinDownPaymentPercent)
                .HasColumnName("min_down_payment_percent")
                .HasPrecision(5, 4)
                .HasDefaultValue(0.05m);

            entity.Property(e => e.MaxTermMonths)
                .HasColumnName("max_term_months")
                .HasDefaultValue(24);

            entity.Property(e => e.DaysToDefault)
                .HasColumnName("days_to_default")
                .HasDefaultValue(3);

            entity.Property(e => e.LatePaymentFeePercent)
                .HasColumnName("late_payment_fee_percent")
                .HasPrecision(5, 4)
                .HasDefaultValue(0.05m);

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
            entity.HasIndex(e => new { e.WorldId, e.IsActive });
        });
    }

    private static void ConfigureLoan(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("loans");

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.PlayerWorldId)
                .HasColumnName("player_world_id")
                .IsRequired();

            entity.Property(e => e.BankId)
                .HasColumnName("bank_id")
                .IsRequired();

            entity.Property(e => e.LoanType)
                .HasColumnName("loan_type")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.PrincipalAmount)
                .HasColumnName("principal_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.InterestRatePerMonth)
                .HasColumnName("interest_rate_per_month")
                .HasPrecision(8, 6)
                .IsRequired();

            entity.Property(e => e.TermMonths)
                .HasColumnName("term_months")
                .IsRequired();

            entity.Property(e => e.MonthlyPayment)
                .HasColumnName("monthly_payment")
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.TotalRepaymentAmount)
                .HasColumnName("total_repayment_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.RemainingPrincipal)
                .HasColumnName("remaining_principal")
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.AccruedInterest)
                .HasColumnName("accrued_interest")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.TotalPaid)
                .HasColumnName("total_paid")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.PaymentsMade)
                .HasColumnName("payments_made")
                .HasDefaultValue(0);

            entity.Property(e => e.PaymentsRemaining)
                .HasColumnName("payments_remaining")
                .IsRequired();

            entity.Property(e => e.LatePaymentCount)
                .HasColumnName("late_payment_count")
                .HasDefaultValue(0);

            entity.Property(e => e.MissedPaymentCount)
                .HasColumnName("missed_payment_count")
                .HasDefaultValue(0);

            entity.Property(e => e.NextPaymentDue)
                .HasColumnName("next_payment_due")
                .HasColumnType("timestamptz");

            entity.Property(e => e.ApprovedAt)
                .HasColumnName("approved_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.DisbursedAt)
                .HasColumnName("disbursed_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.PaidOffAt)
                .HasColumnName("paid_off_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.DefaultedAt)
                .HasColumnName("defaulted_at")
                .HasColumnType("timestamptz");

            entity.Property(e => e.CollateralAircraftId)
                .HasColumnName("collateral_aircraft_id");

            entity.Property(e => e.Purpose)
                .HasColumnName("purpose")
                .HasMaxLength(500);

            entity.Property(e => e.Notes)
                .HasColumnName("notes")
                .HasMaxLength(2000);

            // Relationships
            entity.HasOne(e => e.World)
                .WithMany()
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PlayerWorld)
                .WithMany(pw => pw.Loans)
                .HasForeignKey(e => e.PlayerWorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Bank)
                .WithMany(b => b.Loans)
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CollateralAircraft)
                .WithMany()
                .HasForeignKey(e => e.CollateralAircraftId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.PlayerWorldId);
            entity.HasIndex(e => e.BankId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LoanType);
            entity.HasIndex(e => new { e.PlayerWorldId, e.Status });
            entity.HasIndex(e => new { e.WorldId, e.Status });
            entity.HasIndex(e => e.NextPaymentDue);
        });
    }

    private static void ConfigureLoanPayment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoanPayment>(entity =>
        {
            entity.ToTable("loan_payments");

            entity.Property(e => e.LoanId)
                .HasColumnName("loan_id")
                .IsRequired();

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.PaymentNumber)
                .HasColumnName("payment_number")
                .IsRequired();

            entity.Property(e => e.Amount)
                .HasColumnName("amount")
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.PrincipalPortion)
                .HasColumnName("principal_portion")
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.InterestPortion)
                .HasColumnName("interest_portion")
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.LateFee)
                .HasColumnName("late_fee")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.RemainingBalanceAfter)
                .HasColumnName("remaining_balance_after")
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.DueDate)
                .HasColumnName("due_date")
                .HasColumnType("timestamptz")
                .IsRequired();

            entity.Property(e => e.PaidAt)
                .HasColumnName("paid_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            entity.Property(e => e.IsLate)
                .HasColumnName("is_late")
                .HasDefaultValue(false);

            entity.Property(e => e.Notes)
                .HasColumnName("notes")
                .HasMaxLength(500);

            // Ignore computed property
            entity.Ignore(e => e.DaysLate);

            // Relationships
            entity.HasOne(e => e.Loan)
                .WithMany(l => l.Payments)
                .HasForeignKey(e => e.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.World)
                .WithMany()
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.LoanId);
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => new { e.LoanId, e.PaymentNumber });
            entity.HasIndex(e => e.PaidAt);
        });
    }

    private static void ConfigureCreditScoreEvent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CreditScoreEvent>(entity =>
        {
            entity.ToTable("credit_score_events");

            entity.Property(e => e.PlayerWorldId)
                .HasColumnName("player_world_id")
                .IsRequired();

            entity.Property(e => e.WorldId)
                .HasColumnName("world_id")
                .IsRequired();

            entity.Property(e => e.EventType)
                .HasColumnName("event_type")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.ScoreBefore)
                .HasColumnName("score_before")
                .IsRequired();

            entity.Property(e => e.ScoreAfter)
                .HasColumnName("score_after")
                .IsRequired();

            entity.Property(e => e.ScoreChange)
                .HasColumnName("score_change")
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.RelatedLoanId)
                .HasColumnName("related_loan_id");

            entity.Property(e => e.RelatedJobId)
                .HasColumnName("related_job_id");

            // Relationships
            entity.HasOne(e => e.PlayerWorld)
                .WithMany(pw => pw.CreditScoreEvents)
                .HasForeignKey(e => e.PlayerWorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.World)
                .WithMany()
                .HasForeignKey(e => e.WorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RelatedLoan)
                .WithMany()
                .HasForeignKey(e => e.RelatedLoanId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.RelatedJob)
                .WithMany()
                .HasForeignKey(e => e.RelatedJobId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.PlayerWorldId);
            entity.HasIndex(e => e.WorldId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => new { e.PlayerWorldId, e.CreatedAt });
            entity.HasIndex(e => e.RelatedLoanId);
        });
    }

    private static void ConfigureReputationEvent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReputationEvent>(entity =>
        {
            entity.ToTable("reputation_events");

            // Properties
            entity.Property(e => e.EventType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.PointChange)
                .HasPrecision(10, 4)
                .IsRequired();

            entity.Property(e => e.ResultingScore)
                .HasPrecision(10, 4)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.OccurredAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            entity.HasOne(e => e.PlayerWorld)
                .WithMany(pw => pw.ReputationEvents)
                .HasForeignKey(e => e.PlayerWorldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RelatedJob)
                .WithMany()
                .HasForeignKey(e => e.RelatedJobId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.RelatedFlight)
                .WithMany()
                .HasForeignKey(e => e.RelatedFlightId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.PlayerWorldId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.OccurredAt);
            entity.HasIndex(e => new { e.PlayerWorldId, e.OccurredAt });
        });
    }

    private static void ConfigurePlayerSkill(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerSkill>(entity =>
        {
            entity.ToTable("player_skills");

            // Properties
            entity.Property(e => e.SkillType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.CurrentXp)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(e => e.Level)
                .IsRequired()
                .HasDefaultValue(1);

            entity.Property(e => e.LastUpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            entity.HasOne(e => e.PlayerWorld)
                .WithMany(pw => pw.Skills)
                .HasForeignKey(e => e.PlayerWorldId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.PlayerWorldId);
            entity.HasIndex(e => e.SkillType);
            entity.HasIndex(e => new { e.PlayerWorldId, e.SkillType })
                .IsUnique();
        });
    }

    private static void ConfigureSkillXpEvent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SkillXpEvent>(entity =>
        {
            entity.ToTable("skill_xp_events");

            // Properties
            entity.Property(e => e.XpGained)
                .IsRequired();

            entity.Property(e => e.ResultingXp)
                .IsRequired();

            entity.Property(e => e.ResultingLevel)
                .IsRequired();

            entity.Property(e => e.CausedLevelUp)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.Source)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.OccurredAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            entity.HasOne(e => e.PlayerSkill)
                .WithMany(ps => ps.XpEvents)
                .HasForeignKey(e => e.PlayerSkillId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RelatedFlight)
                .WithMany()
                .HasForeignKey(e => e.RelatedFlightId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.RelatedJob)
                .WithMany()
                .HasForeignKey(e => e.RelatedJobId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.PlayerSkillId);
            entity.HasIndex(e => e.OccurredAt);
            entity.HasIndex(e => new { e.PlayerSkillId, e.OccurredAt });
        });
    }
}
