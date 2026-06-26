using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Web.Areas.Admin.Pages.Kyc;

public class IndexModel(IKycService kycService, UserManager<ApplicationUser> userManager) : PageModel
{
    public IList<KycItem> Items { get; set; } = [];

    public record KycItem(Guid Id, string UserId, string Name, string Status, DateTime CreatedAt);

    public async Task OnGetAsync()
    {
        var pending = await kycService.GetPendingReviewsAsync();
        Items = pending.Select(k => new KycItem(k.Id, k.UserId, k.FullName, k.Status, k.CreatedAt)).ToList();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        await kycService.UpdateStatusAsync(id, KycStatus.Approved, userManager.GetUserId(User)!);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id, string reason)
    {
        await kycService.UpdateStatusAsync(id, KycStatus.Rejected, userManager.GetUserId(User)!, reason);
        return RedirectToPage();
    }
}
