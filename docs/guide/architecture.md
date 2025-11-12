# Architecture

This document describes the technical architecture of Zeitung and how its components work together.

## System Overview

Zeitung follows a modern microservices architecture with clear separation of concerns:

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │
       v
┌─────────────┐     ┌──────────────┐     ┌──────────────┐
│   Nuxt 4    │────>│  Backend API │────>│ PostgreSQL   │
│  Frontend   │     │   (.NET 9)   │     │   Database   │
└─────────────┘     └──────┬───────┘     └──────────────┘
                           │
                           v
                    ┌──────────────┐
                    │    Redis     │
                    │    Cache     │
                    └──────────────┘
                           ^
                           │
                    ┌──────┴───────┐
                    │    Worker    │
                    │   Service    │
                    └──────┬───────┘
                           │
                           v
                    ┌──────────────┐
                    │  OpenRouter  │
                    │     LLM      │
                    └──────────────┘
```

## Components

### Frontend (Nuxt 4)

The frontend is a modern single-page application built with:

- **Framework**: Nuxt 4 with Vue 3
- **UI Library**: Nuxt UI for components
- **API Client**: nuxt-open-fetch for type-safe API calls
- **Testing**: Vitest for unit tests, Playwright for E2E tests

**Key Features:**
- Server-side rendering (SSR) for better SEO
- Type-safe API calls generated from OpenAPI schema
- Responsive design
- Real-time updates (planned)

### Backend API (.NET 9)

The backend is built with ASP.NET Core and provides RESTful APIs:

- **Framework**: ASP.NET Core 9 with minimal APIs
- **Hosting**: .NET Aspire for orchestration
- **Authentication**: JWT with magic links
- **Documentation**: OpenAPI/Swagger

**Key Responsibilities:**
- User authentication and authorization
- Feed management (CRUD operations)
- Article serving with pagination
- Tag management and preferences
- Recommendation engine
- Vote and interaction tracking

### Worker Service

A background service that handles async operations:

- **Framework**: .NET 9 Background Service
- **Scheduling**: Periodic feed fetching
- **AI Integration**: OpenRouter for article tagging

**Key Responsibilities:**
- Fetch RSS feeds on schedule
- Parse feed items
- Tag articles with LLM
- Calculate tag relationships
- Apply tag decay
- Update recommendation scores

### Database (PostgreSQL)

PostgreSQL provides reliable, ACID-compliant data storage:

**Main Tables:**
- `Users`: User accounts and roles
- `Feeds`: RSS feed definitions
- `UserFeeds`: User subscriptions
- `Articles`: Fetched articles
- `Tags`: Tag definitions with usage counts
- `ArticleTags`: Many-to-many with confidence scores
- `UserTags`: User preferences with decay
- `TagRelationships`: Co-occurrence tracking
- `Votes`: User votes on content
- `MagicLinks`: Authentication tokens
- `RefreshTokens`: Session management

### Cache (Redis)

Redis provides fast access to frequently accessed data:

**Cached Data:**
- User sessions
- Tag summaries
- Popular articles
- Recommendation scores
- Feed fetch schedules

### Search (Elasticsearch) - Planned

Future enhancement for advanced search capabilities:
- Full-text article search
- Tag similarity search
- Feed discovery
- Content recommendations

## Data Flow

### Feed Ingestion Flow

1. **Worker** fetches RSS feeds on schedule
2. **Parser** extracts article metadata
3. **LLM Tagger** sends article to OpenRouter
4. **Tag Processor** stores tags with confidence scores
5. **Relationship Calculator** updates tag co-occurrence
6. **Database** stores all data
7. **Cache** invalidates affected entries

### Recommendation Flow

1. **User** interacts with content (click, like, etc.)
2. **API** records interaction in `UserTags`
3. **Decay Engine** applies time-based decay
4. **Tag Summary** calculates weighted preferences
5. **Recommendation Engine** scores articles
6. **Cache** stores computed recommendations
7. **API** serves personalized feed

### Authentication Flow

1. **User** requests magic link with email
2. **API** generates token and stores in database
3. **Email Service** sends link (dev: returns token)
4. **User** clicks link with token
5. **API** verifies token and issues JWT
6. **Frontend** stores JWT and refresh token
7. **Subsequent requests** use JWT bearer token
8. **Token refresh** uses refresh token when JWT expires

## Tag System Architecture

The tag system is the core of Zeitung's intelligence:

### Tag Confidence

When articles are tagged:
- LLM provides confidence scores (0-1)
- Higher confidence = stronger association
- Multiple tags per article supported

### Tag Interactions

Four types of user-tag interactions:

1. **Explicit**: User marks tag as interesting (+5 points)
2. **Ignored**: User marks tag to avoid (-10 points)
3. **Clicked**: User clicks article with tag (+1 point)
4. **Liked**: User likes article with tag (+3 points)

### Tag Decay

Tags decay based on new interactions, not time:

```csharp
// Simplified decay formula
newScore = oldScore * Math.Exp(-decayRate * newInteractionCount / oldInteractionCount)
```

Benefits:
- Natural evolution of interests
- Tags with more history are more stable
- Recent interests are weighted appropriately
- No arbitrary time limits

### Tag Relationships

Tracks which tags appear together:

- Co-occurrence counting
- Similarity calculation
- Used for discovering related content
- Helps recommend new feeds

## Service Communication

### Aspire Orchestration

.NET Aspire orchestrates all services:

```csharp
// AppHost configuration
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var redis = builder.AddRedis("redis");

builder.AddProject<Api>("api")
    .WithReference(postgres)
    .WithReference(redis);

builder.AddProject<Worker>("worker")
    .WithReference(postgres)
    .WithReference(redis);
```

Benefits:
- Service discovery
- Configuration management
- Health monitoring
- Development dashboard

### API Contracts

Communication uses OpenAPI specifications:

1. Backend generates OpenAPI schema
2. Frontend generates TypeScript client
3. Type-safe API calls across the stack

## Deployment Architecture

### Development

```
Aspire Dashboard
├── Frontend (npm run dev)
├── API (dotnet run)
├── Worker (dotnet run)
├── PostgreSQL (Docker)
└── Redis (Docker)
```

### Production (Docker)

```
Docker Compose
├── Frontend Container
├── Backend Container
├── Worker Container
├── PostgreSQL Container
└── Redis Container
```

### Future: Kubernetes

Planning for cloud deployment:
- Horizontal pod autoscaling
- Load balancing
- Managed PostgreSQL (Azure/AWS)
- Managed Redis
- CDN for frontend assets

## Security Architecture

### Authentication

- Magic link passwordless auth
- JWT access tokens (15 min expiry)
- Refresh tokens (30 day expiry)
- Automatic token rotation
- Token revocation on logout

### Authorization

- Role-based access control (RBAC)
- User, Moderator, Admin roles
- API endpoint authorization
- Feed promotion permissions

### Data Protection

- Passwords not stored (passwordless)
- Tokens encrypted in database
- HTTPS only in production
- CORS configured properly
- SQL injection prevention (EF Core)

## Performance Considerations

### Backend

- Async/await throughout
- Connection pooling
- Query optimization
- Pagination for large datasets
- Minimal API for less overhead

### Frontend

- SSR for initial load
- Code splitting
- Lazy loading
- Image optimization
- Asset caching

### Database

- Proper indexing
- Query optimization
- Connection pooling
- Read replicas (future)

### Cache

- Redis for hot data
- Cache-aside pattern
- TTL-based expiration
- Cache invalidation strategy

## Monitoring and Observability

### Logging

- Structured logging with Serilog
- Log levels: Debug, Info, Warning, Error
- Centralized logging (future)

### Metrics

- Aspire telemetry
- Performance counters
- API latency tracking
- Database query times

### Tracing

- Distributed tracing with OpenTelemetry
- Request correlation IDs
- Service dependency mapping

## Scalability

### Current

- Single instance deployment
- Vertical scaling
- Database connection pooling

### Future

- Horizontal scaling
- Load balancing
- Database sharding
- CDN integration
- Caching layer expansion
- Message queue for async tasks
