namespace GameJoltFireside.Models.Chat;

[ResourceName("Chat/User")]
public sealed class ChatUser : Model<ChatUser>
{
    public string username { get; set; }
    public string display_name { get; set; }
    public string img_avatar { get; set; }
    public bool? is_verified { get; set; }
    public int permission_level { get; set; }
    public string role { get; set; }
}
