using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PilotLife.API.Services;
using PilotLife.API.Services.Jobs;
using PilotLife.API.Services.Licenses;
using PilotLife.API.Services.Maintenance;
using PilotLife.API.Services.Reputation;
using PilotLife.API.Services.Skills;
using PilotLife.Application.Authorization;
using PilotLife.Application.FlightTracking;
using PilotLife.Application.Jobs;
using PilotLife.Application.Maintenance;
using PilotLife.Application.Marketplace;
using PilotLife.Application.Reputation;
using PilotLife.Application.Skills;
using PilotLife.Database.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure database connection
var useCustomConnection = builder.Configuration.GetValue<bool>("Database:UseCustomConnection");
var connectionString = useCustomConnection
    ? builder.Configuration.GetValue<string>("Database:CustomConnectionString")
    : builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

builder.Services.AddDbContext<PilotLifeDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure JWT
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("Jwt").Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<IJwtService, JwtService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add CORS for Tauri app
builder.Services.AddCors(options =>
{
    options.AddPolicy("TauriApp", policy =>
    {
        policy.WithOrigins("http://localhost:1420", "tauri://localhost")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Register services
builder.Services.AddSingleton<AirportImportService>();
builder.Services.AddScoped<IFlightTrackingService, FlightTrackingService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<DatabaseSeeder>();

// Marketplace services
builder.Services.Configure<MarketplaceConfiguration>(
    builder.Configuration.GetSection(MarketplaceConfiguration.SectionName));
builder.Services.AddScoped<IMarketplaceGenerator, MarketplaceGenerator>();
builder.Services.AddHostedService<MarketplacePopulationService>();

// Job generation services
builder.Services.Configure<JobGenerationConfiguration>(
    builder.Configuration.GetSection(JobGenerationConfiguration.SectionName));
builder.Services.AddScoped<IJobGenerator, JobGenerationService>();
builder.Services.AddHostedService<JobGenerationBackgroundService>();

// Maintenance services
builder.Services.Configure<MaintenanceConfiguration>(
    builder.Configuration.GetSection(MaintenanceConfiguration.SectionName));
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddHostedService<MaintenanceCompletionService>();

// License services
builder.Services.AddScoped<LicenseService>();
builder.Services.AddScoped<ExamService>();

// Reputation services
builder.Services.Configure<ReputationConfiguration>(
    builder.Configuration.GetSection(ReputationConfiguration.SectionName));
builder.Services.AddScoped<IReputationService, ReputationService>();

// Skills services
builder.Services.Configure<SkillsConfiguration>(
    builder.Configuration.GetSection(SkillsConfiguration.SectionName));
builder.Services.AddScoped<ISkillsService, SkillsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Auto-migrate database in development
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PilotLifeDbContext>();
    await dbContext.Database.MigrateAsync();

    // Import airports from CSV if not already imported
    var airportImportService = app.Services.GetRequiredService<AirportImportService>();
    await airportImportService.ImportAirportsAsync();

    // Seed default worlds and roles
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.UseHttpsRedirection();
app.UseCors("TauriApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
