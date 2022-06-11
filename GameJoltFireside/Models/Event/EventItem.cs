using System.Text.Json;

namespace GameJoltFireside.Models.Event;

[ResourceName("Event_Item")]
public sealed class EventItem : Model<EventItem>
{
    public string type { get; set; }
    public string action_resource { get; set; }
    public int action_resource_id { get; set; }
    public JsonElement action_resource_model { get; set; }
    public string? from_resource { get; set; }
    public int? from_resource_id { get; set; }
    public JsonElement? from_resource_model { get; set; }
    public string? to_resource { get; set; }
    public int? to_resource_id { get; set; }
    public JsonElement? to_resource_model { get; set; }

    public BaseModel GetActionResource()
        => MakeDynamic(action_resource, action_resource_model);

    public BaseModel GetFromResource()
        => MakeDynamic(from_resource, from_resource_model);
}
