using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Zeitung.Api.DTOs;
using Zeitung.Api.Services;
using Zeitung.Worker.Models;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components
builder.AddServiceDefaults();

// Add Database Context
var connectionString = builder.Configuration.GetConnectionString("zeitungdb") ?? "Host=localhost;Database=zeitung;Username=zeitung;Password=zeitung";
builder.Services.AddDbContext<ZeitungDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "your-256-bit-secret-key-here-change-in-production-must-be-at-least-32-characters-long";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "zeitung-api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "zeitung-app";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IMagicLinkService, MagicLinkService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add health checks for external dependencies
builder.Services.AddHealthChecks()
    .AddNpgSql(
        name: "postgres",
        connectionStringFactory: sp => builder.Configuration.GetConnectionString("zeitungdb") ?? "Host=localhost;Database=zeitung;Username=zeitung;Password=zeitung",
        tags: ["ready", "db"])
    .AddRedis(
        name: "redis",
        connectionStringFactory: sp => builder.Configuration.GetConnectionString("redis") ?? "localhost:6379",
        tags: ["ready", "cache"])
    .AddElasticsearch(
        elasticsearchUri: builder.Configuration.GetConnectionString("elasticsearch") ?? "http://localhost:9200",
        name: "elasticsearch",
        tags: ["ready", "search"]);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Zeitung API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zeitung API v1");
    });
}

// Authentication Endpoints
app.MapPost("/auth/login", async (LoginRequest request, IMagicLinkService magicLinkService) =>
{
    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest(new { error = "Email is required" });
    }

    var token = await magicLinkService.GenerateMagicLinkAsync(request.Email);
    
    // In production, send this token via email
    // For now, return it in the response for testing
    return Results.Ok(new { message = "Magic link generated", token });
})
.WithName("RequestMagicLink")
.WithOpenApi();

app.MapPost("/auth/verify", async (VerifyMagicLinkRequest request, IMagicLinkService magicLinkService, IAuthService authService) =>
{
    if (string.IsNullOrWhiteSpace(request.Token))
    {
        return Results.BadRequest(new { error = "Token is required" });
    }

    var (isValid, email) = await magicLinkService.ValidateMagicLinkAsync(request.Token);
    
    if (!isValid || email == null)
    {
        return Results.Unauthorized();
    }

    var (accessToken, refreshToken, expiresAt) = await authService.AuthenticateAsync(email);
    
    return Results.Ok(new AuthResponse(accessToken, refreshToken, expiresAt));
})
.WithName("VerifyMagicLink")
.WithOpenApi();

app.MapPost("/auth/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
{
    if (string.IsNullOrWhiteSpace(request.RefreshToken))
    {
        return Results.BadRequest(new { error = "Refresh token is required" });
    }

    var (isValid, accessToken, refreshToken, expiresAt) = await authService.RefreshTokenAsync(request.RefreshToken);
    
    if (!isValid || accessToken == null || refreshToken == null || expiresAt == null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new AuthResponse(accessToken, refreshToken, expiresAt.Value));
})
.WithName("RefreshToken")
.WithOpenApi();

app.MapPost("/auth/revoke", async (RefreshTokenRequest request, IAuthService authService) =>
{
    if (string.IsNullOrWhiteSpace(request.RefreshToken))
    {
        return Results.BadRequest(new { error = "Refresh token is required" });
    }

    await authService.RevokeRefreshTokenAsync(request.RefreshToken);
    
    return Results.Ok(new { message = "Token revoked successfully" });
})
.WithName("RevokeRefreshToken")
.WithOpenApi();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
