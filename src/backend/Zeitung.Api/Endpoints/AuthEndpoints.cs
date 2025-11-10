using Zeitung.Api.DTOs;
using Zeitung.Api.Services;

namespace Zeitung.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // POST /api/auth/login - Request magic link
        group.MapPost("/login", async (LoginRequest request, IMagicLinkService magicLinkService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Results.BadRequest(new { error = "Email is required" });
            }

            var token = await magicLinkService.GenerateMagicLinkAsync(request.Email);
            
            // In production, send this token via email
            // For now, return it in the response for testing
            return Results.Ok(new { message = "Magic link generated", token });
        })
        .WithName("RequestMagicLink")
        .WithSummary("Request a magic link for passwordless authentication")
        .WithDescription("Generates a one-time magic link token sent to the specified email address");

        // POST /api/auth/verify - Verify magic link and get tokens
        group.MapPost("/verify", async (VerifyMagicLinkRequest request, IMagicLinkService magicLinkService, IAuthService authService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return Results.BadRequest(new { error = "Token is required" });
            }

            var (isValid, email) = await magicLinkService.ValidateMagicLinkAsync(request.Token);
            
            if (!isValid || email == null)
            {
                return Results.Unauthorized();
            }

            var (accessToken, refreshToken, expiresAt) = await authService.AuthenticateAsync(email);
            
            return Results.Ok(new AuthResponse(accessToken, refreshToken, expiresAt));
        })
        .WithName("VerifyMagicLink")
        .WithSummary("Verify magic link and receive authentication tokens")
        .WithDescription("Exchanges a valid magic link token for JWT access and refresh tokens");

        // POST /api/auth/refresh - Refresh access token
        group.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return Results.BadRequest(new { error = "Refresh token is required" });
            }

            var (isValid, accessToken, refreshToken, expiresAt) = await authService.RefreshTokenAsync(request.RefreshToken);
            
            if (!isValid || accessToken == null || refreshToken == null || expiresAt == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new AuthResponse(accessToken, refreshToken, expiresAt.Value));
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh an expired access token")
        .WithDescription("Exchanges a valid refresh token for a new access token and refresh token pair");

        // POST /api/auth/revoke - Revoke refresh token (logout)
        group.MapPost("/revoke", async (RefreshTokenRequest request, IAuthService authService) =>
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return Results.BadRequest(new { error = "Refresh token is required" });
            }

            await authService.RevokeRefreshTokenAsync(request.RefreshToken);
            
            return Results.Ok(new { message = "Token revoked successfully" });
        })
        .WithName("RevokeRefreshToken")
        .WithSummary("Revoke a refresh token")
        .WithDescription("Invalidates a refresh token, effectively logging out the user");
    }
}
