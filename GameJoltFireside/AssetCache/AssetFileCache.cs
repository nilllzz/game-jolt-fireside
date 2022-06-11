namespace GameJoltFireside.AssetCache;

public abstract class AssetFileCache
{
    protected abstract string CacheFolderName { get; }

    private string GetCacheFilePath(string originUrl)
    {
        originUrl = originUrl.Trim().ToLower();

        using var sha = System.Security.Cryptography.SHA1.Create();
        var shaedBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(originUrl));
        var sb = new StringBuilder(shaedBytes.Length * 2);
        foreach (var b in shaedBytes)
        {
            sb.Append(b.ToString("X2"));
        }
        var filename = sb.ToString().ToLower();

        var uri = new Uri(originUrl);
        var originExt = Path.GetExtension(uri.AbsolutePath);
        if (!string.IsNullOrEmpty(originExt))
        {
            filename += originExt;
        }


        return Path.Combine(Environment.CurrentDirectory, "cached-assets", CacheFolderName, filename);
    }

    protected async Task<CachedFile> MaybeDownloadFile(string originUrl)
    {
        var files = await MaybeDownloadFiles(new[] { originUrl });
        return files[0];
    }

    protected async Task<CachedFile[]> MaybeDownloadFiles(IEnumerable<string> originUrls)
    {
        var tasks = new List<Task>();
        var client = HttpClientProvider.GetClient();
        var files = originUrls.Select(u => new CachedFile(
            originUrl: u,
            filePath: GetCacheFilePath(u)
        ));

        foreach (var file in files)
        {
            // Only download if the file isn't already cached.
            if (!File.Exists(file.FilePath))
            {
                tasks.Add(Task.Run(async () =>
                {
                    var dir = Path.GetDirectoryName(file.FilePath);
                    Directory.CreateDirectory(dir);

                    var response = await client.GetAsync(file.OriginUrl);
                    using var fs = new FileStream(file.FilePath, FileMode.OpenOrCreate);
                    await response.Content.CopyToAsync(fs);
                }));
            }
        }

        await Task.WhenAll(tasks);

        return files.ToArray();
    }

    public virtual async Task<CachedFile> GetFile(string originUrl)
        => await MaybeDownloadFile(originUrl);
}
