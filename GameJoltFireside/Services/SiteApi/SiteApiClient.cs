using System.Text.Json;
using GameJoltFireside.Config;

namespace GameJoltFireside.Services.SiteApi;

public sealed class SiteApiClient
{
    private const string SITE_API_HOST = "https://gamejolt.com";

    private readonly UserConfig _user;

    public SiteApiClient(UserConfig? user = null)
    {
        if (user == null)
        {
            _user = UserConfig.Instance;
        }
        else
        {
            _user = user;
        }
    }

    private async Task<SiteApiResponse> MakeRequest(HttpMethod method, string endpoint)
    {
        var endpointUrl = SITE_API_HOST + $"/site-api/{endpoint.Trim(' ', '/')}";

        var message = new HttpRequestMessage(method, endpointUrl);
        // Sign request if logged in.
        if (!_user.IsGuest)
        {
            message.Headers.Add("Cookie", $"frontend={_user.AuthCookie}");
        }

        var client = HttpClientProvider.GetClient();
        var response = await client.SendAsync(message);
        return await SiteApiResponse.FromHttpResponse(response);
    }

    public async Task<JsonElement> Get(string endpoint)
    {
        var response = await RequestGet(endpoint);
        if (!response.IsSuccess)
        {
            throw new Exception($"Failed to perform request. Status: {response.Status}, Http code: {response.HttpCode}");
        }
        return response.Payload.payload;
    }

    public Task<SiteApiResponse> RequestGet(string endpoint)
        => MakeRequest(HttpMethod.Get, endpoint);
}
