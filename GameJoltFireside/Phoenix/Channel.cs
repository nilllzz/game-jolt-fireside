namespace GameJoltFireside.Phoenix;

internal class Channel
{
    #region Constants

    public static Dictionary<ChannelState, string> ChannelStates = new Dictionary<ChannelState, string>
    {
        {ChannelState.Closed, "closed"},
        {ChannelState.Errored, "errored" },
        {ChannelState.Joined, "joined" },
        {ChannelState.Joining, "joining" },
        {ChannelState.Leaving, "leaving" },
    };

    public static Dictionary<ChannelEvent, string> ChannelEvents = new Dictionary<ChannelEvent, string>
    {
        {ChannelEvent.Close, "phx_close" },
        {ChannelEvent.Error, "phx_error" },
        {ChannelEvent.Join, "phx_join" },
        {ChannelEvent.Reply, "phx_reply" },
        {ChannelEvent.Leave, "phx_leave" },
    };

    #endregion

    private ChannelState _state = ChannelState.Closed;
    internal string Topic { get; }
    internal Socket Socket { get; }
    private readonly List<Binding> _bindings = new List<Binding>();
    private readonly int _timeout;
    private bool _joinedOnce;
    private readonly Push _joinPush;
    private readonly List<Push> _pushBuffer = new List<Push>();
    private readonly Timer _rejoinTimer;

    internal Channel(string topic, dynamic @params, Socket socket)
    {
        Topic = topic;
        @params = @params ?? new { };
        Socket = socket;
        _timeout = Socket.Timeout;
        _joinPush = new Push(this, ChannelEvents[ChannelEvent.Join], @params, _timeout);
        _rejoinTimer = new Timer(RejoinUntilConnected, Socket.ReconnectAfterMs);

        _joinPush.Receive("ok", _ =>
        {
            _state = ChannelState.Joined;
            _rejoinTimer.Reset();
            _pushBuffer.ForEach(pushEvent => pushEvent.Send());
            _pushBuffer.Clear();
        });
        OnClose(() =>
        {
            _rejoinTimer.Reset();
            Socket.Log("channel", $"close {Topic}");
            _state = ChannelState.Closed;
            Socket.Remove(this);
        });
        OnError(reason =>
        {
            if (IsLeaving() || IsClosed()) return;
            Socket.Log("channel", $"error {Topic}", reason);
            _state = ChannelState.Errored;
            _rejoinTimer.ScheduleTimeout();
        });
        _joinPush.Receive("timeout", _ =>
        {
            if (!IsJoining()) return;
            Socket.Log("channel", $"timeout {Topic}", _joinPush.Timeout.ToString());
            _state = ChannelState.Errored;
            _rejoinTimer.ScheduleTimeout();
        });
        On(ChannelEvents[ChannelEvent.Reply], (payload, refn) =>
        {
            Trigger(ReplyEventName(refn), payload); // Note: why the refn is not passed here?
        });
    }

    private void RejoinUntilConnected()
    {
        _rejoinTimer.ScheduleTimeout();
        if (Socket.IsConnected())
        {
            Rejoin();
        }
    }

    internal Push Join(int timeout = -1)
    {
        if (timeout == -1) timeout = _timeout;

        if (_joinedOnce)
        {
            throw new Exception(
                "Tried to join multiple times. 'Join' can only be called a single time per channel instance");
        }
        _joinedOnce = true;
        Rejoin(timeout);
        return _joinPush;
    }

    private void OnClose(Action callback)
    {
        On(ChannelEvents[ChannelEvent.Close], callback);
    }

    private void OnError(Action<string> callback)
    {
        On(ChannelEvents[ChannelEvent.Error], reason => callback(reason));
    }

    internal void On(string @event, Action callback)
    {
        On(@event, payload => callback());
    }

    internal void On(string @event, Action<dynamic> callback)
    {
        On(@event, (payload, @ref) => callback(payload));
    }

    internal void On(string @event, Action<dynamic, string> callback)
    {
        _bindings.Add(new Binding { Callback = callback, Event = @event });
    }

    internal void Off(string @event)
    {
        var binding = _bindings.Find(b => b.Event == @event);
        _bindings.Remove(binding);
    }

    private bool CanPush()
    {
        return Socket.IsConnected() && IsJoined();
    }

    internal Push Push(string @event, dynamic payload, int timeout = -1)
    {
        if (timeout == -1) timeout = _timeout;

        if (!_joinedOnce)
        {
            throw new Exception($"Tried to push '{@event}' to '{Topic}' before joining. Use channel.Join() before pushing events");
        }

        var pushEvent = new Push(this, @event, payload, timeout);

        if (CanPush())
        {
            pushEvent.Send();
        }
        else
        {
            pushEvent.StartTimeout();
            _pushBuffer.Add(pushEvent);
        }

        return pushEvent;
    }

    // Leaves the channel
    //
    // Unsubscribes from server events, and
    // instructs channel to terminate on server
    //
    // Triggers onClose() hooks
    //
    // To receive leave acknowledgements, use the a `receive`
    // hook to bind to the server ack, ie:
    //
    //     channel.leave().receive("ok", () => alert("left!") )
    //
    /// <summary>
    /// Leaves the channel
    /// </summary>
    internal Push Leave(int timeout = -1)
    {
        if (timeout == -1) timeout = _timeout;

        _state = ChannelState.Leaving;
        Action onClose = () =>
        {
            Socket.Log("channel", $"leave {Topic}");
            Trigger(ChannelEvents[ChannelEvent.Close], "leave", JoinRef());
        };
        var leavePush = new Push(this, ChannelEvents[ChannelEvent.Leave], new { }, timeout);
        leavePush.Receive("ok", _ => onClose())
            .Receive("timeout", _ => onClose());
        leavePush.Send();

        if (!CanPush())
        {
            leavePush.Trigger("ok", new { });
        }

        return leavePush;
    }

    // Overridable message hook
    //
    // Receives all events for specialized message handling
    // before dispatching to the channel callbacks.
    //
    // Must return the payload, modified or unmodified
    protected virtual dynamic OnMessage(string @event, dynamic payload, string refn)
    {
        return payload;
    }

    // private

    public bool IsMember(string topic)
    {
        return topic == Topic;
    }

    private string JoinRef()
    {
        return _joinPush.Ref;
    }

    private void SendJoin(int timeout)
    {
        _state = ChannelState.Joining;
        _joinPush.Resend(timeout);
    }

    private void Rejoin(int timeout = -1)
    {
        if (timeout == -1) timeout = _timeout;

        if (IsLeaving()) return;

        SendJoin(timeout);
    }

    internal void Trigger(string @event, dynamic payload = null, string @ref = null)
    {
        if (@ref != null && (
            @event == ChannelEvents[ChannelEvent.Close] ||
            @event == ChannelEvents[ChannelEvent.Error] ||
            @event == ChannelEvents[ChannelEvent.Leave] ||
            @event == ChannelEvents[ChannelEvent.Join]) &&
            @ref != JoinRef())
        {
            return;
        }

        var handledPayload = OnMessage(@event, payload, @ref);
        if (payload != null && handledPayload == null)
        {
            throw new Exception("Channel OnMessage callbacks must return the payload, modified or unmodified");
        }

        _bindings.FindAll(binding => binding.Event == @event)
            .ForEach(binding => binding.Callback(handledPayload, @ref));
    }

    internal string ReplyEventName(string @ref)
    {
        return $"chan_reply_{@ref}";
    }

    internal bool IsClosed()
    {
        return _state == ChannelState.Closed;
    }

    internal bool IsErrored()
    {
        return _state == ChannelState.Errored;
    }

    internal bool IsJoined()
    {
        return _state == ChannelState.Joined;
    }

    internal bool IsJoining()
    {
        return _state == ChannelState.Joining;
    }

    internal bool IsLeaving()
    {
        return _state == ChannelState.Leaving;
    }
}
