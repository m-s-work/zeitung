# Zeitung API Documentation

This document describes the backend API endpoints for the Zeitung RSS Feed Reader.

## Base URL

Development: `http://localhost:8080`
Production: TBD

## Authentication

Currently, endpoints use a `userId` query parameter. Full authentication will be added in a future update.

## API Endpoints

### Feed Management

#### GET /api/feeds
Get all feeds (global approved feeds + user's personal feeds).

**Query Parameters:**
- `userId` (optional): Filter feeds for specific user

**Response:**
```json
[
  {
    "id": 1,
    "url": "https://example.com/rss",
    "name": "Example Feed",
    "description": "Feed description",
    "isApproved": true,
    "isSubscribed": false,
    "createdAt": "2024-01-01T00:00:00Z",
    "lastFetchedAt": "2024-01-01T12:00:00Z"
  }
]
```

#### POST /api/feeds
Add a feed to the user's personal feed list.

**Query Parameters:**
- `userId` (required): User ID

**Request Body:**
```json
{
  "url": "https://example.com/rss",
  "name": "Example Feed",
  "description": "Feed description"
}
```

**Response:** `201 Created` with feed object

#### DELETE /api/feeds/{id}
Remove a feed from the user's personal feed list.

**Query Parameters:**
- `userId` (required): User ID

**Response:** `204 No Content`

#### POST /api/feeds/{id}/promote
Promote a feed to the global feed list (requires admin/mod role).

**Response:**
```json
{
  "message": "Feed promoted to global list"
}
```

#### GET /api/feeds/recommendations
Get personalized feed recommendations based on user's tag preferences.

**Query Parameters:**
- `userId` (required): User ID

**Response:**
```json
[
  {
    "id": 1,
    "url": "https://example.com/rss",
    "name": "Example Feed",
    "description": "Feed description",
    "relevanceScore": 0.85,
    "relevantTags": ["technology", "programming"]
  }
]
```

### Articles

#### GET /api/articles
Get paginated articles with optional filtering.

**Query Parameters:**
- `userId` (optional): User ID for vote information
- `feedId` (optional): Filter by feed
- `tagId` (optional): Filter by tag
- `page` (default: 1): Page number
- `pageSize` (default: 20): Items per page

**Response:**
```json
{
  "articles": [
    {
      "id": 1,
      "title": "Article Title",
      "link": "https://example.com/article",
      "description": "Article description",
      "publishedDate": "2024-01-01T00:00:00Z",
      "feedName": "Example Feed",
      "tags": ["technology", "programming"]
    }
  ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20
}
```

#### GET /api/articles/{id}
Get detailed article information.

**Query Parameters:**
- `userId` (optional): User ID for vote information

**Response:**
```json
{
  "id": 1,
  "title": "Article Title",
  "link": "https://example.com/article",
  "description": "Article description",
  "publishedDate": "2024-01-01T00:00:00Z",
  "feedName": "Example Feed",
  "tags": [
    {
      "id": 1,
      "name": "technology",
      "confidence": 0.95
    }
  ],
  "userVote": 1
}
```

#### POST /api/articles/{id}/vote
Vote on an article (1 for upvote, -1 for downvote).

**Query Parameters:**
- `userId` (required): User ID

**Request Body:**
```json
{
  "value": 1
}
```

**Response:**
```json
{
  "message": "Vote recorded"
}
```

#### POST /api/articles/{id}/like
Like an article and update user's tag preferences.

**Query Parameters:**
- `userId` (required): User ID

**Response:**
```json
{
  "message": "Article liked, tag preferences updated"
}
```

#### POST /api/articles/{id}/click
Track article click and update user's tag preferences.

**Query Parameters:**
- `userId` (required): User ID

**Response:**
```json
{
  "message": "Article click tracked"
}
```

### Tags

#### GET /api/tags
Get all tags ordered by usage count.

**Query Parameters:**
- `page` (default: 1): Page number
- `pageSize` (default: 50): Items per page

**Response:**
```json
{
  "tags": [
    {
      "id": 1,
      "name": "technology",
      "usageCount": 150
    }
  ],
  "totalCount": 500,
  "page": 1,
  "pageSize": 50
}
```

#### GET /api/tags/user
Get user's tag preferences grouped by interaction type.

**Query Parameters:**
- `userId` (required): User ID

**Response:**
```json
[
  {
    "tagId": 1,
    "tagName": "technology",
    "interactionType": "Explicit",
    "score": 5.5,
    "interactionCount": 3
  }
]
```

#### POST /api/tags/interest
Mark tags as explicitly interesting.

**Query Parameters:**
- `userId` (required): User ID

**Request Body:**
```json
{
  "tagIds": [1, 2, 3]
}
```

**Response:**
```json
{
  "message": "Tag interests updated"
}
```

#### POST /api/tags/ignore
Mark tags to explicitly ignore.

**Query Parameters:**
- `userId` (required): User ID

**Request Body:**
```json
{
  "tagIds": [4, 5]
}
```

**Response:**
```json
{
  "message": "Tags marked as ignored"
}
```

#### GET /api/tags/summary
Get tag summary with time-based decay applied.

**Query Parameters:**
- `userId` (required): User ID

**Response:**
```json
{
  "tagScores": {
    "technology": 8.5,
    "programming": 6.2,
    "science": 4.1
  },
  "lastUpdated": "2024-01-01T12:00:00Z"
}
```

### Users

#### GET /api/users/current
Get current user information.

**Query Parameters:**
- `userId` (required): User ID

**Response:**
```json
{
  "id": 1,
  "username": "johndoe",
  "email": "john@example.com",
  "role": "User",
  "lastSyncAt": "2024-01-01T12:00:00Z"
}
```

#### GET /api/users/sync
Get sync state for SPA/mobile apps and update last sync time.

**Query Parameters:**
- `userId` (required): User ID

**Response:**
```json
{
  "lastSyncAt": "2024-01-01T12:00:00Z",
  "unreadArticleCount": 42,
  "newFeedCount": 2
}
```

## Database Schema

The API uses PostgreSQL with the following main entities:

- **User**: User accounts with roles
- **Feed**: RSS feeds (personal or globally approved)
- **UserFeed**: User subscriptions to feeds
- **Article**: Articles from feeds
- **Tag**: Tags for categorization
- **ArticleTag**: Many-to-many relationship with confidence scores
- **UserTag**: User preferences for tags with decay
- **TagRelationship**: Co-occurrence tracking between tags
- **Vote**: User votes on articles/feeds

## Tag System

### Interaction Types
- **Explicit**: User explicitly marked tag as interesting
- **Ignored**: User explicitly wants to ignore tag
- **Clicked**: User clicked article with this tag
- **Liked**: User liked article with this tag

### Tag Decay
Tags decay as new interests are added rather than over time. When a user adds new tag interactions, the system applies exponential decay to existing tag scores based on their relative interaction counts. Tags with more interactions decay less aggressively, while tags with fewer interactions decay more. This allows the system to naturally adapt to changing user interests without arbitrary time limits.

## Future Enhancements

- Full authentication and authorization
- Caching layer with Redis
- Search with Elasticsearch
- Real-time notifications
- Batch operations
- Rate limiting
