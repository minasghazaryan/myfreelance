namespace MyFreelance.Web;

public static class TierImageUrls
{
    public static string? Resolve(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return null;

        return imagePath.StartsWith("images/", StringComparison.OrdinalIgnoreCase)
            ? $"~/{imagePath.TrimStart('/')}"
            : $"/uploads/{imagePath.TrimStart('/')}";
    }
}
