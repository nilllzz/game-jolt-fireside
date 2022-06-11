namespace GameJoltFireside;

internal class HttpClientProvider
{
    private readonly HttpClient _client = new();

    private readonly static HttpClientProvider _instance = new();
    internal static HttpClient GetClient() => _instance._client;
}
