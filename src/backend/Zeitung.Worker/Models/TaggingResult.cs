namespace Zeitung.Worker.Models;

public class TaggingResult
{
    public List<TagWithConfidence> Tags { get; set; } = new();
    public string? Comment { get; set; }
    public string? Error { get; set; }
}

public class TagWithConfidence
{
    public required string Tag { get; set; }
    public double Probability { get; set; }
}
