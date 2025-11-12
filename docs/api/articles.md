# Articles API

Read articles, track interactions, and manage votes.

## Endpoints

### GET /api/articles

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

---

### GET /api/articles/{id}

Get detailed article information.

**Path Parameters:**
- `id`: Article ID

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

---

### POST /api/articles/{id}/vote

Vote on an article. Use `1` for upvote, `-1` for downvote.

**Authentication:** Required

**Path Parameters:**
- `id`: Article ID

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

---

### POST /api/articles/{id}/like

Like an article and update user's tag preferences. This is a stronger signal than clicking.

**Authentication:** Required

**Path Parameters:**
- `id`: Article ID

**Query Parameters:**
- `userId` (required): User ID

**Response:**
```json
{
  "message": "Article liked, tag preferences updated"
}
```

---

### POST /api/articles/{id}/click

Track article click and update user's tag preferences. Used for implicit interest tracking.

**Authentication:** Required

**Path Parameters:**
- `id`: Article ID

**Query Parameters:**
- `userId` (required): User ID

**Response:**
```json
{
  "message": "Article click tracked"
}
```

## Examples

### List Articles with Pagination

```bash
curl http://localhost:8080/api/articles?page=1&pageSize=20
```

### Filter Articles by Feed

```bash
curl http://localhost:8080/api/articles?feedId=5
```

### Filter Articles by Tag

```bash
curl http://localhost:8080/api/articles?tagId=10
```

### Get Article Details

```bash
curl http://localhost:8080/api/articles/123
```

### Vote on an Article

```bash
# Upvote
curl -X POST \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"value": 1}' \
  http://localhost:8080/api/articles/123/vote?userId=YOUR_USER_ID

# Downvote
curl -X POST \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"value": -1}' \
  http://localhost:8080/api/articles/123/vote?userId=YOUR_USER_ID
```

### Like an Article

```bash
curl -X POST \
  -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:8080/api/articles/123/like?userId=YOUR_USER_ID
```

### Track Article Click

```bash
curl -X POST \
  -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:8080/api/articles/123/click?userId=YOUR_USER_ID
```

## Article Properties

| Property | Type | Description |
|----------|------|-------------|
| id | number | Unique article identifier |
| title | string | Article title |
| link | string | URL to the original article |
| description | string | Article summary/excerpt |
| publishedDate | datetime | Publication date |
| feedName | string | Name of the source feed |
| tags | array | Associated tags |
| userVote | number | User's vote (-1, 0, or 1) |

## Tag Confidence

Each article-tag relationship includes a confidence score:
- **0.0 - 0.3**: Low confidence
- **0.3 - 0.7**: Medium confidence
- **0.7 - 1.0**: High confidence

These scores come from the LLM tagging process and indicate how relevant the tag is to the article.

## Interaction Tracking

The system tracks three types of interactions:

1. **Clicks** (+1 point): Implicit interest signal
2. **Likes** (+3 points): Strong interest signal
3. **Votes**: Feedback on content quality (doesn't affect tag preferences as strongly)

These interactions update your tag preferences and improve recommendations over time.

## Notes

- Articles are fetched by the worker service
- New articles appear automatically as feeds are polled
- Duplicate articles (same URL) are prevented
- Old articles may be archived after a configurable period
- Article content is not stored, only metadata and links
