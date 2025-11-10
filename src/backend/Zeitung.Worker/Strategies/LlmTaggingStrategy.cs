using System.ClientModel;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;
using Zeitung.Worker.Models;

namespace Zeitung.Worker.Strategies;

public class LlmTaggingStrategy : ITaggingStrategy
{
    private readonly ILogger<LlmTaggingStrategy> _logger;
    private readonly ChatClient _chatClient;
    private const int MaxRetries = 3;

    public LlmTaggingStrategy(
        ILogger<LlmTaggingStrategy> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        var apiKey = configuration["OpenRouter:ApiKey"] ?? throw new InvalidOperationException("OpenRouter:ApiKey not configured");
        var apiUrl = configuration["OpenRouter:ApiUrl"] ?? "https://openrouter.ai/api/v1/chat/completions";
        var model = configuration["OpenRouter:Model"] ?? "meta-llama/llama-3.1-8b-instruct:free";

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(apiUrl)
        };

        var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey), options);
        _chatClient = openAiClient.GetChatClient(model);
    }

    public async Task<List<string>> GenerateTagsAsync(Article article, CancellationToken cancellationToken = default)
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var prompt = $@"Extract 5-10 relevant tags from this article. Return ONLY a valid JSON object in this exact format:
{{
  ""tags"": [
    {{ ""tag"": ""technology"", ""probability"": 0.95 }},
    {{ ""tag"": ""science"", ""probability"": 0.87 }}
  ],
  ""comment"": ""Brief explanation of tag selection"",
  ""error"": null
}}

Title: {article.Title}
Description: {article.Description}

Return only the JSON object, no additional text.";

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You are a precise tagging system that returns only valid JSON responses."),
                    new UserChatMessage(prompt)
                };

                var response = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
                var content = response.Value.Content[0].Text;

                // Try to parse the JSON response
                var taggingResult = ParseTaggingResponse(content);

                if (taggingResult != null && taggingResult.Tags.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(taggingResult.Comment))
                    {
                        _logger.LogInformation("LLM tagging comment: {Comment}", taggingResult.Comment);
                    }

                    return taggingResult.Tags
                        .OrderByDescending(t => t.Probability)
                        .Select(t => t.Tag)
                        .ToList();
                }

                _logger.LogWarning("LLM returned invalid format on attempt {Attempt}/{MaxRetries}", attempt, MaxRetries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating LLM tags on attempt {Attempt}/{MaxRetries} for article: {Title}", 
                    attempt, MaxRetries, article.Title);
            }

            if (attempt < MaxRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
            }
        }

        // Fallback to basic tags from categories after all retries exhausted
        _logger.LogWarning("All LLM tagging attempts failed for article: {Title}. Using fallback strategy.", article.Title);
        return article.Categories.ToList();
    }

    private TaggingResult? ParseTaggingResponse(string content)
    {
        try
        {
            // Try to extract JSON from the response (in case there's extra text)
            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<TaggingResult>(jsonContent, options);
                return result;
            }

            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse LLM response as JSON: {Content}", content);
            return null;
        }
    }
}
