namespace Zeitung.Api.DTOs;

public record LoginRequest(string Email);

public record VerifyMagicLinkRequest(string Token);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
