using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Infrastructure.Services;

public class FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger) : IFileStorageService
{
    private readonly string _basePath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

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
        var fullPath = Path.Combine(_basePath, storedPath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<bool> ScanForVirusAsync(string storedPath, CancellationToken cancellationToken = default)
    {
        // Hook for ClamAV or cloud virus scanning integration
        logger.LogDebug("Virus scan hook invoked for {Path}", storedPath);
        return Task.FromResult(true);
    }
}
