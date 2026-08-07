using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Deposits;
using MyFreelance.Application.DTOs.Withdrawals;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Dashboard;

public class WithdrawalsModel(IWithdrawalService withdrawalService, IDepositService depositService) : PageModel
{
    public IReadOnlyList<DepositNetworkDto> Networks { get; set; } = [];
    public IReadOnlyList<WithdrawalDto> Withdrawals { get; set; } = [];

    [BindProperty] public Guid DepositNetworkId { get; set; }
    [BindProperty] public decimal Amount { get; set; }
    [BindProperty] public string WalletAddress { get; set; } = string.Empty;
    [BindProperty] public bool IsImmediate { get; set; }

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["ActiveNav"] = "withdrawals";
        SuccessMessage = TempData["SuccessMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        await LoadPageDataAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ViewData["ActiveNav"] = "withdrawals";

        if (DepositNetworkId == Guid.Empty)
            return RedirectWithError("Please select a withdrawal network.");

        if (Amount <= 0)
            return RedirectWithError("Withdrawal amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(WalletAddress))
            return RedirectWithError("Wallet address is required.");

        try
        {
            await withdrawalService.RequestWithdrawalAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                new CreateWithdrawalDto(DepositNetworkId, Amount, WalletAddress.Trim(), IsImmediate));

            TempData["SuccessMessage"] = "Withdrawal request submitted successfully. Awaiting admin approval.";
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
        Withdrawals = await withdrawalService.GetUserWithdrawalsAsync(userId);
    }
}
