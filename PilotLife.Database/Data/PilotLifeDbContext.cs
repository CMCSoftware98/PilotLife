using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Entities;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("uuidv7()")
                .ValueGeneratedOnAdd();

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

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(e => e.LastLoginAt)
                .HasColumnName("last_login_at");

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
        });

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

        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("jobs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("uuidv7()")
                .ValueGeneratedOnAdd();

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

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at");

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

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("uuidv7()")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Token)
                .HasColumnName("token")
                .IsRequired();

            entity.HasIndex(e => e.Token)
                .IsUnique();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

            entity.Property(e => e.RevokedAt)
                .HasColumnName("revoked_at");

            entity.Property(e => e.ReplacedByToken)
                .HasColumnName("replaced_by_token");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
        });
    }
}
