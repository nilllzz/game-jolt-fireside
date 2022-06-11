namespace GameJoltFireside.Models.Chat;

[ResourceName("Chat_Room")]
public sealed class ChatRoom : Model<ChatRoom>
{
    public string title { get; set; }
    public string type { get; set; }
    public int owner_id { get; set; }
    public long last_message_on { get; set; }

    public ChatUser[] members { get; set; }
    public ChatRole[] roles { get; set; }
}
