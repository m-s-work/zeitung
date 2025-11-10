namespace Zeitung.Api.DTOs;

public record ArticleDto(
    int Id,
    string Title,
    string Link,
    string Description,
    DateTime PublishedDate,
    string FeedName,
    List<TagDto> Tags,
    int? UserVote
);

public record ArticleListDto(
    int Id,
    string Title,
    string Link,
    string Description,
    DateTime PublishedDate,
    string FeedName,
    List<string> Tags
);

public record VoteDto(
    int Value  // 1 for upvote, -1 for downvote
);
