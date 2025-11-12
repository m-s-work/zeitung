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

## Interactive API Reference

For detailed API endpoint documentation, see the [API Reference](/api/reference) page which provides an interactive explorer generated from the OpenAPI specification.

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
