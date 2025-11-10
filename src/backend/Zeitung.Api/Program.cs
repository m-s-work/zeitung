using Zeitung.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components
builder.AddServiceDefaults();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zeitung API v1");
    });
}

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

// Map API endpoints
app.MapFeedEndpoints();
app.MapArticleEndpoints();
app.MapTagEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
