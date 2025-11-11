# Zeitung Frontend

Nuxt 4 frontend for the Zeitung RSS Feed Reader with automatic API client generation.

## Features

- Nuxt 4 with TypeScript
- Automatic API client generation from OpenAPI spec using nuxt-open-fetch
- Type-safe API calls
- Modern, responsive UI using @nuxt/ui
- Article feed with like/dislike and voting
- Feed management (add, subscribe, promote)
- Tag preferences management
- Reading time tracking with 5-star rating
- Mobile-responsive design

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

### Unit Tests

Run unit tests:
```bash
npm test
```

### E2E Tests

Install Playwright browsers (one-time setup):
```bash
npm run test:e2e:install
```

Run E2E tests:
```bash
npm run test:e2e
```

**For detailed E2E testing documentation, including CI/CD setup and browser configuration, see [tests/E2E_TESTING.md](tests/E2E_TESTING.md).**

## Project Structure

```
src/frontend/
├── components/          # Reusable Vue components
│   ├── ArticleCard.vue # Article display with actions
│   ├── FeedCard.vue    # Feed display with management actions
│   └── RatingModal.vue # 5-star rating modal
├── composables/         # Vue composables for state management
│   ├── useReadingTracker.ts # Reading time tracking
│   └── useUser.ts      # User state management
├── pages/              # Nuxt pages (auto-routed)
│   ├── index.vue       # Main article feed
│   ├── feeds.vue       # Feed management
│   └── tags.vue        # Tag preferences
├── types/              # TypeScript type definitions
│   └── index.ts
├── openapi.json        # OpenAPI spec from backend
└── nuxt.config.ts      # Nuxt configuration

```

## API Integration

The frontend uses `nuxt-open-fetch` to generate type-safe API clients from the backend's OpenAPI specification.

To regenerate the API client after backend changes:
1. Generate the OpenAPI spec from the backend: `cd ../backend && ./generate-openapi.sh`
2. The OpenAPI spec will be saved to `openapi.json`
3. Restart the dev server to regenerate the API client

### Usage Example

```typescript
// Using the auto-generated API client
const { data, error } = await useZeitungApi('/api/articles', {
  query: {
    userId: userId.value,
    page: 1,
    pageSize: 20,
  },
})
```

## Docker

Build the Docker image:
```bash
docker build -t zeitung-frontend .
```

Run the Docker container:
```bash
docker run -p 3000:3000 zeitung-frontend
```
