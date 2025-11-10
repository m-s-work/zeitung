using System.Text;
using System.Text.Json;
using Zeitung.Worker.Models;

namespace Zeitung.Worker.Strategies;

public class LlmTaggingStrategy : ITaggingStrategy
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LlmTaggingStrategy> _logger;
    private readonly string _apiKey;
    private readonly string _apiUrl;

    public LlmTaggingStrategy(
        IHttpClientFactory httpClientFactory, 
        ILogger<LlmTaggingStrategy> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _apiKey = configuration["OpenRouter:ApiKey"] ?? throw new InvalidOperationException("OpenRouter:ApiKey not configured");
        _apiUrl = configuration["OpenRouter:ApiUrl"] ?? "https://openrouter.ai/api/v1/chat/completions";
    }

    public async Task<List<string>> GenerateTagsAsync(Article article, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            var prompt = $@"Extract 5-10 relevant tags from this article. Return only comma-separated tags without any additional text.

Title: {article.Title}
Description: {article.Description}

Tags:";

            var request = new
            {
                model = "meta-llama/llama-3.1-8b-instruct:free",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var requestJson = JsonSerializer.Serialize(request);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            
            var response = await httpClient.PostAsync(_apiUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
            
            var tagsText = result.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
            
            var tags = tagsText.Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
            
            return tags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating LLM tags for article: {Title}", article.Title);
            // Fallback to basic tags from categories
            return article.Categories.ToList();
        }
    }
}
