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

        ConfigureUser(modelBuilder);
        ConfigureAirport(modelBuilder);
        ConfigureJob(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
        ConfigureAircraft(modelBuilder);
        ConfigureAircraftRequest(modelBuilder);
        ConfigureTrackedFlight(modelBuilder);
        ConfigureFlightJob(modelBuilder);
        ConfigureFlightFinancials(modelBuilder);
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
}
