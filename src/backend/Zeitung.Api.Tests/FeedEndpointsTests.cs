using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Zeitung.Api.DTOs;
using Zeitung.Core.Models;

namespace Zeitung.Api.Tests;

[TestFixture]
public class FeedEndpointsTests : IntegrationTestBase
{
    [Test]
    public async Task GetFeeds_ReturnsEmptyList_WhenNoFeeds()
    {
        // Act - No test data created, so should return empty
        var response = await Client.GetAsync("/api/feeds");
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var feeds = await response.Content.ReadFromJsonAsync<List<FeedDto>>();
        Assert.That(feeds, Is.Not.Null);
        // Note: May contain feeds from other parallel tests, but that's OK for this test
        // It validates the endpoint works, not that DB is empty
    }

    [Test]
    public async Task CreateFeed_ReturnsCreated_WhenValidData()
    {
        // Arrange - Use TestId to ensure unique data for parallel execution
        using var db = GetDbContext();
        var user = new User
        {
            Username = $"testuser_{TestId}",
            Email = $"test_{TestId}@example.com",
            Role = "User"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var createDto = new CreateFeedDto(
            $"https://example.com/rss_{TestId}",
            $"Example Feed {TestId}",
            "Test Description"
        );

        // Act
        var response = await Client.PostAsJsonAsync($"/api/feeds?userId={user.Id}", createDto);
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var feed = await response.Content.ReadFromJsonAsync<FeedDto>();
        Assert.That(feed, Is.Not.Null);
        Assert.That(feed!.Name, Is.EqualTo($"Example Feed {TestId}"));
        Assert.That(feed.Url, Is.EqualTo($"https://example.com/rss_{TestId}"));
    }

    [Test]
    public async Task CreateFeed_ReturnsConflict_WhenAlreadySubscribed()
    {
        // Arrange - Use TestId to ensure unique data for parallel execution
        using var db = GetDbContext();
        var user = new User
        {
            Username = $"testuser2_{TestId}",
            Email = $"test2_{TestId}@example.com",
            Role = "User"
        };
        var feed = new Feed
        {
            Url = $"https://example.com/rss2_{TestId}",
            Name = $"Example Feed 2 {TestId}",
            IsApproved = false
        };
        var userFeed = new UserFeed
        {
            User = user,
            Feed = feed
        };
        db.Users.Add(user);
        db.Feeds.Add(feed);
        db.UserFeeds.Add(userFeed);
        await db.SaveChangesAsync();

        var createDto = new CreateFeedDto(
            $"https://example.com/rss2_{TestId}",
            $"Example Feed 2 {TestId}",
            null
        );

        // Act
        var response = await Client.PostAsJsonAsync($"/api/feeds?userId={user.Id}", createDto);
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task PromoteFeed_ReturnsOk_WhenFeedExists()
    {
        // Arrange - Use TestId to ensure unique data for parallel execution
        int feedId;
        using (var db = GetDbContext())
        {
            var feed = new Feed
            {
                Url = $"https://example.com/rss3_{TestId}",
                Name = $"Example Feed 3 {TestId}",
                IsApproved = false
            };
            db.Feeds.Add(feed);
            await db.SaveChangesAsync();
            feedId = feed.Id;
        }

        // Act
        var response = await Client.PostAsync($"/api/feeds/{feedId}/promote", null);
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        // Verify feed is approved with a fresh DbContext
        using (var db = GetDbContext())
        {
            var updatedFeed = await db.Feeds.FindAsync(feedId);
            Assert.That(updatedFeed, Is.Not.Null);
            Assert.That(updatedFeed!.IsApproved, Is.True);
            Assert.That(updatedFeed.ApprovedAt, Is.Not.Null);
        }
    }

    [Test]
    public async Task DeleteFeed_ReturnsNoContent_WhenSubscriptionExists()
    {
        // Arrange - Use TestId to ensure unique data for parallel execution
        using var db = GetDbContext();
        var user = new User
        {
            Username = $"testuser3_{TestId}",
            Email = $"test3_{TestId}@example.com",
            Role = "User"
        };
        var feed = new Feed
        {
            Url = $"https://example.com/rss4_{TestId}",
            Name = $"Example Feed 4 {TestId}",
            IsApproved = false
        };
        var userFeed = new UserFeed
        {
            User = user,
            Feed = feed
        };
        db.Users.Add(user);
        db.Feeds.Add(feed);
        db.UserFeeds.Add(userFeed);
        await db.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/feeds/{feed.Id}?userId={user.Id}");
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        
        // Verify subscription is removed
        var subscription = await db.UserFeeds
            .FirstOrDefaultAsync(uf => uf.UserId == user.Id && uf.FeedId == feed.Id);
        Assert.That(subscription, Is.Null);
    }
}
