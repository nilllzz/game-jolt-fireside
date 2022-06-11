namespace GameJoltFireside.Models.Fireside;

[ResourceName("Fireside")]
public sealed class Fireside : Model<Fireside>
{
    public string hash { get; set; }
    public int chat_room_id { get; set; }
}
