using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Kyc;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Web.Areas.Admin.Pages.Kyc;

public class DetailsModel(IKycService kycService, UserManager<ApplicationUser> userManager) : PageModel
{
    public KycDetailDto? Detail { get; set; }

    [BindProperty]
    public string? RejectionReason { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Detail = await kycService.GetProfileByIdAsync(id);
        return Detail is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        await kycService.UpdateStatusAsync(id, KycStatus.Approved, userManager.GetUserId(User)!);
        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id)
    {
        if (string.IsNullOrWhiteSpace(RejectionReason))
        {
            Detail = await kycService.GetProfileByIdAsync(id);
            ModelState.AddModelError(nameof(RejectionReason), "Rejection reason is required.");
            return Page();
        }

        await kycService.UpdateStatusAsync(id, KycStatus.Rejected, userManager.GetUserId(User)!, RejectionReason.Trim());
        return RedirectToPage("./Index");
    }
}
