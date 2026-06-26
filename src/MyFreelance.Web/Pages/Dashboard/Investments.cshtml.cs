using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Investments;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Dashboard;

public class InvestmentsModel(IInvestmentService investmentService) : PageModel
{
    public IReadOnlyList<InvestmentTierDto> Tiers { get; set; } = [];
    public IReadOnlyList<InvestmentDto> Investments { get; set; } = [];

    [BindProperty]
    public Guid TierId { get; set; }

    [BindProperty]
    public decimal Amount { get; set; }

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["ActiveNav"] = "investments";
        Tiers = await investmentService.GetActiveTiersAsync();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Investments = await investmentService.GetUserInvestmentsAsync(userId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ViewData["ActiveNav"] = "investments";
        Tiers = await investmentService.GetActiveTiersAsync();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        try
        {
            await investmentService.CreateInvestmentAsync(userId, new CreateInvestmentDto(TierId, Amount));
            SuccessMessage = "Investment created successfully.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        Investments = await investmentService.GetUserInvestmentsAsync(userId);
        return Page();
    }
}
