using System.Text.Json.Nodes;
using Microsoft.CSharp.RuntimeBinder;

namespace GameJoltFireside.Phoenix;

internal class Push
{
    private readonly Channel _channel;
    private readonly string _event;
    private readonly dynamic _payload;
    private dynamic _receivedResp;
    public int Timeout { get; private set; }
    private Timer _timeoutTimer;
    private readonly List<Hook> _recHooks;
    internal bool Sent { get; private set; }
    public string Ref { get; private set; }
    private string _refEvent;

    /// <summary>
    /// Initializes the Push
    /// </summary>
    /// <param name="channel">The Channel</param>
    /// <param name="event">The event, for example `"phx_join"`</param>
    /// <param name="payload">The payload, for example `{user_id: 123}`</param>
    /// <param name="timeout">The push timeout in milliseconds</param>
    internal Push(Channel channel, string @event, dynamic payload, int timeout)
    {
        _channel = channel;
        _event = @event;
        _payload = payload;
        _receivedResp = null;
        Timeout = timeout;
        _timeoutTimer = null;
        _recHooks = new List<Hook>();
        Sent = false;
    }

    internal void Resend(int timeout)
    {
        Timeout = timeout;
        CancelRefEvent();
        Ref = null;
        _refEvent = null;
        _receivedResp = null;
        Sent = false;
        Send();
    }

    internal void Send()
    {
        StartTimeout();
        Sent = true;
        _channel.Socket.Push(new PushData
        {
            Topic = _channel.Topic,
            Event = _event,
            Payload = PushData.SerializeDynamicPayload(_payload),
            Ref = Ref
        });
    }

    internal Push Receive(string status, Action<JsonObject> callback)
    {
        if (HasReceived(status))
        {
            dynamic response = null;
            try
            {
                response = _receivedResp.response;
            }
            catch (RuntimeBinderException)
            {
                // property doesn't exist
            }
            callback(response);
        }

        _recHooks.Add(new Hook { Status = status, Callback = callback });
        return this;
    }

    // private

    private void MatchReceive(JsonObject payload)
    {
        // ReSharper disable RedundantAssignment
        string status = null;
        JsonObject? response = null;

        try
        {
            status = payload["status"].ToString();
            response = payload["response"] as JsonObject;
        }
        catch (RuntimeBinderException)
        {
            // properties don't exist
            return;
        }

        _recHooks.FindAll(hook => hook.Status == status).ForEach(hook => hook.Callback(response));
    }

    private void CancelRefEvent()
    {
        if (_refEvent == null) return;
        _channel.Off(_refEvent);
    }

    private void CancelTimeout()
    {
        _timeoutTimer?.Reset();
        _timeoutTimer?.Dispose();
        _timeoutTimer = null;
    }

    internal void StartTimeout()
    {
        if (_timeoutTimer != null) return;
        Ref = _channel.Socket.MakeRef();
        _refEvent = _channel.ReplyEventName(Ref);

        _channel.On(_refEvent, (payload, _) =>
        {
            CancelRefEvent();
            CancelTimeout();
            _receivedResp = payload;
            MatchReceive(payload);
        });

        _timeoutTimer = new Timer(() => Trigger("timeout", new { }), _ => Timeout);
        _timeoutTimer.ScheduleTimeout();
    }

    private bool HasReceived(string status)
    {
        try
        {
            return _receivedResp != null && _receivedResp.status == status;
        }
        catch (RuntimeBinderException)
        {
            return false;
        }
    }

    internal void Trigger(string status, dynamic response)
    {
        _channel.Trigger(_refEvent, new { status, response });
    }
}