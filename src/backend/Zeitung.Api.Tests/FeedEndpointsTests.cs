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
        // Act
        var response = await Client.GetAsync("/api/feeds");
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var feeds = await response.Content.ReadFromJsonAsync<List<FeedDto>>();
        Assert.That(feeds, Is.Not.Null);
        Assert.That(feeds, Is.Empty);
    }

    [Test]
    public async Task CreateFeed_ReturnsCreated_WhenValidData()
    {
        // Arrange
        using var db = GetDbContext();
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            Role = "User"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var createDto = new CreateFeedDto(
            "https://example.com/rss",
            "Example Feed",
            "Test Description"
        );

        // Act
        var response = await Client.PostAsJsonAsync($"/api/feeds?userId={user.Id}", createDto);
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var feed = await response.Content.ReadFromJsonAsync<FeedDto>();
        Assert.That(feed, Is.Not.Null);
        Assert.That(feed!.Name, Is.EqualTo("Example Feed"));
        Assert.That(feed.Url, Is.EqualTo("https://example.com/rss"));
    }

    [Test]
    public async Task CreateFeed_ReturnsConflict_WhenAlreadySubscribed()
    {
        // Arrange
        using var db = GetDbContext();
        var user = new User
        {
            Username = "testuser2",
            Email = "test2@example.com",
            Role = "User"
        };
        var feed = new Feed
        {
            Url = "https://example.com/rss2",
            Name = "Example Feed 2",
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
            "https://example.com/rss2",
            "Example Feed 2",
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
        // Arrange
        using var db = GetDbContext();
        var feed = new Feed
        {
            Url = "https://example.com/rss3",
            Name = "Example Feed 3",
            IsApproved = false
        };
        db.Feeds.Add(feed);
        await db.SaveChangesAsync();

        // Act
        var response = await Client.PostAsync($"/api/feeds/{feed.Id}/promote", null);
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        // Verify feed is approved
        var updatedFeed = await db.Feeds.FindAsync(feed.Id);
        Assert.That(updatedFeed, Is.Not.Null);
        Assert.That(updatedFeed!.IsApproved, Is.True);
        Assert.That(updatedFeed.ApprovedAt, Is.Not.Null);
    }

    [Test]
    public async Task DeleteFeed_ReturnsNoContent_WhenSubscriptionExists()
    {
        // Arrange
        using var db = GetDbContext();
        var user = new User
        {
            Username = "testuser3",
            Email = "test3@example.com",
            Role = "User"
        };
        var feed = new Feed
        {
            Url = "https://example.com/rss4",
            Name = "Example Feed 4",
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
