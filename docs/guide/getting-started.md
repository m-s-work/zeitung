# Getting Started

This guide will help you set up Zeitung on your local machine for development or testing.

## Prerequisites

Before you begin, ensure you have the following installed:

- [Docker](https://www.docker.com/) and Docker Compose
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 22](https://nodejs.org/) (LTS recommended)
- [PostgreSQL](https://www.postgresql.org/) (optional if using Docker)
- [Redis](https://redis.io/) (optional if using Docker)

## Quick Start with Docker Compose

The easiest way to get started is using Docker Compose:

```bash
# Clone the repository
git clone https://github.com/m-s-work/zeitung.git
cd zeitung

# Start all services
docker-compose up
```

This will start:
- Frontend on http://localhost:3000
- Backend API on http://localhost:8080
- PostgreSQL on localhost:5432
- Redis on localhost:6379

## Manual Setup

### 1. Clone the Repository

```bash
git clone https://github.com/m-s-work/zeitung.git
cd zeitung
```

### 2. Set Up the Backend

```bash
cd src/backend

# Restore dependencies
dotnet restore Zeitung.sln

# Build the solution
dotnet build Zeitung.sln --configuration Release

# Run tests
dotnet test Zeitung.sln

# Generate OpenAPI schema (required for frontend)
chmod +x generate-openapi.sh
./generate-openapi.sh
```

### 3. Configure the Database

Create a PostgreSQL database and update the connection string in `src/backend/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=zeitung;Username=your_user;Password=your_password"
  }
}
```

### 4. Set Up the Frontend

```bash
cd src/frontend

# Install dependencies
npm ci

# Run tests
npm test

# Build the frontend
npm run build
```

## Running in Development Mode

### Backend

```bash
cd src/backend

# Run with Aspire (recommended)
dotnet run --project Zeitung.AppHost

# Or run the API directly
dotnet run --project Zeitung.Api
```

The API will be available at http://localhost:8080

### Frontend

```bash
cd src/frontend

# Start development server
npm run dev
```

The frontend will be available at http://localhost:3000

### Worker Service

The worker service handles feed ingestion and article tagging:

```bash
cd src/backend
dotnet run --project Zeitung.Worker
```

## Configuration

### Authentication

Configure JWT settings in `src/backend/appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "your-256-bit-secret-key-change-in-production",
    "Issuer": "zeitung-api",
    "Audience": "zeitung-app",
    "AccessTokenExpirationMinutes": "15"
  }
}
```

### OpenRouter API

For AI tagging, you'll need an OpenRouter API key:

1. Sign up at [OpenRouter](https://openrouter.ai/)
2. Add your API key to environment variables or configuration

## Verification

To verify your setup:

1. **Backend**: Visit http://localhost:8080/swagger for the API documentation
2. **Frontend**: Visit http://localhost:3000 to see the UI
3. **Database**: Check that tables are created in PostgreSQL

## Next Steps

- Read the [Architecture Guide](/guide/architecture) to understand how Zeitung works
- Explore the [API Reference](/api/overview) for available endpoints
- Check the [Contributing Guide](/development/contributing) if you want to contribute

## Troubleshooting

### Port Already in Use

If you see port conflict errors, you can change the ports in:
- Backend: `src/backend/Zeitung.Api/Properties/launchSettings.json`
- Frontend: `src/frontend/nuxt.config.ts`

### Database Connection Issues

Ensure PostgreSQL is running and the connection string is correct. You can test the connection with:

```bash
psql -h localhost -U your_user -d zeitung
```

### Node Module Issues

If you encounter issues with node modules:

```bash
cd src/frontend
rm -rf node_modules package-lock.json
npm install
```

## Development Tips

- Use the Aspire dashboard to monitor services
- Enable hot reload for both backend and frontend
- Use the OpenAPI schema for type-safe API calls
- Check logs in the Aspire dashboard for debugging
