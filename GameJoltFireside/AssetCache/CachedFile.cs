namespace GameJoltFireside.AssetCache;

public sealed class CachedFile
{
    public string OriginUrl { get; }
    public string FilePath { get; }

    internal CachedFile(string originUrl, string filePath)
    {
        OriginUrl = originUrl;
        FilePath = filePath;
    }
}
