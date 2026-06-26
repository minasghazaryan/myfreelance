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

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["ActiveNav"] = "deposits";
        Networks = await depositService.GetActiveNetworksAsync();
        Deposits = await depositService.GetUserDepositsAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ViewData["ActiveNav"] = "deposits";
        Networks = await depositService.GetActiveNetworksAsync();
        try
        {
            await depositService.CreateDepositAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                new CreateDepositDto(NetworkId, Amount, TransactionHash));
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }

        Deposits = await depositService.GetUserDepositsAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Page();
    }
}
