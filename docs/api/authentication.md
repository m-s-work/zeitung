# Authentication Documentation

This API implements passwordless authentication using magic links with JWT access tokens and refresh tokens.

## Authentication Flow

1. **Request Magic Link**: User submits email address
2. **Verify Magic Link**: User clicks link with token, receives JWT access token and refresh token
3. **Use Access Token**: Client uses JWT bearer token for authenticated requests
4. **Refresh Token**: When access token expires, use refresh token to get new access token

## Endpoints

### POST /auth/login
Request a magic link for authentication.

**Request:**
```json
{
  "email": "user@example.com"
}
```

**Response:**
```json
{
  "message": "Magic link generated",
  "token": "base64-encoded-token"
}
```

**Note:** In production, the token should be sent via email, not returned in the response.

### POST /auth/verify
Verify a magic link token and receive authentication tokens.

**Request:**
```json
{
  "token": "base64-encoded-token"
}
```

**Response:**
```json
{
  "accessToken": "jwt-access-token",
  "refreshToken": "base64-encoded-refresh-token",
  "expiresAt": "2024-12-10T12:00:00Z"
}
```

### POST /auth/refresh
Refresh an expired access token using a refresh token.

**Request:**
```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

**Response:**
```json
{
  "accessToken": "new-jwt-access-token",
  "refreshToken": "new-base64-encoded-refresh-token",
  "expiresAt": "2024-12-10T12:00:00Z"
}
```

### POST /auth/revoke
Revoke a refresh token (logout).

**Request:**
```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

**Response:**
```json
{
  "message": "Token revoked successfully"
}
```

## Configuration

Configure authentication settings in `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "your-256-bit-secret-key-change-in-production",
    "Issuer": "zeitung-api",
    "Audience": "zeitung-app",
    "AccessTokenExpirationMinutes": "15"
  },
  "MagicLink": {
    "ExpirationMinutes": "15"
  },
  "RefreshToken": {
    "ExpirationDays": "30"
  }
}
```

## Security Considerations

1. **Secret Key**: Must be at least 32 characters long and stored securely (environment variables, Azure Key Vault, etc.)
2. **HTTPS Only**: All authentication endpoints should only be accessible over HTTPS in production
3. **Email Service**: Implement email service to send magic links (currently returns token in response for testing)
4. **Token Storage**: Store refresh tokens securely on the client side
5. **Token Rotation**: Refresh tokens are automatically rotated on each refresh request
6. **Token Revocation**: Old refresh tokens are revoked when new ones are issued

## Using the Access Token

Include the JWT access token in the Authorization header:

```
Authorization: Bearer {accessToken}
```

Example with curl:
```bash
curl -H "Authorization: Bearer eyJhbGciOiJIUzI1..." http://localhost:8080/api/protected-endpoint
```

