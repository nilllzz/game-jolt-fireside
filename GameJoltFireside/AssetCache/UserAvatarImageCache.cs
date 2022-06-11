using GameJoltFireside.Models.User;

namespace GameJoltFireside.AssetCache;

public sealed class UserAvatarImageCache : AssetFileCache
{
    protected override string CacheFolderName => "user-avatars";

    public async Task<CachedFile?> GetFile(User user)
    {
        var imageUrl = user.img_avatar;
        if (string.IsNullOrEmpty(imageUrl))
        {
            return null;
        }

        return await MaybeDownloadFile(imageUrl);
    }
}
