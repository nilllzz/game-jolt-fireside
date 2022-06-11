using System.Text.Json;

namespace GameJoltFireside.Models;

abstract public class Model<T> : BaseModel where T : Model<T>
{
    public static T Make(JsonElement data)
        => JsonSerializer.Deserialize<T>(data);
    public static T Make(Dictionary<string, JsonElement> data)
        => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(data));
}
