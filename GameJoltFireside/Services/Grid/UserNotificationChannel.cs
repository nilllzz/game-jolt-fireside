using System.Text.Json;
using GameJoltFireside.Config;
using GameJoltFireside.Models.Event;
using GameJoltFireside.Phoenix;

namespace GameJoltFireside.Services.Grid;

public sealed class UserNotificationChannel : BaseChannel<EmptyPayload>, IGridChannel
{
    public event Action<NewNotificationPayload>? NewNotification;

    private UserNotificationChannel(Channel channel)
        : base(channel)
    {
        Joined += OnJoined;
    }

    internal static UserNotificationChannel Make(Socket socket, UserConfig? user = null)
    {
        if (user == null)
        {
            user = UserConfig.Instance;
        }

        if (user.IsGuest)
        {
            throw new Exception("User must be set.");
        }

        var chanParams = new Dictionary<string, object>()
        {
            { "auth_token", user.AuthCookie! },
        };

        var channel = socket.Channel("notifications:" + user.UserId, chanParams);
        return new(channel);
    }

    private void OnJoined(EmptyPayload response)
    {
        // Hook up channel events.
        _channel.On("new-notification", (payload) =>
        {
            if (NewNotification != null)
            {
                var payloadObj = JsonSerializer.Deserialize<NewNotificationPayload>(payload);
                NewNotification.Invoke(payloadObj);
            }
        });
    }

    public sealed class NewNotificationPayload
    {
        public NewNotificationNotificationData notification_data { get; set; }
    }

    public sealed class NewNotificationNotificationData
    {
        public EventItem event_item { get; set; }
    }
}
