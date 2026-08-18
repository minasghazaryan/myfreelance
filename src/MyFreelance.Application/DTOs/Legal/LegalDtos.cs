using MyFreelance.Domain.Enums;

namespace MyFreelance.Application.DTOs.Legal;

public record LegalDocumentDto(
    Guid Id,
    LegalDocumentType DocumentType,
    string Title,
    string FileName,
    string DownloadUrl);

public record LegalDocumentListItemDto(
    Guid Id,
    LegalDocumentType DocumentType,
    string Title,
    string FileName,
    string ContentType,
    bool IsActive,
    DateTime CreatedAt);
