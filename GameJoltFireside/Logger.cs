#if DEBUG

namespace GameJoltFireside;

internal sealed class Logger
{
    private readonly ILoggable _service;

    internal Logger(ILoggable service)
    {
        _service = service;
    }

    internal void Debug(params string[] messages)
    {
        Console.WriteLine($"[{_service.ServiceName}] " + string.Join(' ', messages));
    }
}

#endif
