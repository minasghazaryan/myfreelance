namespace MyFreelance.Application.DTOs.Kyc;

public record SubmitKycDto(
    string FirstName, string LastName, DateTime DateOfBirth, string Gender,
    string Country, string Nationality, string Address, string City, string PostalCode,
    string Email, string MobileNumber);

public record KycProfileDto(Guid Id, string UserId, string FullName, string Status, DateTime CreatedAt, string? RejectionReason, int DocumentCount);

public record KycDocumentDto(Guid Id, string DocumentType, string FileName, string ContentType, long FileSizeBytes, DateTime CreatedAt);

public record KycDetailDto(
    Guid Id,
    string UserId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Gender,
    string Country,
    string Nationality,
    string Address,
    string City,
    string PostalCode,
    string Email,
    string MobileNumber,
    string Status,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<KycDocumentDto> Documents);
