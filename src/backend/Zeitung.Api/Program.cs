using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.MigrationsAssembly("Zeitung.Worker")));
        
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
        // is currently broken - https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2355
        //.AddElasticsearch(
        //    elasticsearchUri: builder.Configuration.GetConnectionString("elasticsearch") ?? "http://localhost:9200",
        //    name: "elasticsearch",
        //    tags: new[] { "ready", "search" })
        ;
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


// this maps health endpoints
//app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
// Map health endpoints with a detailed JSON response writer so clients can see which resources are failing.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponseAsync
});

app.MapHealthChecks("/alive", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live"),
    ResponseWriter = WriteHealthCheckResponseAsync
});

app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = WriteHealthCheckResponseAsync
});

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

static async Task WriteHealthCheckResponseAsync(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    var response = new
    {
        status = report.Status.ToString(), // "Healthy", "Unhealthy", "Degraded"
        totalDuration = report.TotalDuration.ToString(@"hh\:mm\:ss\.fffffff"),
        entries = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description,
            exception = e.Value.Exception?.Message,
            duration = e.Value.Duration.ToString(@"hh\:mm\:ss\.fffffff")
        }).ToArray()
    };

    await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
}

// Make Program class accessible for testing
public partial class Program { }
