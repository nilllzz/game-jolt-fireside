using System.Text.Json;
using System.Web;
using WebSocket4Net;

namespace GameJoltFireside.Phoenix;

/// <summary>
/// WebSocket connection to Phoenix Framework Channels 
/// 
/// Based on commit: dffe05346e1b8b159dfdde418774dba5fed82a3f
/// In https://github.com/phoenixframework/phoenix/blob/master/web/static/js/phoenix.js
/// </summary>
internal class Socket
{
    #region Constants

    internal const string Vsn = "2.0.0";
    internal const int DefaultTimeout = 10000;

    internal static Dictionary<Transport, string> Transports = new Dictionary<Transport, string>
        {
            {Transport.Longpoll, "longpoll" },
            {Transport.Websocket, "websocket" }
        };

    #endregion

    private readonly List<Channel> _channels = new List<Channel>();
    private readonly List<Action> _sendBuffer = new List<Action>();
    private int _ref;
    internal int Timeout { get; private set; }
    private readonly int _heartbeatIntervalMs;
    internal readonly Func<int, int> ReconnectAfterMs = tries => tries > 3 ? 10000 : new[] { 1000, 2000, 5000 }[tries - 1];
    private readonly Action<string, string, string> _logger = (kind, msg, data) => { };
    private readonly Dictionary<string, string> _params = new Dictionary<string, string>();
    private readonly string _endPoint;
    private readonly Timer _reconnectTimer;
    private System.Timers.Timer _heartbeatTimer;

    /// <summary>
    /// Initializes the Socket
    /// </summary>
    /// <param name="endPoint">The string WebSocket endpoint, ie,
    /// "ws://example.com/ws", "wss://example.com", "/ws" (inherited host & protocol)</param>
    /// <param name="timeout">The default timeout in milliseconds to trigger push timeouts. Defaults `DefaultTimeout`</param>
    /// <param name="heartbeatIntervalMs">The millisec interval to send a heartbeat message</param>
    /// <param name="reconnectAfterMs">The optional function that returns the millsec reconnect interval.
    /// Defaults to stepped backoff of:
    /// tries =&gt; tries &gt; 3 ? 10000 : new[] {1000, 2000, 5000}[tries - 1];
    /// </param>
    /// <param name="logger">The optional function for specialized logging, ie:
    /// (kind, msg, data) =&gt; { Trace.WriteLine($"{kind}: {msg}, {data}"); }</param>
    /// <param name="urlparams">The optional params to pass when connecting</param>
    internal Socket(string endPoint, int timeout = DefaultTimeout, int heartbeatIntervalMs = 30000,
        Func<int, int> reconnectAfterMs = null, Action<string, string, string> logger = null,
        dynamic urlparams = null)
    {
        Timeout = timeout;
        _heartbeatIntervalMs = heartbeatIntervalMs;
        ReconnectAfterMs = reconnectAfterMs ?? ReconnectAfterMs;
        _logger = logger ?? _logger;
        _params = urlparams ?? _params;
        _endPoint = $"{endPoint}/{Transports[Transport.Websocket]}";
        _reconnectTimer = new Timer(() =>
        {
            Disconnect(Connect);
        }, ReconnectAfterMs);
    }

    private string EndPointUrl()
    {
        var uriBuilder = new UriBuilder(_endPoint);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        foreach (var p in _params) query.Set(p.Key, p.Value);
        query.Set("vsn", Vsn);
        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString().Replace(":443", "");
        // Note: Removed checking protocol, because this will never get called 
        // from browser so we don't have javascript's "location" object.
    }

    private WebSocket _conn;

    internal void Disconnect(Action callback, int? code = null, string reason = "")
    {
        if (_conn != null)
        {
            _conn.Closed -= OnConnClose; // noop
            if (code != null)
            {
                _conn.Close(code.Value, reason);
            }
            else
            {
                _conn.Close();
            }
            _conn = null;
        }

        callback?.Invoke();
    }

    internal void Connect()
    {
        // Note: Didn't implement deprecated params
        if (_conn != null) return;

        _conn = new WebSocket(EndPointUrl(),
            origin: "https://gamejolt.com"
        );
        _conn.Opened += OnConnOpen;
        _conn.Error += OnConnError;
        _conn.MessageReceived += OnConnMessage;
        _conn.Closed += OnConnClose;
        _conn.Open();

        /*
        while (true)
        {
            Thread.Sleep(5000);
            _conn.Send("{ \"topic\":\"phoenix\",\"event\":\"heartbeat\",\"payload\":{ },\"ref\":\"" + _ref++ + "\"}");
        }
        */
    }

    internal void Log(string kind, string msg, string data = null)
    {
        _logger(kind, msg, data);
    }

    #region Event Handlers

    internal event EventHandler Opened;
    internal event EventHandler Closed;
    internal event EventHandler<SuperSocket.ClientEngine.ErrorEventArgs> Error;
    internal event EventHandler<MessageReceivedEventArgs> Message;

    #endregion

    private void OnConnOpen(object sender, EventArgs eventArgs)
    {
        Log("transport", $"connected to {EndPointUrl()}", Transports[Transport.Websocket]);
        FlushSendBuffer();
        _reconnectTimer.Reset();
        // Note: WebSocket4Net doesn't implement skipHeartbeat so we will send heartbeats always
        _heartbeatTimer?.Stop();
        _heartbeatTimer = new System.Timers.Timer(_heartbeatIntervalMs) { AutoReset = true };
        _heartbeatTimer.Elapsed += (s, a) => SendHeartbeat();
        _heartbeatTimer.Start();
        Opened?.Invoke(sender, eventArgs);
    }

    private void OnConnClose(object sender, EventArgs eventArgs)
    {
        Log("transport", "close", eventArgs.ToString());
        TriggerChanError();
        _heartbeatTimer?.Stop();
        //_reconnectTimer.ScheduleTimeout();
        Closed?.Invoke(sender, eventArgs);
    }

    private void OnConnError(object? sender, SuperSocket.ClientEngine.ErrorEventArgs errorEventArgs)
    {
        Log("transport", "error", errorEventArgs.ToString());
        TriggerChanError();
        Error?.Invoke(sender, errorEventArgs);
    }

    private void TriggerChanError()
    {
        _channels.ForEach(channel => channel.Trigger(Phoenix.Channel.ChannelEvents[ChannelEvent.Error]));
    }

    internal string ConnectionState()
    {
        if (_conn == null) return "closed";
        return _conn.State.ToString().ToLower();
    }

    internal bool IsConnected()
    {
        return ConnectionState() == "open";
    }

    internal void Remove(Channel channel)
    {
        _channels.Remove(channel);
        // TODO: easier way to do this by reference, although not exactly the same as JS Client
    }

    internal Channel Channel(string topic, dynamic chanParams = null)
    {
        chanParams = chanParams ?? EmptyPayload.Instance;
        var chan = new Channel(topic, chanParams, this);
        _channels.Add(chan);
        return chan;
    }

    internal void Push(PushData data)
    {
        Action callback = () => _conn.Send(data.Serialize());
        //Log("push", $"{data.Topic} {data.Event} ({data.Ref})", data.Payload);
        Log("push", data.Serialize());
        if (IsConnected())
        {
            callback();
        }
        else
        {
            _sendBuffer.Add(callback);
        }
    }

    internal string MakeRef()
    {
        var newRef = _ref + 1;
        _ref = newRef == _ref ? 0 : newRef;
        return _ref.ToString();
    }

    internal void SendHeartbeat()
    {
        if (!IsConnected()) return;
        Push(new PushData
        {
            Topic = "phoenix",
            Event = "heartbeat",
            Ref = MakeRef(),
            Payload = PushData.SerializeDynamicPayload(EmptyPayload.Instance),
        });
    }

    private void FlushSendBuffer()
    {
        if (IsConnected() && _sendBuffer.Count > 0)
        {
            _sendBuffer.ForEach(callback => callback());
            _sendBuffer.Clear();
        }
    }

    private void OnConnMessage(object sender, MessageReceivedEventArgs messageReceivedEventArgs)
    {
        var data = PushData.Deserialize(messageReceivedEventArgs.Message);
        data.Ref = data.Ref ?? "";

        Log("receive", $"{data.Topic} {data.Event} " + (string.IsNullOrEmpty(data.Ref) ? "" : $"({data.Ref})"), JsonSerializer.Serialize(data.Payload));
        // Note: Removed Status from Log as Payload is dynamic and it's not very efficient to check if dynamic object has any
        // given key. However if the status is there it's still available through the "data" object in Log function.
        _channels.FindAll(channel => channel.IsMember(data.Topic))
                 .ForEach(channel => channel.Trigger(data.Event, data.Payload, data.Ref));
        Message?.Invoke(sender, messageReceivedEventArgs);
    }
}