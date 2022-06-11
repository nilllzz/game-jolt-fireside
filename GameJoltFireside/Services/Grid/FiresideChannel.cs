using System.Text.Json;
using GameJoltFireside.Config;
using GameJoltFireside.Models.Fireside;
using GameJoltFireside.Phoenix;

namespace GameJoltFireside.Services.Grid;

public sealed class FiresideChannelJoinResponse
{
    public long server_time { get; internal set; }
}

public class FiresideChannel : BaseChannel<FiresideChannelJoinResponse>, IGridChannel
{
    public event Action<StickerPlacementPayload>? StickerPlacement;

    private FiresideChannel(Channel channel)
        : base(channel)
    {
        Joined += OnJoined;
    }

    internal static FiresideChannel Make(Socket socket, Fireside fireside, UserConfig? user = null)
    {
        if (user == null)
        {
            user = UserConfig.Instance;
        }

        var chanParams = new Dictionary<string, object>();
        if (!user.IsGuest)
        {
            chanParams.Add("user_id", user.UserId.ToString());
            chanParams.Add("auth_token", user.AuthCookie!);
        }

        var channel = socket.Channel("fireside:" + fireside.hash, chanParams);
        return new(channel);
    }

    private void OnJoined(FiresideChannelJoinResponse response)
    {
        // Hook up channel events.
        _channel.On("sticker-placement", (payload) =>
        {
            if (StickerPlacement != null)
            {
                var payloadObj = JsonSerializer.Deserialize<StickerPlacementPayload>(payload);
                StickerPlacement.Invoke(payloadObj);
            }
        });
    }

    public sealed class StickerPlacementPayload
    {
        public Models.Sticker.StickerPlacement sticker_placement { get; set; }
        public object target_data { get; set; }
        public int streak { get; set; }
        public int user_id { get; set; }
    }
}
