namespace Zeitung.Api.DTOs;

public record UserDto(
    int Id,
    string Username,
    string Email,
    string Role,
    DateTime LastSyncAt
);

public record SyncStateDto(
    DateTime LastSyncAt,
    int UnreadArticleCount,
    int NewFeedCount
);
