# Zeitung Backend

ASP.NET Core Web API with .NET Aspire 9.5.2 for the Zeitung RSS Feed Reader.

## Structure

- **Zeitung.Api** - Web API project
- **Zeitung.AppHost** - Aspire orchestration host
- **Zeitung.ServiceDefaults** - Shared service configurations (telemetry, health checks, etc.)

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

## Testing

```bash
dotnet test Zeitung.sln
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

Run the Docker container:
```bash
docker run -p 8080:8080 zeitung-backend
```
