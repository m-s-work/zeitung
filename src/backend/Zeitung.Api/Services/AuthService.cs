using Microsoft.EntityFrameworkCore;
using Zeitung.Core.Models;

namespace Zeitung.Api.Services;

public interface IAuthService
{
    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> AuthenticateAsync(string email);
    Task<(bool IsValid, string? AccessToken, string? RefreshToken, DateTime? ExpiresAt)> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly ZeitungDbContext _dbContext;
    private readonly IJwtService _jwtService;
    private readonly int _refreshTokenExpirationDays;

    public AuthService(ZeitungDbContext dbContext, IJwtService jwtService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _refreshTokenExpirationDays = int.Parse(configuration["RefreshToken:ExpirationDays"] ?? "30");
    }

    public async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> AuthenticateAsync(string email)
    {
        email = email.ToLowerInvariant();
        
        // Get or create user
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            user = new User
            {
                Username = email.Split('@')[0], // Use email prefix as username
                Email = email,
                Role = "User", // Default role
                CreatedAt = DateTime.UtcNow,
                LastSyncAt = DateTime.UtcNow,
                IsActive = true
            };
            _dbContext.Users.Add(user);
        }
        
        user.LastLoginAt = DateTime.UtcNow;

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return (accessToken, refreshTokenValue, expiresAt);
    }

    public async Task<(bool IsValid, string? AccessToken, string? RefreshToken, DateTime? ExpiresAt)> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null || !storedToken.IsActive || !storedToken.User.IsActive)
        {
            return (false, null, null, null);
        }

        // Generate new tokens
        var accessToken = _jwtService.GenerateAccessToken(storedToken.User.Id, storedToken.User.Email);
        var newRefreshTokenValue = _jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);

        // Revoke old refresh token
        storedToken.RevokedAt = DateTime.UtcNow;

        // Create new refresh token
        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = storedToken.UserId,
            Token = newRefreshTokenValue,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };

        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync();

        return (true, accessToken, newRefreshTokenValue, expiresAt);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token != null && !token.IsRevoked)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }
}
