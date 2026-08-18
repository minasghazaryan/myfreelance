using MyFreelance.Application.DTOs.Legal;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Application.Interfaces;

public interface ILegalDocumentService
{
    Task<IReadOnlyList<LegalDocumentDto>> GetActiveDocumentsAsync(CancellationToken cancellationToken = default);
    Task<LegalDocumentDto?> GetActiveDocumentAsync(LegalDocumentType documentType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LegalDocumentListItemDto>> GetAllDocumentsAsync(CancellationToken cancellationToken = default);
    Task<(string StoredPath, string FileName, string ContentType)?> GetDocumentFileAsync(Guid id, CancellationToken cancellationToken = default);
    Task UploadDocumentAsync(
        LegalDocumentType documentType,
        string title,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default);
}
