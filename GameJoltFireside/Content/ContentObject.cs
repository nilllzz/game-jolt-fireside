using System.Text.Json;

namespace GameJoltFireside.Content;

public sealed class ContentObject
{
    public const string TYPE_TEXT = "text";
    public const string TYPE_PARAGRAPH = "paragraph";

    public string type { get; internal set; }
    public ContentObject[]? content { get; internal set; }
    public Dictionary<string, JsonElement>? attrs { get; internal set; }
    public string? text { get; internal set; }

    public override string ToString()
    {
        var str = "";

        if (content != null && content.Length > 0)
        {
            foreach (var child in content)
            {
                str += child.ToString();
            }
        }

        switch (type)
        {
            case TYPE_TEXT:
                str += text;
                break;
            case TYPE_PARAGRAPH:
                str += "\n";
                break;
        }

        return str;
    }

    internal List<ContentObject> GetObjectsByType(params string[] types)
    {
        var objs = new List<ContentObject>();

        if (types.Contains(type))
        {
            objs.Add(this);
        }

        if (content != null && content.Length > 0)
        {
            foreach (var child in content)
            {
                objs.AddRange(child.GetObjectsByType(types));
            }
        }

        return objs;
    }
}
