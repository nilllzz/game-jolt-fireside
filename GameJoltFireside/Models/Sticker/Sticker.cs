namespace GameJoltFireside.Models.Sticker;

[ResourceName("Sticker")]
public sealed class Sticker : Model<Sticker>
{
    public string img_url { get; set; }
    public bool is_event { get; set; }
    public int rarity { get; set; }
}
