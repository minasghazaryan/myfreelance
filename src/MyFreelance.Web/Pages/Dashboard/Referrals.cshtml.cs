using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Referrals;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Dashboard;

public class ReferralsModel(IReferralService referralService) : PageModel
{
    public ReferralTreeDto Tree { get; set; } = new("", "", new(0, 0, 0, 0, 0), []);
    public IReadOnlyList<ReferralConfigDto> Config { get; set; } = [];

    public async Task OnGetAsync()
    {
        ViewData["ActiveNav"] = "referrals";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Tree = await referralService.GetReferralTreeAsync(userId);
        Config = await referralService.GetReferralConfigAsync();
    }
}
