using Microsoft.EntityFrameworkCore;
using PilotLife.Database.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure database connection
// Check if custom connection should be used
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
}

app.UseHttpsRedirection();
app.UseCors("TauriApp");
app.MapControllers();

app.Run();
