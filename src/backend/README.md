# Zeitung Backend API

ASP.NET Core Web API for the Zeitung RSS Feed Reader.

## Building

```bash
dotnet build
```

## Running

```bash
dotnet run
```

## Testing

```bash
dotnet test
```

## Docker

Build the Docker image:
```bash
docker build -t zeitung-backend .
```

Run the Docker container:
```bash
docker run -p 8080:8080 zeitung-backend
```
