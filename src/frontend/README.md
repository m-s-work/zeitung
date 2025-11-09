# Zeitung Frontend

Nuxt 4 frontend for the Zeitung RSS Feed Reader with automatic API client generation.

## Features

- Nuxt 4 with TypeScript
- Automatic API client generation from OpenAPI spec using nuxt-open-fetch
- Type-safe API calls

## Setup

Install dependencies:
```bash
npm install
```

## Development

Start the development server:
```bash
npm run dev
```

The API client will be auto-generated from `openapi.json` when available.

## Build

Build for production:
```bash
npm run build
```

## Testing

Run tests:
```bash
npm test
```

## API Integration

The frontend uses `nuxt-open-fetch` to generate type-safe API clients from the backend's OpenAPI specification.

To regenerate the API client after backend changes:
1. Generate the OpenAPI spec from the backend: `cd ../backend && ./generate-openapi.sh`
2. The OpenAPI spec will be saved to `openapi.json`
3. Restart the dev server to regenerate the API client

## Docker

Build the Docker image:
```bash
docker build -t zeitung-frontend .
```

Run the Docker container:
```bash
docker run -p 3000:3000 zeitung-frontend
```
