using MyFreelance.Domain.Common;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Domain.Entities;

public class LegalDocument : BaseEntity
{
    public LegalDocumentType DocumentType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public string StoredPath { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
