namespace MyFreelance.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string storedPath, CancellationToken cancellationToken = default);
    Task<bool> ScanForVirusAsync(string storedPath, CancellationToken cancellationToken = default);
}
