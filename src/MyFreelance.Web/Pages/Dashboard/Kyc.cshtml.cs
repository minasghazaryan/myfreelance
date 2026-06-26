using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Kyc;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Web.Pages.Dashboard;

public class KycModel(IKycService kycService, IFileStorageService fileStorage) : PageModel
{
    public KycProfile? Profile { get; set; }

    [BindProperty] public SubmitKycDto Input { get; set; } = new("", "", DateTime.UtcNow.AddYears(-25), "Male", "Ghana", "Ghanaian", "", "", "", "", "");
    [BindProperty] public IFormFile? Passport { get; set; }
    [BindProperty] public IFormFile? NationalId { get; set; }
    [BindProperty] public IFormFile? Selfie { get; set; }
    [BindProperty] public IFormFile? ProofOfAddress { get; set; }

    public string? Message { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["ActiveNav"] = "kyc";
        Profile = await kycService.GetProfileAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ViewData["ActiveNav"] = "kyc";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        try
        {
            Profile = await kycService.SubmitKycAsync(userId, Input);
            await UploadDoc(Passport, DocumentType.Passport, Profile.Id);
            await UploadDoc(NationalId, DocumentType.NationalId, Profile.Id);
            await UploadDoc(Selfie, DocumentType.Selfie, Profile.Id);
            await UploadDoc(ProofOfAddress, DocumentType.ProofOfAddress, Profile.Id);
            Message = "KYC submitted successfully. Awaiting review.";
        }
        catch (Exception ex) { Message = ex.Message; }

        Profile = await kycService.GetProfileAsync(userId);
        return Page();
    }

    private async Task UploadDoc(IFormFile? file, DocumentType type, Guid kycId)
    {
        if (file is null || file.Length == 0) return;
        var path = await fileStorage.SaveFileAsync(file.OpenReadStream(), file.FileName, $"kyc/{kycId}");
        await fileStorage.ScanForVirusAsync(path);
        // Documents would be added via extended KycService - simplified here
    }
}
