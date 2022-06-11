using System.Text.Json;
using GameJoltFireside.Phoenix;

namespace GameJoltFireside.Services;

abstract public class BaseChannel<JoinT>
{
    private protected readonly Channel _channel;

    protected event Action<JoinT>? Joined;

    internal BaseChannel(Channel channel)
    {
        _channel = channel;
    }

    public Task<JoinT> Join()
    {
        var promise = new TaskCompletionSource<JoinT>();

        _channel.Join()
            .Receive("error", response =>
            {
                promise.SetException(new Exception("Failed to join channel."));
            })
            .Receive("ok", response =>
            {
                var parsed = JsonSerializer.Deserialize<JoinT>(response);

                Joined?.Invoke(parsed);
                promise.SetResult(parsed);
            });

        return promise.Task;
    }

    public void Disconnect()
    {
        _channel.Socket.Remove(_channel);
    }
}
