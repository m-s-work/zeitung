using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Zeitung.Core.Models;
using Zeitung.Worker.Models;
using ArticleDto = Zeitung.Worker.Models.Article;
using Zeitung.Worker.Strategies;

namespace Zeitung.Worker.Tests.Strategies;

[TestFixture]
public class LlmTaggingStrategyTests
{
    [Test]
    public void Constructor_ThrowsException_WhenApiKeyNotConfigured()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LlmTaggingStrategy>>();
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            new LlmTaggingStrategy(loggerMock.Object, configuration));
    }

    [Test]
    public void Constructor_UsesDefaultValues_WhenOptionalConfigurationNotProvided()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LlmTaggingStrategy>>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenRouter:ApiKey"] = "test-key"
            })
            .Build();

        // Act & Assert
        Assert.DoesNotThrow(() =>
            new LlmTaggingStrategy(loggerMock.Object, configuration));
    }
}
