namespace GameJoltFireside.Phoenix;

internal enum ChannelState
{
    Closed,
    Errored,
    Joined,
    Joining,
    Leaving
}

internal enum ChannelEvent
{
    Close,
    Error,
    Join,
    Reply,
    Leave
}

internal enum Transport
{
    Longpoll,
    Websocket
}
