using System.Text.Json.Nodes;

namespace GameJoltFireside.Phoenix;

internal class Hook
{
    internal string Status { get; set; }
    internal Action<JsonObject?> Callback { get; set; }
}
