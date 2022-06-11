namespace GameJoltFireside.Models.User;

[ResourceName("User")]
public sealed class User : Model<User>
{
    public string type { get; set; }
    public string username { get; set; }
    public string name { get; set; }
    public string display_name { get; set; }
    public string web_site { get; set; }
    public string url { get; set; }
    public string slug { get; set; }
    public string img_avatar { get; set; }
}
