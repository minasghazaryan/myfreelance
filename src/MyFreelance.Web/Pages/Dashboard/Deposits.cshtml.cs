using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Deposits;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Dashboard;

public class DepositsModel(IDepositService depositService) : PageModel
{
    public IReadOnlyList<DepositNetworkDto> Networks { get; set; } = [];
    public IReadOnlyList<DepositDto> Deposits { get; set; } = [];

    [BindProperty] public Guid NetworkId { get; set; }
    [BindProperty] public decimal Amount { get; set; }
    [BindProperty] public string? TransactionHash { get; set; }

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["ActiveNav"] = "deposits";
        SuccessMessage = TempData["SuccessMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadPageDataAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ViewData["ActiveNav"] = "deposits";

        if (NetworkId == Guid.Empty)
            return RedirectWithError("Please select a deposit network.");

        if (Amount <= 0)
            return RedirectWithError("Deposit amount must be greater than zero.");

        try
        {
            await depositService.CreateDepositAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                new CreateDepositDto(NetworkId, Amount, TransactionHash));

            TempData["SuccessMessage"] = "Deposit submitted successfully. It will appear in your balance after admin confirmation.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage();
        }
    }

    private IActionResult RedirectWithError(string message)
    {
        TempData["ErrorMessage"] = message;
        return RedirectToPage();
    }

    private async Task LoadPageDataAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Networks = await depositService.GetActiveNetworksAsync();
        Deposits = await depositService.GetUserDepositsAsync(userId);
    }
}
