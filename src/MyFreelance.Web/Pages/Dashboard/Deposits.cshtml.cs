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

        if (Receipt is null || Receipt.Length == 0)
            return RedirectWithError("Please upload your transaction receipt.");

        if (Receipt.Length > MaxReceiptBytes)
            return RedirectWithError("Receipt file must be 10 MB or smaller.");

        try
        {
            await using var stream = Receipt.OpenReadStream();
            await depositService.CreateDepositFromReceiptAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                new CreateDepositReceiptDto(stream, Receipt.FileName, Receipt.ContentType));

            TempData["SuccessMessage"] = "Receipt submitted successfully. Your deposit will appear in your balance after admin approval.";
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
