# Zeitung Backend

ASP.NET Core Web API with .NET Aspire 9.5.2 for the Zeitung RSS Feed Reader.

## Structure

- **Zeitung.Api** - Web API project
- **Zeitung.Worker** - Background worker service for RSS feed ingestion
- **Zeitung.AppHost** - Aspire orchestration host
- **Zeitung.ServiceDefaults** - Shared service configurations (telemetry, health checks, etc.)
- **Zeitung.Worker.Tests** - Unit tests for the worker service
- **Zeitung.AppHost.Tests** - Integration tests for the AppHost

## Building

```bash
dotnet restore Zeitung.sln
dotnet build Zeitung.sln
```

## Running

Run with Aspire AppHost (recommended):
```bash
dotnet run --project Zeitung.AppHost/Zeitung.AppHost.csproj
```

Or run API directly:
```bash
dotnet run --project Zeitung.Api/Zeitung.Api.csproj
```

Or run Worker directly:
```bash
dotnet run --project Zeitung.Worker/Zeitung.Worker.csproj
```

## Testing

```bash
dotnet test Zeitung.sln
```

Run only worker tests:
```bash
dotnet test Zeitung.Worker.Tests/Zeitung.Worker.Tests.csproj
```

## RSS Feed Worker

The worker service ingests RSS feeds in the background and tags articles using configurable strategies.

### Configuration

Configure feeds and tagging strategy in `appsettings.json`:

```json
{
  "TaggingStrategy": "FeedBased",
  "IngestIntervalMinutes": 5,
  "RssFeeds": [
    {
      "Name": "BBC News",
      "Url": "http://feeds.bbci.co.uk/news/rss.xml",
      "Description": "BBC News - World"
    }
  ]
}
```

### Tagging Strategies

- **Mock** - Returns predictable mock tags for testing
- **FeedBased** - Extracts tags from feed metadata and article content
- **LLM** - Uses OpenRouter API for AI-powered tagging (requires API key)

Set `TaggingStrategy` to "Mock", "FeedBased", or "LLM" in configuration.

For LLM strategy, configure:
```json
{
  "OpenRouter": {
    "ApiKey": "your-api-key",
    "ApiUrl": "https://openrouter.ai/api/v1/chat/completions"
  }
}
```

## Generating OpenAPI JSON

The OpenAPI JSON for the frontend is generated using:
```bash
./generate-openapi.sh
```

This will create `../frontend/openapi.json` for use with nuxt-open-fetch.

## Docker

Build the Docker image for the API:
```bash
cd Zeitung.Api
docker build -t zeitung-backend .
```

Build the Docker image for the Worker:
```bash
cd Zeitung.Worker
docker build -t zeitung-worker .
```

Run the Docker container:
```bash
docker run -p 8080:8080 zeitung-backend
docker run zeitung-worker
```
