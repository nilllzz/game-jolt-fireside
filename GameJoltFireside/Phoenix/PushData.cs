using System.Text.Json;
using System.Text.Json.Nodes;

namespace GameJoltFireside.Phoenix;

internal class PushData
{
    internal string? Ref { get; set; } = null;
    internal string Topic { get; set; } = "";
    internal string Event { get; set; } = "";
    internal JsonObject Payload { get; set; }

    internal string Serialize()
    {
        //[REF,REF,TOPIC                ,EVENT     ,PAYLOAD]
        //["N","N","notifications:XXXXX","phx_join",{"..."}]
        var payloadEncoded = JsonSerializer.Serialize(Payload);
        return $"[\"{Ref}\",\"{Ref}\",\"{Topic}\",\"{Event}\",{payloadEncoded}]";
    }

    internal void SetPayload(dynamic payload)
    {
        Payload = SerializeDynamicPayload(payload);
    }

    internal static JsonObject SerializeDynamicPayload(dynamic payload)
    {
        return JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(payload));
    }

    internal static PushData Deserialize(string rawMessage)
    {
        var rawPayload = JsonSerializer.Deserialize<JsonElement>(rawMessage);

        return new PushData
        {
            Ref = rawPayload[0].GetString(),
            Topic = rawPayload[2].GetString(),
            Event = rawPayload[3].GetString(),

            Payload = SerializeDynamicPayload(rawPayload[4]),
        };
    }
}
