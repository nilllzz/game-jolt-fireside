namespace GameJoltFireside.Models.Chat;

[ResourceName("Chat/Role")]
public sealed class ChatRole : Model<ChatRole>
{
    public override int id { get => throw new Exception("No id property exists on this model."); set { } }

    public string role { get; set; }
    public int user_id { get; set; }
}
