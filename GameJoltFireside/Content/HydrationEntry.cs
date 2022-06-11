using System.Text.Json;

namespace GameJoltFireside.Content;

public sealed class HydrationEntry
{
    public string type { get; internal set; }
    public string source { get; internal set; }
    public Dictionary<string, JsonElement>? data { get; internal set; }
}
