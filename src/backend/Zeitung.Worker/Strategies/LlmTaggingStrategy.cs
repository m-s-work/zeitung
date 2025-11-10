using System.ClientModel;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;
using Polly;
using Polly.Retry;
using Zeitung.Worker.Models;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Strategies;

public class LlmTaggingStrategy : ITaggingStrategy
{
    private readonly ILogger<LlmTaggingStrategy> _logger;
    private readonly ChatClient _chatClient;
    private readonly ITagRepository? _tagRepository;
    private readonly bool _includeExistingTags;
    private readonly double _minimumProbability;
    private readonly AsyncRetryPolicy _retryPolicy;

    public LlmTaggingStrategy(
        ILogger<LlmTaggingStrategy> logger,
        IConfiguration configuration,
        ITagRepository? tagRepository = null)
    {
        _logger = logger;
        _tagRepository = tagRepository;
        
        var apiKey = configuration["OpenRouter:ApiKey"] ?? throw new InvalidOperationException("OpenRouter:ApiKey not configured");
        var apiUrl = configuration["OpenRouter:ApiUrl"] ?? "https://openrouter.ai/api/v1/chat/completions";
        var model = configuration["OpenRouter:Model"] ?? "meta-llama/llama-3.1-8b-instruct:free";
        
        _includeExistingTags = configuration.GetValue<bool>("OpenRouter:IncludeExistingTags", false);
        _minimumProbability = configuration.GetValue<double>("OpenRouter:MinimumTagProbability", 0.7);

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(apiUrl)
        };

        var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey), options);
        _chatClient = openAiClient.GetChatClient(model);

        // Configure Polly retry policy with exponential backoff
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, attempt, context) =>
                {
                    _logger.LogWarning(exception, "Retry attempt {Attempt} after {Delay}s", attempt, timeSpan.TotalSeconds);
                });
    }

    public async Task<List<string>> GenerateTagsAsync(Article article, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var existingTags = _includeExistingTags && _tagRepository != null
                    ? await _tagRepository.GetAllTagsAsync(cancellationToken)
                    : new List<string>();

                var existingTagsContext = existingTags.Count > 0
                    ? $"\n\nExisting tags in the system (prefer using these when relevant): {string.Join(", ", existingTags.Take(50))}"
                    : "";

                var prompt = $@"Extract 5-10 relevant tags from this article. Return ONLY a valid JSON object in this exact format:
{{{{
  ""tags"": [
    {{{{ ""tag"": ""technology"", ""probability"": 0.95 }}}},
    {{{{ ""tag"": ""science"", ""probability"": 0.87 }}}}
  ],
  ""comment"": ""Brief explanation of tag selection (optional)"",
  ""error"": null
}}}}

IMPORTANT RULES for tag creation:
1. Use only singular forms (e.g., ""technology"" not ""technologies"")
2. Use lowercase for tags
3. Prefer existing tags when relevant
4. Only include tags with high confidence (probability >= 0.7)
5. Be specific but not overly detailed
6. Avoid generic tags like ""news"" or ""article""
{existingTagsContext}

Title: {article.Title}
Description: {article.Description}

Return only the JSON object, no additional text.";

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You are a precise tagging system that returns only valid JSON responses. The 'error' field should be null if successful, or a string describing any issue encountered."),
                    new UserChatMessage(prompt)
                };

                var response = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
                var content = response.Value.Content[0].Text;

                // Try to parse the JSON response
                var taggingResult = ParseTaggingResponse(content);

                if (taggingResult == null || taggingResult.Tags.Count == 0)
                {
                    throw new InvalidOperationException("LLM returned invalid format or no tags");
                }

                if (!string.IsNullOrWhiteSpace(taggingResult.Error))
                {
                    _logger.LogWarning("LLM reported error: {Error}", taggingResult.Error);
                }

                if (!string.IsNullOrWhiteSpace(taggingResult.Comment))
                {
                    _logger.LogInformation("LLM tagging comment: {Comment}", taggingResult.Comment);
                }

                // Filter by minimum probability and return tags
                var filteredTags = taggingResult.Tags
                    .Where(t => t.Probability >= _minimumProbability)
                    .OrderByDescending(t => t.Probability)
                    .Select(t => t.Tag)
                    .ToList();

                if (filteredTags.Count == 0)
                {
                    _logger.LogWarning("No tags met the minimum probability threshold of {Threshold}", _minimumProbability);
                    throw new InvalidOperationException($"No tags met minimum probability threshold of {_minimumProbability}");
                }

                return filteredTags;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "All LLM tagging attempts failed for article: {Title}. Using fallback strategy.", article.Title);
            // Fallback to basic tags from categories after all retries exhausted
            return article.Categories.ToList();
        }
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
