using Zeitung.Core.Models;
namespace Zeitung.Api.DTOs;

public record TagDto(
    int Id,
    string Name,
    double Confidence
);

public record UserTagPreferenceDto(
    int TagId,
    string TagName,
    string InteractionType,
    double Score,
    int InteractionCount
);

public record TagInterestDto(
    List<int> TagIds
);

public record TagSummaryDto(
    Dictionary<string, double> TagScores,
    DateTime LastUpdated
);
