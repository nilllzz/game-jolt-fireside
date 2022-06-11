namespace GameJoltFireside.Config;

public sealed class PlatformConfig
{
    private static readonly PlatformConfig _instance = new();
    public static PlatformConfig Instance => _instance;

    public string Platform { get; set; } = "Client";
    public string Version { get; set; } = "1.8.0";
}
