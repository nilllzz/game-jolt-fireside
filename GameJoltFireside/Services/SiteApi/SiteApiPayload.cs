using System.Text.Json;
using GameJoltFireside.Models.User;

namespace GameJoltFireside.Services.SiteApi;

public struct SiteApiPayload
{
    public string ver { get; set; }
    public User? user { get; set; }
    public JsonElement payload { get; set; }
}
