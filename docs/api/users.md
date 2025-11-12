# Users API

Manage user accounts and sync state.

## Endpoints

### GET /api/users/current

Get current user information based on the authenticated JWT token.

**Authentication:** Required

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

---

### GET /api/users/sync

Get sync state for SPA/mobile apps and update last sync time. Used to check for new content.

**Authentication:** Required

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

## Examples

### Get Current User

```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:8080/api/users/current?userId=YOUR_USER_ID
```

### Get Sync State

```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:8080/api/users/sync?userId=YOUR_USER_ID
```

## User Properties

| Property | Type | Description |
|----------|------|-------------|
| id | number | Unique user identifier |
| username | string | Display name |
| email | string | User's email address |
| role | string | User role (User, Moderator, Admin) |
| lastSyncAt | datetime | Last sync timestamp |

## User Roles

### User
- Default role for new accounts
- Can manage personal feeds
- Can interact with articles
- Can set tag preferences

### Moderator
- All User permissions
- Can promote feeds to global list
- Can moderate content
- Can help with community curation

### Admin
- All Moderator permissions
- Can manage users
- Can configure system settings
- Full access to all features

## Sync State

The sync endpoint helps mobile and SPA applications:

- **lastSyncAt**: Last time data was synchronized
- **unreadArticleCount**: New articles since last sync
- **newFeedCount**: New feeds added since last sync

### Usage Pattern

```javascript
// Initial load
const syncState = await api.getSync(userId);
console.log(`${syncState.unreadArticleCount} new articles`);

// Poll for updates (every 5 minutes)
setInterval(async () => {
  const syncState = await api.getSync(userId);
  if (syncState.unreadArticleCount > 0) {
    // Fetch new articles
    refreshArticles();
  }
}, 5 * 60 * 1000);
```

## User Management

### Account Creation

Users are created automatically when they verify a magic link:

1. Request magic link with email
2. Click link in email
3. Account created on first verification
4. JWT tokens issued

### Account Deletion

Currently not implemented. Planned for future release.

### Profile Updates

Currently not implemented. Planned features:
- Update username
- Update email
- Notification preferences
- Privacy settings

## Best Practices

### For Users
- Keep your email up to date for magic links
- Use sync endpoint to efficiently check for updates
- Don't poll too frequently (max once per minute)

### For Developers
- Cache user info after first fetch
- Use sync endpoint for polling, not full data fetch
- Implement exponential backoff for errors
- Handle token expiration gracefully

## Notes

- Users are identified by email address
- Email addresses must be unique
- User IDs are permanent
- Username can be changed (future)
- Role changes require admin access
