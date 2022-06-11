using System.Net.Http.Json;
using System.Text.Json;
using GameJoltFireside.Config;
using GameJoltFireside.Phoenix;

namespace GameJoltFireside.Services.Chat;

public sealed class ChatClient : ILoggable
{
    private const string CHAT_SERVER = "https://chatex.gamejolt.com";

    private readonly UserConfig _user;
    private Socket _socket;

#if DEBUG
    private readonly Logger _logger;
#endif

    string ILoggable.ServiceName => "CHAT";

    public ChatClient(UserConfig? user = null)
    {
#if DEBUG
        _logger = new(this);
#endif
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

        // Get auth token to use instead of the user cookie for auth.
        var token = await GetAuthToken();
#if DEBUG
        _logger.Debug("Got chat auth token.");
#endif

        var urlParams = new Dictionary<string, string>()
        {
            { "token", token },
        };
        // Create a socket and connect.
        var websocketHostUrl = websocketHost;
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
    private void Socket_Log(string kind, string msg, object data)
    {
        _logger.Debug(kind, msg);
    }

    private void Socket_Closed(object sender, System.EventArgs e)
    {
        _logger.Debug("CLOSED");
    }

    private void Socket_Opened(object sneder, System.EventArgs e)
    {
        _logger.Debug("OPENED");
    }
#endif

    private static async Task<string> GetWebsocketHost()
    {
        var client = HttpClientProvider.GetClient();
        return await client.GetStringAsync($"{CHAT_SERVER}/chatex/host");
    }

    private struct AuthTokenResponsePayload
    {
        public string token { get; set; }
    }

    private async Task<string> GetAuthToken()
    {
        // TODO: guest token
        var authPayloadData = new
        {
            auth_token = _user.AuthCookie,
            user_id = _user.UserId,
        };

        var content = JsonContent.Create(authPayloadData, authPayloadData.GetType());
        var client = HttpClientProvider.GetClient();
        var response = await client.PostAsync($"{CHAT_SERVER}/chatex/token", content);
        var responseText = await response.Content.ReadAsStringAsync();

        var responseObj = JsonSerializer.Deserialize<AuthTokenResponsePayload>(responseText);
        return responseObj.token;
    }

    #region Channels

    public RoomChannel CreateChannelRoom(int roomId)
        => RoomChannel.Make(_socket, roomId);

    #endregion
}
