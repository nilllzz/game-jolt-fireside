namespace GameJoltFireside.Content;

public sealed class ContentDocument
{
    public string version { get; internal set; }
    public long createdOn { get; internal set; }
    public string context { get; internal set; }
    public ContentObject[] content { get; internal set; }
    public HydrationEntry[]? hydration { get; internal set; }

    public override string ToString()
    {
        var str = "";

        if (content.Length > 0)
        {
            foreach (var child in content)
            {
                str += child.ToString();
            }
        }

        str = str.Trim();

        return str;
    }

    public ContentObject[] GetObjectsByType(params string[] types)
    {
        var objs = new List<ContentObject>();

        foreach (var child in content)
        {
            objs.AddRange(child.GetObjectsByType(types));
        }

        return objs.ToArray();
    }

    public HydrationEntry? GetHydration(string type, string source)
    {
        if (hydration == null || hydration.Length == 0)
        {
            return null;
        }

        return hydration.FirstOrDefault(h => h.type == type && h.source == source);
    }
}
