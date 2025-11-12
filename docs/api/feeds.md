# Feeds API

Manage RSS feeds, subscriptions, and feed recommendations.

## Endpoints

### GET /api/feeds

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

---

### POST /api/feeds

Add a feed to the user's personal feed list.

**Authentication:** Required

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

---

### DELETE /api/feeds/{id}

Remove a feed from the user's personal feed list.

**Authentication:** Required

**Path Parameters:**
- `id`: Feed ID

**Query Parameters:**
- `userId` (required): User ID

**Response:** `204 No Content`

---

### POST /api/feeds/{id}/promote

Promote a feed to the global feed list. Requires admin or moderator role.

**Authentication:** Required (Admin/Moderator)

**Path Parameters:**
- `id`: Feed ID

**Response:**
```json
{
  "message": "Feed promoted to global list"
}
```

---

### GET /api/feeds/recommendations

Get personalized feed recommendations based on user's tag preferences.

**Authentication:** Required

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

## Examples

### List All Feeds

```bash
curl http://localhost:8080/api/feeds
```

### Add a Personal Feed

```bash
curl -X POST \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "url": "https://blog.example.com/rss",
    "name": "Example Tech Blog",
    "description": "A great tech blog"
  }' \
  http://localhost:8080/api/feeds?userId=YOUR_USER_ID
```

### Remove a Feed

```bash
curl -X DELETE \
  -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:8080/api/feeds/123?userId=YOUR_USER_ID
```

### Promote a Feed (Admin/Mod only)

```bash
curl -X POST \
  -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:8080/api/feeds/123/promote
```

### Get Feed Recommendations

```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:8080/api/feeds/recommendations?userId=YOUR_USER_ID
```

## Feed Properties

| Property | Type | Description |
|----------|------|-------------|
| id | number | Unique feed identifier |
| url | string | RSS/Atom feed URL |
| name | string | Display name for the feed |
| description | string | Optional description |
| isApproved | boolean | Whether feed is in global list |
| isSubscribed | boolean | Whether user is subscribed |
| createdAt | datetime | When feed was added |
| lastFetchedAt | datetime | Last successful fetch |

## Feed Approval Process

1. User adds a feed (personal only)
2. Moderator/Admin reviews the feed
3. If quality content, promotes to global list
4. All users can now subscribe to the feed

## Notes

- Personal feeds are private to each user
- Global feeds are visible to all users
- Feeds are fetched periodically by the worker service
- Failed fetches are logged but don't remove the feed
- Duplicate URLs are prevented
