using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Infrastructure.Services;

public class FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger) : IFileStorageService
{
    private readonly string _basePath = ResolveBasePath(configuration["FileStorage:Path"]);

    private static string ResolveBasePath(string? configuredPath)
    {
        var path = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(Directory.GetCurrentDirectory(), "uploads")
            : configuredPath;

        return Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var safeName = Path.GetFileName(fileName);
        var dir = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(dir);
        var storedName = $"{Guid.NewGuid():N}_{safeName}";
        var fullPath = Path.Combine(dir, storedName);

        await using var fs = File.Create(fullPath);
        await fileStream.CopyToAsync(fs, cancellationToken);

        logger.LogInformation("File saved: {Path}", fullPath);
        return Path.Combine(folder, storedName).Replace('\\', '/');
    }

    public Task DeleteFileAsync(string storedPath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetAbsolutePath(storedPath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<bool> ScanForVirusAsync(string storedPath, CancellationToken cancellationToken = default)
    {
        // Hook for ClamAV or cloud virus scanning integration
        logger.LogDebug("Virus scan hook invoked for {Path}", storedPath);
        return Task.FromResult(true);
    }

    public string GetAbsolutePath(string storedPath)
        => Path.Combine(_basePath, storedPath.Replace('/', Path.DirectorySeparatorChar));

    public bool FileExists(string storedPath)
        => File.Exists(GetAbsolutePath(storedPath));
}
