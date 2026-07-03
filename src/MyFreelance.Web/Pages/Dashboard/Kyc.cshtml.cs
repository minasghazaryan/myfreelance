using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.DTOs.Kyc;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Web.Pages.Dashboard;

public class KycModel(
    IKycService kycService,
    IValidator<SubmitKycDto> validator,
    UserManager<ApplicationUser> userManager) : PageModel
{
    public KycProfile? Profile { get; set; }

    [BindProperty] public SubmitKycDto Input { get; set; } = new("", "", DateTime.UtcNow.AddYears(-25), "Male", "Ghana", "Ghanaian", "", "", "", "", "");
    [BindProperty] public IFormFile? Passport { get; set; }
    [BindProperty] public IFormFile? NationalId { get; set; }
    [BindProperty] public IFormFile? Selfie { get; set; }
    [BindProperty] public IFormFile? ProofOfAddress { get; set; }

    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["ActiveNav"] = "kyc";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Profile = await kycService.GetProfileAsync(userId);
        await PopulateInputAsync(userId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ViewData["ActiveNav"] = "kyc";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Profile = await kycService.GetProfileAsync(userId);

        await EnsureEmailFromAccountAsync();
        await ValidateAsync();

        if (!ModelState.IsValid)
            return Page();

        try
        {
            Profile = await kycService.SubmitKycAsync(userId, Input);

            var uploadErrors = new List<string>();
            await TryUploadDoc(Passport, DocumentType.Passport, "Passport", Profile.Id, uploadErrors);
            await TryUploadDoc(NationalId, DocumentType.NationalId, "National ID", Profile.Id, uploadErrors);
            await TryUploadDoc(Selfie, DocumentType.Selfie, "Selfie", Profile.Id, uploadErrors);
            await TryUploadDoc(ProofOfAddress, DocumentType.ProofOfAddress, "Proof of address", Profile.Id, uploadErrors);

            if (uploadErrors.Count > 0)
            {
                foreach (var error in uploadErrors)
                    ModelState.AddModelError(string.Empty, error);
                Profile = await kycService.GetProfileAsync(userId);
                return Page();
            }

            SuccessMessage = "KYC submitted successfully. Awaiting review.";
            Profile = await kycService.GetProfileAsync(userId);
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError(string.Empty, GetFriendlyDbError(ex));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return Page();
    }

    private async Task PopulateInputAsync(string userId)
    {
        if (Profile is not null)
        {
            Input = new SubmitKycDto(
                Profile.FirstName,
                Profile.LastName,
                Profile.DateOfBirth,
                Profile.Gender.ToString(),
                Profile.Country,
                Profile.Nationality,
                Profile.Address,
                Profile.City,
                Profile.PostalCode,
                Profile.Email,
                Profile.MobileNumber);
            return;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return;

        Input = Input with
        {
            Email = user.Email ?? "",
            FirstName = string.IsNullOrWhiteSpace(Input.FirstName) ? user.FirstName : Input.FirstName,
            LastName = string.IsNullOrWhiteSpace(Input.LastName) ? user.LastName : Input.LastName,
            MobileNumber = string.IsNullOrWhiteSpace(Input.MobileNumber) ? user.PhoneNumber ?? "" : Input.MobileNumber
        };
    }

    private async Task EnsureEmailFromAccountAsync()
    {
        if (!string.IsNullOrWhiteSpace(Input.Email)) return;

        var user = await userManager.GetUserAsync(User);
        if (!string.IsNullOrWhiteSpace(user?.Email))
            Input = Input with { Email = user.Email };
    }

    private async Task ValidateAsync()
    {
        var validation = await validator.ValidateAsync(Input);
        foreach (var error in validation.Errors)
            ModelState.AddModelError($"Input.{error.PropertyName}", error.ErrorMessage);

        var existingDocs = Profile?.Documents.Select(d => d.DocumentType).ToHashSet() ?? [];

        ValidateDocument(Passport, nameof(Passport), "Passport", DocumentType.Passport, existingDocs);
        ValidateDocument(NationalId, nameof(NationalId), "National ID", DocumentType.NationalId, existingDocs);
        ValidateDocument(Selfie, nameof(Selfie), "Selfie", DocumentType.Selfie, existingDocs);
        ValidateDocument(ProofOfAddress, nameof(ProofOfAddress), "Proof of address", DocumentType.ProofOfAddress, existingDocs);
    }

    private void ValidateDocument(IFormFile? file, string fieldName, string label, DocumentType type, HashSet<DocumentType> existingDocs)
    {
        if (HasFile(file) || existingDocs.Contains(type)) return;

        ModelState.AddModelError(fieldName, $"{label} is required.");
    }

    private static bool HasFile(IFormFile? file) => file is not null && file.Length > 0;

    private async Task TryUploadDoc(IFormFile? file, DocumentType type, string label, Guid kycId, List<string> errors)
    {
        if (!HasFile(file)) return;

        const long maxBytes = 10 * 1024 * 1024;
        if (file!.Length > maxBytes)
        {
            errors.Add($"{label} must be 10 MB or smaller.");
            return;
        }

        try
        {
            await using var stream = file.OpenReadStream();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            var base64 = Convert.ToBase64String(memory.ToArray());

            await kycService.SaveDocumentAsync(
                kycId,
                type,
                file.FileName,
                base64,
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                file.Length);
        }
        catch (Exception ex)
        {
            errors.Add($"{label} could not be saved: {ex.Message}");
        }
    }

    private static string GetFriendlyDbError(DbUpdateException ex)
    {
        var inner = ex.InnerException?.Message ?? ex.Message;
        if (inner.Contains("IX_KycProfiles_UserId", StringComparison.OrdinalIgnoreCase))
            return "A KYC profile already exists for your account.";

        return inner;
    }
}
