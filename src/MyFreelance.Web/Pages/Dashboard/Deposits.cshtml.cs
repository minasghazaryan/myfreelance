using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Deposits;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Dashboard;

public class DepositsModel(IDepositService depositService) : PageModel
{
    private const long MaxReceiptBytes = 10 * 1024 * 1024;

    public IReadOnlyList<DepositNetworkDto> Networks { get; set; } = [];
    public IReadOnlyList<DepositDto> Deposits { get; set; } = [];

    [BindProperty]
    public Guid NetworkId { get; set; }

    [BindProperty]
    public decimal Amount { get; set; }

    [BindProperty]
    public string? TransactionHash { get; set; }

    [BindProperty]
    public IFormFile? Receipt { get; set; }

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

        if (Receipt is null || Receipt.Length == 0)
            return RedirectWithError("Please upload your transaction receipt.");

        if (Receipt.Length > MaxReceiptBytes)
            return RedirectWithError("Receipt file must be 10 MB or smaller.");

        try
        {
            await using var stream = Receipt.OpenReadStream();
            await depositService.CreateDepositFromReceiptAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                new CreateDepositReceiptDto(NetworkId, Amount, TransactionHash, stream, Receipt.FileName, Receipt.ContentType));

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
