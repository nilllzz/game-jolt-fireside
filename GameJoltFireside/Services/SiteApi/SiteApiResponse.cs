using System.Text.Json;

namespace GameJoltFireside.Services.SiteApi;

public enum SiteApiRequestStatus
{
    Success,

    HttpNoResponse,
    HttpFailure,

    PayloadParseFailure,
    InvalidPayload,
}

public class SiteApiResponse
{
    internal static async Task<SiteApiResponse> FromHttpResponse(HttpResponseMessage? httpResponse)
    {
        if (httpResponse == null)
        {
            return Create(SiteApiRequestStatus.HttpNoResponse, -1);
        }
        else if (!httpResponse.IsSuccessStatusCode)
        {
            return Create(SiteApiRequestStatus.HttpFailure, (int)httpResponse.StatusCode);
        }

        try
        {
            // Read the content from the http response.
            var responseStr = await httpResponse.Content.ReadAsStringAsync();
            // Try to deserialize into a payload object.
            var payload = JsonSerializer.Deserialize<SiteApiPayload>(responseStr);

            // Has to return a payload version.
            if (!int.TryParse(payload.ver, out var _))
            {
                return Create(SiteApiRequestStatus.InvalidPayload, (int)httpResponse.StatusCode);
            }

            var response = Create(SiteApiRequestStatus.Success, (int)httpResponse.StatusCode);
            response.Payload = payload;
            return response;
        }
        catch (Exception)
        {
            return Create(SiteApiRequestStatus.PayloadParseFailure, (int)httpResponse.StatusCode);
        }
    }

    private static SiteApiResponse Create(SiteApiRequestStatus status, int httpStatus)
    {
        var response = new SiteApiResponse
        {
            Status = status,
            HttpCode = httpStatus,
        };
        return response;
    }

    public SiteApiRequestStatus Status { get; init; }
    public bool IsSuccess => Status == SiteApiRequestStatus.Success;
    public int HttpCode { get; init; }
    public SiteApiPayload Payload { get; private set; }
}
