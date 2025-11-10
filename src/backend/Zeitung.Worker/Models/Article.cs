namespace Zeitung.Worker.Models;

public class Article
{
    public required string Title { get; init; }
    public required string Link { get; init; }
    public required string Description { get; init; }
    public DateTime PublishedDate { get; init; }
    public List<string> Categories { get; init; } = new();
    public List<string> Tags { get; set; } = new();
    public required string FeedSource { get; init; }
}
