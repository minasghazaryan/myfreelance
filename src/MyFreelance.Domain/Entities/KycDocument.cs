using MyFreelance.Domain.Common;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Domain.Entities;

public class KycDocument : BaseEntity
{
    public Guid KycProfileId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredPath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public bool IsVerified { get; set; }

    public KycProfile KycProfile { get; set; } = null!;
}
