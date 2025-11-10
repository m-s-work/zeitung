using Microsoft.Extensions.Configuration;
using Zeitung.Api.Services;

namespace Zeitung.Api.Tests;

public class JwtServiceTests
{
    private readonly IJwtService _jwtService;

    public JwtServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-that-is-at-least-32-characters-long-for-testing",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:AccessTokenExpirationMinutes"] = "15"
            })
            .Build();

        _jwtService = new JwtService(configuration);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";

        // Act
        var token = _jwtService.GenerateAccessToken(userId, email);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens()
    {
        // Act
        var token1 = _jwtService.GenerateRefreshToken();
        var token2 = _jwtService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(token1);
        Assert.NotNull(token2);
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var token = _jwtService.GenerateAccessToken(userId, email);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        Assert.NotNull(principal);
        var subClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        Assert.NotNull(subClaim);
        Assert.Equal(userId.ToString(), subClaim.Value);
        var emailClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.Email);
        Assert.NotNull(emailClaim);
        Assert.Equal(email, emailClaim.Value);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _jwtService.ValidateToken(invalidToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange - Create service with very short expiration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-that-is-at-least-32-characters-long-for-testing",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:AccessTokenExpirationMinutes"] = "0" // Already expired
            })
            .Build();

        var shortLivedService = new JwtService(configuration);
        var userId = Guid.NewGuid();
        var token = shortLivedService.GenerateAccessToken(userId, "test@example.com");

        // Wait a brief moment to ensure expiration
        Thread.Sleep(100);

        // Act
        var principal = shortLivedService.ValidateToken(token);

        // Assert
        Assert.Null(principal);
    }
}
