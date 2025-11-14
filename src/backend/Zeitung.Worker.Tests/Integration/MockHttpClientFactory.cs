namespace Zeitung.Worker.Tests.Integration;

/// <summary>
/// Mock HttpClientFactory for testing
/// </summary>
internal class MockHttpClientFactory : IHttpClientFactory
{
    private readonly string _content;

    public MockHttpClientFactory(string content)
    {
        _content = content;
    }

    public HttpClient CreateClient(string name)
    {
        var messageHandler = new MockHttpMessageHandler(_content);
        return new HttpClient(messageHandler);
    }
}
