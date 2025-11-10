using Zeitung.Api.Models;

namespace Zeitung.Api.Endpoints;

/// <summary>
/// Article and recommendation endpoints
/// </summary>
public static class ArticleEndpoints
{
    public static void MapArticleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/articles")
            .WithTags("Articles")
            .WithOpenApi();

        // Get recommended articles feed
        group.MapGet("/recommended", GetRecommendedArticles)
            .WithName("GetRecommendedArticles")
            .WithSummary("Get recommended articles")
            .WithDescription("Returns a personalized feed of recommended articles based on user preferences");

        // Get article details
        group.MapGet("/{id}", GetArticle)
            .WithName("GetArticle")
            .WithSummary("Get article details")
            .WithDescription("Returns detailed information about a specific article");

        // Record recommendation feedback (like/dislike)
        group.MapPost("/{id}/feedback", RecordFeedback)
            .WithName("RecordRecommendationFeedback")
            .WithSummary("Record recommendation feedback")
            .WithDescription("Records whether user liked or disliked a recommendation");

        // Record article open
        group.MapPost("/{id}/open", RecordOpen)
            .WithName("RecordArticleOpen")
            .WithSummary("Record article open")
            .WithDescription("Records that user opened an article");

        // Record read time
        group.MapPost("/{id}/readtime", RecordReadTime)
            .WithName("RecordReadTime")
            .WithSummary("Record read time")
            .WithDescription("Records time user spent reading an article");

        // Rate article
        group.MapPost("/{id}/rating", RateArticle)
            .WithName("RateArticle")
            .WithSummary("Rate article")
            .WithDescription("Records user's rating (1-5 stars) for an article");

        // Get user's interaction with article
        group.MapGet("/{id}/interaction", GetInteraction)
            .WithName("GetArticleInteraction")
            .WithSummary("Get article interaction")
            .WithDescription("Returns user's interaction history with a specific article");
    }

    private static async Task<IResult> GetRecommendedArticles(int? limit, int? offset)
    {
        // TODO: Implement actual recommendation algorithm
        // For now, return mock data
        var articles = new List<RecommendedArticle>
        {
            new()
            {
                Article = new Article
                {
                    Id = "1",
                    Title = "The Future of AI in Software Development",
                    Url = "https://techcrunch.com/article1",
                    Content = "Artificial intelligence is rapidly changing how we write code...",
                    Author = "Jane Doe",
                    FeedId = "1",
                    FeedTitle = "TechCrunch",
                    PublishedAt = DateTime.UtcNow.AddHours(-3),
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    Tags = ["AI", "Software Development", "Technology"],
                    RecommendationScore = 0.95
                },
                Score = 0.95,
                Reason = "Matches your interests in AI and software development"
            },
            new()
            {
                Article = new Article
                {
                    Id = "2",
                    Title = "Understanding Rust's Ownership System",
                    Url = "https://example.com/rust-ownership",
                    Content = "Rust's ownership system is one of its most unique features...",
                    Author = "John Smith",
                    FeedId = "2",
                    FeedTitle = "Hacker News",
                    PublishedAt = DateTime.UtcNow.AddHours(-5),
                    CreatedAt = DateTime.UtcNow.AddHours(-4),
                    Tags = ["Rust", "Programming", "Systems"],
                    RecommendationScore = 0.87
                },
                Score = 0.87,
                Reason = "Popular in your reading history"
            },
            new()
            {
                Article = new Article
                {
                    Id = "3",
                    Title = "Building Scalable Microservices with Kubernetes",
                    Url = "https://techcrunch.com/article3",
                    Content = "Kubernetes has become the de facto standard for container orchestration...",
                    Author = "Alice Johnson",
                    FeedId = "1",
                    FeedTitle = "TechCrunch",
                    PublishedAt = DateTime.UtcNow.AddHours(-8),
                    CreatedAt = DateTime.UtcNow.AddHours(-7),
                    Tags = ["Kubernetes", "DevOps", "Cloud"],
                    RecommendationScore = 0.82
                },
                Score = 0.82,
                Reason = "Similar to articles you've liked"
            }
        };

        var limitValue = limit ?? 20;
        var offsetValue = offset ?? 0;
        var result = articles.Skip(offsetValue).Take(limitValue).ToList();

        return Results.Ok(result);
    }

    private static async Task<IResult> GetArticle(string id)
    {
        // TODO: Implement actual database query
        if (id == "1")
        {
            return Results.Ok(new Article
            {
                Id = "1",
                Title = "The Future of AI in Software Development",
                Url = "https://techcrunch.com/article1",
                Content = "Artificial intelligence is rapidly changing how we write code...",
                Author = "Jane Doe",
                FeedId = "1",
                FeedTitle = "TechCrunch",
                PublishedAt = DateTime.UtcNow.AddHours(-3),
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                Tags = ["AI", "Software Development", "Technology"]
            });
        }

        return Results.NotFound(new { message = "Article not found" });
    }

    private static async Task<IResult> RecordFeedback(string id, RecommendationFeedbackRequest request)
    {
        // TODO: Implement actual feedback recording
        // Should update user preferences based on feedback
        return Results.Ok(new { message = "Feedback recorded", articleId = id, liked = request.Liked });
    }

    private static async Task<IResult> RecordOpen(string id)
    {
        // TODO: Implement actual open tracking
        return Results.Ok(new { message = "Article open recorded", articleId = id, timestamp = DateTime.UtcNow });
    }

    private static async Task<IResult> RecordReadTime(string id, ArticleReadTimeRequest request)
    {
        // TODO: Implement actual read time tracking
        return Results.Ok(new { message = "Read time recorded", articleId = id, readTimeSeconds = request.ReadTimeSeconds });
    }

    private static async Task<IResult> RateArticle(string id, ArticleRatingRequest request)
    {
        // TODO: Implement actual rating storage
        if (request.Rating < 1 || request.Rating > 5)
        {
            return Results.BadRequest(new { message = "Rating must be between 1 and 5" });
        }

        return Results.Ok(new { message = "Rating recorded", articleId = id, rating = request.Rating });
    }

    private static async Task<IResult> GetInteraction(string id)
    {
        // TODO: Implement actual interaction query
        // For now, return mock data
        return Results.Ok(new UserArticleInteraction
        {
            ArticleId = id,
            UserId = "user1",
            LikedRecommendation = true,
            Opened = true,
            OpenedAt = DateTime.UtcNow.AddMinutes(-30),
            ReadTimeSeconds = 180,
            Rating = 4,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-25)
        });
    }
}
