namespace GameJoltFireside.Models.Sticker;

[ResourceName("Sticker_Placement")]
public sealed class StickerPlacement : Model<StickerPlacement>
{
    public Sticker sticker { get; set; }

    public float position_x { get; set; }
    public float position_y { get; set; }
    public float rotation { get; set; }
}
