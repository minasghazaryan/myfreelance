using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Wallet;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Constants;

namespace MyFreelance.Web.Areas.Admin.Pages.Bonuses;

public class IndexModel(IWalletService walletService, UserManager<Domain.Entities.ApplicationUser> userManager) : PageModel
{
    public IReadOnlyList<InvestorOptionDto> Investors { get; set; } = [];
    public IReadOnlyList<BonusAwardDto> RecentBonuses { get; set; } = [];

    [BindProperty]
    public AwardBonusInput Input { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public class AwardBonusInput
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, Range(0.01, 1000000)]
        public decimal Amount { get; set; }

        [Required, StringLength(500, MinimumLength = 3)]
        public string Description { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        SuccessMessage = TempData["SuccessMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        Investors = await walletService.GetInvestorOptionsAsync();
        RecentBonuses = await walletService.GetRecentBonusesAsync();
    }

    public async Task<IActionResult> OnPostAwardAsync()
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        try
        {
            await walletService.AwardBonusAsync(
                Input.UserId,
                Input.Amount,
                Input.Description,
                userManager.GetUserId(User)!);
            TempData["SuccessMessage"] = $"Bonus of ${Input.Amount:N2} credited to the client's available balance.";
            return RedirectToPage();
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage();
        }
    }
}
