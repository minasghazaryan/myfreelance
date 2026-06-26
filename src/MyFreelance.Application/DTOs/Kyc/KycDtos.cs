namespace MyFreelance.Application.DTOs.Kyc;

public record SubmitKycDto(
    string FirstName, string LastName, DateTime DateOfBirth, string Gender,
    string Country, string Nationality, string Address, string City, string PostalCode,
    string Email, string MobileNumber);

public record KycProfileDto(Guid Id, string UserId, string FullName, string Status, DateTime CreatedAt, string? RejectionReason);
