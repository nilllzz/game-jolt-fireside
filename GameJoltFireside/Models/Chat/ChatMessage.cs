using System.Text.Json;
using GameJoltFireside.Content;

namespace GameJoltFireside.Models.Chat;

[ResourceName("Chat/Message")]
public sealed class ChatMessage : Model<ChatMessage>
{
    public string type { get; set; }
    public string status { get; set; }
    public long logged_on { get; set; }
    public long? edited_on { get; set; }

    public string content { get; set; }

    public long room_id { get; set; }
    public ChatUser user { get; set; }

    public ContentDocument GetMessageDocument()
        => JsonSerializer.Deserialize<ContentDocument>(content);
}
