using System.Text.Json;
using GameJoltFireside.Models.Chat;
using GameJoltFireside.Phoenix;

namespace GameJoltFireside.Services.Chat;

public sealed class RoomChannelJoinResponse
{
    public ChatRoom room { get; set; }
    public ChatMessage[] messages { get; set; }
}

public sealed class RoomChannel : BaseChannel<RoomChannelJoinResponse>, IChatChannel
{
    public event Action<ChatMessage>? Message;
    public event Action<int>? MessageRemoved;
    public event Action<ChatMessage>? MessageUpdated;

    private RoomChannel(Channel channel)
        : base(channel)
    {
        Joined += OnJoined;
    }

    internal static RoomChannel Make(Socket socket, int roomId)
    {
        var channel = socket.Channel($"room:{roomId}");
        return new(channel);
    }

    private void OnJoined(RoomChannelJoinResponse response)
    {
        // Hook up channel events.
        _channel.On("message", (payload) =>
        {
            if (Message != null)
            {
                var message = JsonSerializer.Deserialize<ChatMessage>(payload);
                Message.Invoke(message);
            }
        });
        _channel.On("message_remove", (payload) =>
        {
            if (MessageRemoved != null)
            {
                MessageRemoveEventPayload response = JsonSerializer.Deserialize<MessageRemoveEventPayload>(payload);
                MessageRemoved.Invoke(response.id);
            }
        });
        _channel.On("message_update", (payload) =>
        {
            if (MessageUpdated != null)
            {
                var message = JsonSerializer.Deserialize<ChatMessage>(payload);
                MessageUpdated.Invoke(message);
            }
        });
    }

    private class MessageRemoveEventPayload
    {
        public int id { get; set; }
    }
}
