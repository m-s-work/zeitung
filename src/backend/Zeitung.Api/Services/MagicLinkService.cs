using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Zeitung.Core.Context;
using Zeitung.Core.Models;

namespace Zeitung.Api.Services;

public interface IMagicLinkService
{
    Task<string> GenerateMagicLinkAsync(string email);
    Task<(bool IsValid, string? Email)> ValidateMagicLinkAsync(string token);
}

public class MagicLinkService : IMagicLinkService
{
    private readonly ZeitungDbContext _dbContext;
    private readonly int _magicLinkExpirationMinutes;

    public MagicLinkService(ZeitungDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _magicLinkExpirationMinutes = int.Parse(configuration["MagicLink:ExpirationMinutes"] ?? "15");
    }

    public async Task<string> GenerateMagicLinkAsync(string email)
    {
        var token = GenerateSecureToken();
        
        var magicLink = new MagicLink
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_magicLinkExpirationMinutes)
        };

        _dbContext.MagicLinks.Add(magicLink);
        await _dbContext.SaveChangesAsync();

        return token;
    }

    public async Task<(bool IsValid, string? Email)> ValidateMagicLinkAsync(string token)
    {
        var magicLink = await _dbContext.MagicLinks
            .FirstOrDefaultAsync(ml => ml.Token == token);

        if (magicLink == null || !magicLink.IsValid)
        {
            return (false, null);
        }

        // Mark the magic link as used
        magicLink.UsedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return (true, magicLink.Email);
    }

    private static string GenerateSecureToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
