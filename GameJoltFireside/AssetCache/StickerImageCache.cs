using GameJoltFireside.Models.Chat;
using GameJoltFireside.Models.Sticker;

namespace GameJoltFireside.AssetCache;

public sealed class StickerImageCache : AssetFileCache
{
    protected override string CacheFolderName => "stickers";

    public async Task<CachedFile[]> GetFiles(ChatMessage message)
    {
        // Only process sticker messages.
        if (message.type != "sticker")
        {
            return Array.Empty<CachedFile>();
        }

        var doc = message.GetMessageDocument();
        var stickers = doc.GetObjectsByType("sticker");

        var imageUrls = new List<string>();

        foreach (var stickerObj in stickers)
        {
            var stickerId = stickerObj.attrs["id"].GetInt32();
            var hydration = doc.GetHydration("sticker-id", stickerId.ToString());

            var stickerModel = Sticker.Make(hydration.data);

            var stickerImageUrl = stickerModel.img_url;
            if (!imageUrls.Contains(stickerImageUrl))
            {
                imageUrls.Add(stickerImageUrl);
            }
        }

        return await MaybeDownloadFiles(imageUrls);
    }
}
