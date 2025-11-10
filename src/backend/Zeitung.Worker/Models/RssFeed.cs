namespace Zeitung.Worker.Models;

public class RssFeed
{
    public required string Url { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}
