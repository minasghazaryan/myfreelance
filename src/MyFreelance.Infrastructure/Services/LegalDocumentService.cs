using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Legal;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Infrastructure.Services;

public class LegalDocumentService(ApplicationDbContext db, IFileStorageService fileStorage) : ILegalDocumentService
{
    public async Task<IReadOnlyList<LegalDocumentDto>> GetActiveDocumentsAsync(CancellationToken cancellationToken = default)
        => await db.LegalDocuments
            .Where(d => d.IsActive)
            .OrderBy(d => d.DocumentType)
            .Select(d => new LegalDocumentDto(
                d.Id,
                d.DocumentType,
                d.Title,
                d.FileName,
                $"/Legal/Download/{d.Id}"))
            .ToListAsync(cancellationToken);

    public async Task<LegalDocumentDto?> GetActiveDocumentAsync(LegalDocumentType documentType, CancellationToken cancellationToken = default)
        => await db.LegalDocuments
            .Where(d => d.IsActive && d.DocumentType == documentType)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new LegalDocumentDto(
                d.Id,
                d.DocumentType,
                d.Title,
                d.FileName,
                $"/Legal/Download/{d.Id}"))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<LegalDocumentListItemDto>> GetAllDocumentsAsync(CancellationToken cancellationToken = default)
        => await db.LegalDocuments
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new LegalDocumentListItemDto(
                d.Id,
                d.DocumentType,
                d.Title,
                d.FileName,
                d.ContentType,
                d.IsActive,
                d.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task<(string StoredPath, string FileName, string ContentType)?> GetDocumentFileAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var document = await db.LegalDocuments.FirstOrDefaultAsync(d => d.Id == id && d.IsActive, cancellationToken);
        if (document is null || !fileStorage.FileExists(document.StoredPath))
            return null;

        return (document.StoredPath, document.FileName, document.ContentType);
    }

    public async Task UploadDocumentAsync(
        LegalDocumentType documentType,
        string title,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var trimmedTitle = title.Trim();
        if (string.IsNullOrWhiteSpace(trimmedTitle))
            throw new InvalidOperationException("Document title is required.");

        var storedPath = await fileStorage.SaveFileAsync(fileStream, fileName, "legal", cancellationToken);

        var existing = await db.LegalDocuments
            .Where(d => d.DocumentType == documentType && d.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var doc in existing)
        {
            doc.IsActive = false;
            await fileStorage.DeleteFileAsync(doc.StoredPath, cancellationToken);
        }

        await db.LegalDocuments.AddAsync(new LegalDocument
        {
            DocumentType = documentType,
            Title = trimmedTitle,
            FileName = Path.GetFileName(fileName),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/pdf" : contentType,
            StoredPath = storedPath,
            IsActive = true
        }, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await db.LegalDocuments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        await fileStorage.DeleteFileAsync(document.StoredPath, cancellationToken);
        db.LegalDocuments.Remove(document);
        await db.SaveChangesAsync(cancellationToken);
    }
}
