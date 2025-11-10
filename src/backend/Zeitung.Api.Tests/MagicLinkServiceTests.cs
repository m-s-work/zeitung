using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Zeitung.Api.Services;
using Zeitung.Worker.Models;

namespace Zeitung.Api.Tests;

public class MagicLinkServiceTests : IDisposable
{
    private readonly ZeitungDbContext _dbContext;
    private readonly IMagicLinkService _magicLinkService;

    public MagicLinkServiceTests()
    {
        var options = new DbContextOptionsBuilder<ZeitungDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ZeitungDbContext(options);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MagicLink:ExpirationMinutes"] = "15"
            })
            .Build();

        _magicLinkService = new MagicLinkService(_dbContext, configuration);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task GenerateMagicLinkAsync_ShouldCreateToken()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var token = await _magicLinkService.GenerateMagicLinkAsync(email);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var magicLink = await _dbContext.MagicLinks.FirstOrDefaultAsync(ml => ml.Token == token);
        Assert.NotNull(magicLink);
        Assert.Equal(email.ToLowerInvariant(), magicLink.Email);
        Assert.True(magicLink.IsValid);
    }

    [Fact]
    public async Task ValidateMagicLinkAsync_WithValidToken_ShouldReturnEmailAndMarkAsUsed()
    {
        // Arrange
        var email = "test@example.com";
        var token = await _magicLinkService.GenerateMagicLinkAsync(email);

        // Act
        var (isValid, returnedEmail) = await _magicLinkService.ValidateMagicLinkAsync(token);

        // Assert
        Assert.True(isValid);
        Assert.Equal(email.ToLowerInvariant(), returnedEmail);

        var magicLink = await _dbContext.MagicLinks.FirstOrDefaultAsync(ml => ml.Token == token);
        Assert.NotNull(magicLink);
        Assert.True(magicLink.IsUsed);
    }

    [Fact]
    public async Task ValidateMagicLinkAsync_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var invalidToken = "invalid-token";

        // Act
        var (isValid, email) = await _magicLinkService.ValidateMagicLinkAsync(invalidToken);

        // Assert
        Assert.False(isValid);
        Assert.Null(email);
    }

    [Fact]
    public async Task ValidateMagicLinkAsync_WithUsedToken_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var token = await _magicLinkService.GenerateMagicLinkAsync(email);
        await _magicLinkService.ValidateMagicLinkAsync(token); // Use it once

        // Act
        var (isValid, returnedEmail) = await _magicLinkService.ValidateMagicLinkAsync(token);

        // Assert
        Assert.False(isValid);
        Assert.Null(returnedEmail);
    }

    [Fact]
    public async Task ValidateMagicLinkAsync_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var magicLink = new MagicLinkEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            Token = "expired-token",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1) // Expired
        };
        _dbContext.MagicLinks.Add(magicLink);
        await _dbContext.SaveChangesAsync();

        // Act
        var (isValid, returnedEmail) = await _magicLinkService.ValidateMagicLinkAsync(magicLink.Token);

        // Assert
        Assert.False(isValid);
        Assert.Null(returnedEmail);
    }
}
