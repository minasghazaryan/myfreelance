using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Areas.Admin.Pages.Tiers;

internal static class TierPhotoHelper
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    public const long MaxBytes = 10 * 1024 * 1024;

    public static bool IsAllowedImage(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        return AllowedContentTypes.Contains(file.ContentType)
            || AllowedExtensions.Contains(extension);
    }

    public static async Task<string?> SaveAsync(IFileStorageService fileStorage, IFormFile? photo, CancellationToken cancellationToken = default)
    {
        if (photo is null || photo.Length == 0)
            return null;

        if (photo.Length > MaxBytes)
            throw new InvalidOperationException("Tier photo must be 10 MB or smaller.");

        if (!IsAllowedImage(photo))
            throw new InvalidOperationException("Tier photo must be JPG, PNG, WEBP, or GIF.");

        await using var stream = photo.OpenReadStream();
        return await fileStorage.SaveFileAsync(stream, photo.FileName, "tiers", cancellationToken);
    }

    public static async Task DeleteUploadedAsync(IFileStorageService fileStorage, string? imagePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imagePath)
            || imagePath.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await fileStorage.DeleteFileAsync(imagePath, cancellationToken);
    }
}
