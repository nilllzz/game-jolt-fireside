namespace GameJoltFireside.Config;

public sealed class UserConfig
{
    private static readonly UserConfig _instance = new();
    public static UserConfig Instance => _instance;

    public int UserId { get; set; } = -1;
    public string? Username { get; set; } = null;
    public string? AuthCookie { get; set; } = null;

    public bool IsGuest => UserId < 1 ||
        string.IsNullOrEmpty(Username) ||
        string.IsNullOrEmpty(AuthCookie);
}
