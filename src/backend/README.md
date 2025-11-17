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

The worker service ingests RSS feeds in the background, tags articles using configurable strategies, and persists them to PostgreSQL and Elasticsearch.

### Persistence

The worker:
- **Stores articles in PostgreSQL** - Articles, tags, and their relationships are persisted
- **Tracks tag co-occurrence** - Maintains counts of how often tags appear together for similarity calculations
- **Indexes articles in Elasticsearch** - Enables full-text search and recommendations

Database schema includes:
- `Articles` - Article content and metadata
- `Tags` - Unique tag names
- `ArticleTags` - Many-to-many relationship between articles and tags
- `TagCoOccurrences` - Tracks how often tag pairs appear together

### Database Migrations

Database migrations can be run on-demand using the `--migrate` or `-m` flag. This is useful for pre-deploy hooks in Helm or Argo:

```bash
dotnet run --project Zeitung.Worker/Zeitung.Worker.csproj -- --migrate
# or
dotnet run --project Zeitung.Worker/Zeitung.Worker.csproj -- -m
```

The flag can be combined with `--run-once` for one-time migration and ingestion:

```bash
dotnet run --project Zeitung.Worker/Zeitung.Worker.csproj -- --migrate --run-once
```

#### Creating New Migrations

**Important:** Migrations are stored in `Zeitung.Worker/Migrations` but use the `ZeitungDbContext` from `Zeitung.Core`. Both projects specify `MigrationsAssembly("Zeitung.Worker")` to ensure migrations are found.

To create a new migration after modifying the database models:

1. Ensure you have the Entity Framework Core tools installed:
   ```bash
   dotnet tool install --global dotnet-ef --version 9.0.0
   ```

2. Navigate to the Worker project directory:
   ```bash
   cd src/backend/Zeitung.Worker
   ```

3. Create the migration:
   ```bash
   dotnet ef migrations add <MigrationName> --context ZeitungDbContext
   ```

**You do NOT need PostgreSQL running** to create migrations. The design-time factory (`ZeitungDbContextFactory`) in `Zeitung.Worker/Context` provides the necessary configuration to generate migration code without a database connection.

Example:
```bash
cd src/backend/Zeitung.Worker
dotnet ef migrations add AddUserPreferences --context ZeitungDbContext
```

To remove the last migration (if not yet applied):
```bash
dotnet ef migrations remove --context ZeitungDbContext
```

To list all migrations:
```bash
dotnet ef migrations list --context ZeitungDbContext
```

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
- **LLM** - Uses OpenRouter API for AI-powered tagging with JSON format, probability scores, and automatic retry on format errors (requires API key)

Set `TaggingStrategy` to "Mock", "FeedBased", or "LLM" in configuration.

For LLM strategy, configure:
```json
{
  "OpenRouter": {
    "ApiKey": "your-api-key",
    "ApiUrl": "https://openrouter.ai/api/v1/chat/completions",
    "Model": "meta-llama/llama-3.1-8b-instruct:free",
    "IncludeExistingTags": true,
    "MinimumTagProbability": 0.7
  }
}
```

The LLM strategy features:
- **Polly-based retry logic** with exponential backoff (up to 3 attempts)
- **Structured JSON responses** with tags, probability scores, comments, and error fields
- **Configurable probability threshold** - only tags meeting the minimum probability are included (default: 0.7)
- **Existing tag awareness** (optional) - can pass existing tags to LLM to avoid creating duplicates
- **Tag creation guidelines** enforced:
  - Use singular forms (e.g., "technology" not "technologies")
  - Lowercase tags
  - Prefer existing tags when relevant
  - Be specific but not overly detailed
  - Avoid generic tags like "news" or "article"

### One-off Execution Mode

For K8s CronJob scenarios, run the worker once and exit:

```bash
dotnet run --project Zeitung.Worker/Zeitung.Worker.csproj -- --run-once
# or
dotnet run --project Zeitung.Worker/Zeitung.Worker.csproj -- -o
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

Run the Worker in one-off mode:
```bash
docker run zeitung-worker --run-once
```

Run the Docker container:
```bash
docker run -p 8080:8080 zeitung-backend
docker run zeitung-worker
```
