using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Withdrawals;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Dashboard;

public class WithdrawalsModel(IWithdrawalService withdrawalService, IDepositService depositService) : PageModel
{
    public IReadOnlyList<WithdrawalDto> Withdrawals { get; set; } = [];

    [BindProperty] public Guid DepositNetworkId { get; set; }
    [BindProperty] public decimal Amount { get; set; }
    [BindProperty] public string WalletAddress { get; set; } = string.Empty;
    [BindProperty] public bool IsImmediate { get; set; }

    public decimal EstimatedPenalty { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["ActiveNav"] = "withdrawals";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Withdrawals = await withdrawalService.GetUserWithdrawalsAsync(userId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ViewData["ActiveNav"] = "withdrawals";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await withdrawalService.RequestWithdrawalAsync(userId,
                new CreateWithdrawalDto(DepositNetworkId, Amount, WalletAddress, IsImmediate));
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }

        Withdrawals = await withdrawalService.GetUserWithdrawalsAsync(userId);
        return Page();
    }
}
