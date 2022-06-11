using System.Text.Json;

namespace GameJoltFireside.Models;

public abstract class BaseModel
{
    public virtual int id { get; set; }

    private static Dictionary<string, Type>? _modelTypeCache;

    private static string GetResourceNameForType(Type t)
    {
        var attr = (ResourceNameAttribute)t.GetCustomAttributes(typeof(ResourceNameAttribute), false).First();
        return attr.ResourceName;
    }

    internal static BaseModel? MakeDynamic(string resourceName, JsonElement? data)
    {
        if (!data.HasValue)
        {
            return null;
        }

        if (_modelTypeCache == null)
        {
            _modelTypeCache = typeof(BaseModel)
                .Assembly
                .GetTypes()
                .Where(t => !t.IsGenericType && t.IsSubclassOf(typeof(BaseModel)) && t.IsSubclassOf(typeof(Model<>).MakeGenericType(t)))
                .ToDictionary(t => GetResourceNameForType(t), t => t);
        }

        var modelType = _modelTypeCache[resourceName];
        return (BaseModel)JsonSerializer.Deserialize(data.Value, modelType);
    }
}
