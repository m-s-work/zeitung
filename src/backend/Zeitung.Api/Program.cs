using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Zeitung.Api.Services;
using Zeitung.Core.Context;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components
builder.AddServiceDefaults();

// Add database context
// Check if we should use in-memory database (for testing)
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase") ||
                           Environment.GetEnvironmentVariable("UseInMemoryDatabase") == "true";

if (!useInMemoryDatabase)
{
    var connectionString = builder.Configuration.GetConnectionString("zeitungdb")
        ?? "Host=localhost;Database=zeitung;Username=zeitung;Password=zeitung";
    builder.Services.AddDbContext<ZeitungDbContext>(options =>
        options.UseNpgsql(connectionString));
        
    // Add health checks for external dependencies
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            name: "postgres",
            connectionStringFactory: sp => connectionString,
            tags: new[] { "ready", "db" })
        .AddRedis(
            name: "redis",
            connectionStringFactory: sp => builder.Configuration.GetConnectionString("redis") ?? "localhost:6379",
            tags: new[] { "ready", "cache" })
        .AddElasticsearch(
            elasticsearchUri: builder.Configuration.GetConnectionString("elasticsearch") ?? "http://localhost:9200",
            name: "elasticsearch",
            tags: new[] { "ready", "search" });
}
else
{
    // In-memory database for testing - DbContext will be configured by test setup
    // Don't register DbContext here to avoid provider conflict
    builder.Services.AddHealthChecks();
}

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

// Add authentication services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IMagicLinkService, MagicLinkService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Zeitung API", Version = "v1" });
});

// Add MVC controllers so MapControllers works
builder.Services.AddControllers();

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

// Use attribute routed controllers
app.MapControllers();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
