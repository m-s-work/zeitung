using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Zeitung.Api.Services;
using Zeitung.Worker.Models;

namespace Zeitung.Api.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly ZeitungDbContext _dbContext;
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ZeitungDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ZeitungDbContext(options);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-that-is-at-least-32-characters-long-for-testing",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:AccessTokenExpirationMinutes"] = "15",
                ["RefreshToken:ExpirationDays"] = "30"
            })
            .Build();

        _jwtService = new JwtService(configuration);
        _authService = new AuthService(_dbContext, _jwtService, configuration);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task AuthenticateAsync_WithNewUser_ShouldCreateUserAndTokens()
    {
        // Arrange
        var email = "newuser@example.com";

        // Act
        var (accessToken, refreshToken, expiresAt) = await _authService.AuthenticateAsync(email);

        // Assert
        Assert.NotNull(accessToken);
        Assert.NotNull(refreshToken);
        Assert.True(expiresAt > DateTime.UtcNow);

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
        Assert.NotNull(user);
        Assert.NotNull(user.LastLoginAt);

        var storedRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        Assert.NotNull(storedRefreshToken);
        Assert.Equal(user.Id, storedRefreshToken.UserId);
    }

    [Fact]
    public async Task AuthenticateAsync_WithExistingUser_ShouldUpdateLastLogin()
    {
        // Arrange
        var email = "existinguser@example.com";
        var existingUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            IsActive = true
        };
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        // Act
        var (accessToken, refreshToken, expiresAt) = await _authService.AuthenticateAsync(email);

        // Assert
        Assert.NotNull(accessToken);
        Assert.NotNull(refreshToken);

        var user = await _dbContext.Users.FindAsync(existingUser.Id);
        Assert.NotNull(user);
        Assert.NotNull(user.LastLoginAt);
        Assert.True(user.LastLoginAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var email = "test@example.com";
        var (_, oldRefreshToken, _) = await _authService.AuthenticateAsync(email);

        // Act
        var (isValid, accessToken, newRefreshToken, expiresAt) = await _authService.RefreshTokenAsync(oldRefreshToken);

        // Assert
        Assert.True(isValid);
        Assert.NotNull(accessToken);
        Assert.NotNull(newRefreshToken);
        Assert.NotNull(expiresAt);
        Assert.NotEqual(oldRefreshToken, newRefreshToken);

        var oldToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == oldRefreshToken);
        Assert.NotNull(oldToken);
        Assert.True(oldToken.IsRevoked);

        var newToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == newRefreshToken);
        Assert.NotNull(newToken);
        Assert.True(newToken.IsActive);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ShouldReturnInvalid()
    {
        // Arrange
        var invalidToken = "invalid-refresh-token";

        // Act
        var (isValid, accessToken, refreshToken, expiresAt) = await _authService.RefreshTokenAsync(invalidToken);

        // Assert
        Assert.False(isValid);
        Assert.Null(accessToken);
        Assert.Null(refreshToken);
        Assert.Null(expiresAt);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ShouldReturnInvalid()
    {
        // Arrange
        var email = "test@example.com";
        var (_, refreshToken, _) = await _authService.AuthenticateAsync(email);
        await _authService.RevokeRefreshTokenAsync(refreshToken);

        // Act
        var (isValid, accessToken, newRefreshToken, expiresAt) = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        Assert.False(isValid);
        Assert.Null(accessToken);
        Assert.Null(newRefreshToken);
        Assert.Null(expiresAt);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ShouldMarkTokenAsRevoked()
    {
        // Arrange
        var email = "test@example.com";
        var (_, refreshToken, _) = await _authService.AuthenticateAsync(email);

        // Act
        await _authService.RevokeRefreshTokenAsync(refreshToken);

        // Assert
        var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        Assert.NotNull(token);
        Assert.True(token.IsRevoked);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithInvalidToken_ShouldNotThrow()
    {
        // Arrange
        var invalidToken = "invalid-refresh-token";

        // Act & Assert
        await _authService.RevokeRefreshTokenAsync(invalidToken); // Should not throw
    }
}
