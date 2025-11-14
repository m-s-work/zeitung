namespace Zeitung.Worker.Tests.Integration;

/// <summary>
/// Mock HttpMessageHandler for testing
/// </summary>
internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly string _content;

    public MockHttpMessageHandler(string content)
    {
        _content = content;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(_content)
        });
    }
}
