# API Overview

The Zeitung API is a RESTful API built with ASP.NET Core that provides endpoints for managing RSS feeds, articles, tags, and user preferences.

## Base URL

- **Development**: `http://localhost:8080`
- **Production**: TBD

## API Documentation

Interactive API documentation is available via Swagger:
- **Swagger UI**: `http://localhost:8080/swagger`
- **OpenAPI Spec**: `http://localhost:8080/swagger/v1/swagger.json`

## Authentication

Most endpoints require authentication using JWT bearer tokens. See the [Authentication Guide](/api/authentication) for details.

```http
Authorization: Bearer <your-jwt-token>
```

## Response Format

All API responses follow a consistent format:

### Success Response

```json
{
  "data": { ... },
  "message": "Success"
}
```

### Error Response

```json
{
  "error": "Error message",
  "statusCode": 400
}
```

## Pagination

List endpoints support pagination:

**Query Parameters:**
- `page` (default: 1): Page number
- `pageSize` (default: 20, max: 100): Items per page

**Response:**
```json
{
  "items": [...],
  "totalCount": 500,
  "page": 1,
  "pageSize": 20,
  "totalPages": 25
}
```

## Status Codes

The API uses standard HTTP status codes:

- `200 OK`: Successful request
- `201 Created`: Resource created successfully
- `204 No Content`: Successful request with no response body
- `400 Bad Request`: Invalid request parameters
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

## Rate Limiting

Currently, there is no rate limiting. This will be added in a future update.

## CORS

CORS is configured to allow requests from:
- Development: `http://localhost:3000`
- Production: Your frontend domain

## API Endpoints

### Authentication
- `POST /auth/login` - Request magic link
- `POST /auth/verify` - Verify magic link
- `POST /auth/refresh` - Refresh access token
- `POST /auth/revoke` - Revoke refresh token

### Feeds
- `GET /api/feeds` - List feeds
- `POST /api/feeds` - Add feed
- `DELETE /api/feeds/{id}` - Remove feed
- `POST /api/feeds/{id}/promote` - Promote feed to global
- `GET /api/feeds/recommendations` - Get feed recommendations

### Articles
- `GET /api/articles` - List articles
- `GET /api/articles/{id}` - Get article details
- `POST /api/articles/{id}/vote` - Vote on article
- `POST /api/articles/{id}/like` - Like article
- `POST /api/articles/{id}/click` - Track article click

### Tags
- `GET /api/tags` - List all tags
- `GET /api/tags/user` - Get user tag preferences
- `POST /api/tags/interest` - Mark tags as interesting
- `POST /api/tags/ignore` - Mark tags to ignore
- `GET /api/tags/summary` - Get tag summary with decay

### Users
- `GET /api/users/current` - Get current user
- `GET /api/users/sync` - Get sync state

## Examples

### Using cURL

```bash
# Get articles with authentication
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:8080/api/articles?page=1&pageSize=20

# Add a feed
curl -X POST \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"url":"https://example.com/rss","name":"Example Feed"}' \
  http://localhost:8080/api/feeds
```

### Using JavaScript

```javascript
// Fetch articles
const response = await fetch('http://localhost:8080/api/articles', {
  headers: {
    'Authorization': `Bearer ${accessToken}`
  }
});
const data = await response.json();
```

### Using the Generated Client

If you're using the Nuxt frontend, a type-safe client is generated from the OpenAPI schema:

```typescript
// Automatically typed!
const articles = await $api('/api/articles', {
  query: { page: 1, pageSize: 20 }
});
```

## Versioning

The API is currently unversioned (v1). Future versions will be indicated in the URL path or Accept header.

## SDKs and Clients

- **TypeScript/JavaScript**: Auto-generated from OpenAPI spec (included in frontend)
- **C#**: Refit client (planned)
- **Python**: httpx client (planned)

## Support

For API issues or questions:
- Open an issue on [GitHub](https://github.com/m-s-work/zeitung/issues)
- Check the [API Documentation files](https://github.com/m-s-work/zeitung/tree/main/src/backend/Zeitung.Api) in the repository
