using GameJoltFireside.Config;
using GameJoltFireside.Models.Fireside;
using GameJoltFireside.Phoenix;

namespace GameJoltFireside.Services.Grid;

public sealed class GridClient : ILoggable
{
    private const string GRID_SERVER = "https://grid.gamejolt.com";

    private readonly PlatformConfig _platform;
    private readonly UserConfig _user;
    private Socket _socket;

#if DEBUG
    private readonly Logger _logger;
#endif

    string ILoggable.ServiceName => "GRID";

    public GridClient(PlatformConfig? platform = null, UserConfig? user = null)
    {
#if DEBUG
        _logger = new(this);
#endif
        if (platform == null)
        {
            _platform = PlatformConfig.Instance;
        }
        else
        {
            _platform = platform;
        }
        if (user == null)
        {
            _user = UserConfig.Instance;
        }
        else
        {
            _user = user;
        }
    }

    public async Task Connect()
    {
        // Get the websocket url of the host to connect to.
        var websocketHost = await GetWebsocketHost();
#if DEBUG
        _logger.Debug("Got websocket host:", websocketHost);
#endif

        var urlParams = new Dictionary<string, string>()
        {
            { "gj_platform", _platform.Platform },
            { "gj_platform_version", _platform.Version },
        };

        // Create a socket and connect.
        var websocketHostUrl = websocketHost + "/grid/socket";
        _socket = new Socket(
            endPoint: websocketHostUrl,
            timeout: 10_000,
            heartbeatIntervalMs: 30_000,
            urlparams: urlParams
#if DEBUG
, logger: Socket_Log
#endif
        );
#if DEBUG
        _socket.Opened += Socket_Opened;
        _socket.Closed += Socket_Closed;
#endif
        _socket.Connect();
    }

#if DEBUG
    private void Socket_Log(string kind, string msg, string data)
    {
        _logger.Debug(kind, msg, data);
    }
    private void Socket_Closed(object sender, System.EventArgs e)
    {
        _logger.Debug("CLOSED");
    }
    private void Socket_Opened(object sender, System.EventArgs e)
    {
        _logger.Debug("OPENED");
    }
#endif

    private static async Task<string> GetWebsocketHost()
    {
        var client = HttpClientProvider.GetClient();
        return await client.GetStringAsync($"{GRID_SERVER}/grid/host");
    }

    #region Channels

    public FiresideChannel CreateChannelFireside(Fireside fireside)
        => FiresideChannel.Make(_socket, fireside, _user);

    public UserNotificationChannel CreateChannelNotifications()
        => UserNotificationChannel.Make(_socket, _user);

    #endregion
}
