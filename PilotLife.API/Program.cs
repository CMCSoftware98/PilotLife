using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PilotLife.API.Services;
using PilotLife.Application.FlightTracking;
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
}

app.UseHttpsRedirection();
app.UseCors("TauriApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
