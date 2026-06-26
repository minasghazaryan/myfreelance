using MyFreelance.Domain.Common;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Domain.Entities;

public class KycProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string Country { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public KycStatus Status { get; set; } = KycStatus.Pending;
    public string? RejectionReason { get; set; }
    public string? ReviewedByAdminId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public ICollection<KycDocument> Documents { get; set; } = [];
}
