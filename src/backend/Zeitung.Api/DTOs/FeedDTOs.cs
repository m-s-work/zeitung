namespace Zeitung.Api.DTOs;

public record FeedDto(
    int Id,
    string Url,
    string Name,
    string? Description,
    bool IsApproved,
    bool IsSubscribed,
    DateTime CreatedAt,
    DateTime? LastFetchedAt
);

public record CreateFeedDto(
    string Url,
    string Name,
    string? Description
);

public record FeedRecommendationDto(
    int Id,
    string Url,
    string Name,
    string? Description,
    double RelevanceScore,
    List<string> RelevantTags
);
