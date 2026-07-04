using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;

namespace MyFreelance.Web.Pages.Dashboard;

public class VerifyPhoneModel(
    IPhoneVerificationService phoneService,
    UserManager<ApplicationUser> userManager,
    ILogger<VerifyPhoneModel> logger) : PageModel
{
    [BindProperty] public string PhoneNumber { get; set; } = string.Empty;
    [BindProperty] public string OtpCode { get; set; } = string.Empty;
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public bool ShowOtp { get; set; }
    public bool IsAlreadyVerified { get; set; }
    public string? VerifiedPhoneNumber { get; set; }

    public async Task OnGetAsync() => await LoadUserStateAsync();

    public async Task<IActionResult> OnPostSendAsync()
    {
        await LoadUserStateAsync();
        if (IsAlreadyVerified)
            return Page();

        try
        {
            await phoneService.SendOtpAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, PhoneNumber);
            ShowOtp = true;
            Message = "Verification code sent to your WhatsApp.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send WhatsApp OTP to {PhoneNumber}", PhoneNumber);
            IsError = true;
            Message = ex.Message;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostVerifyAsync()
    {
        await LoadUserStateAsync();
        if (IsAlreadyVerified)
            return Page();

        var verified = await phoneService.VerifyOtpAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, OtpCode);
        if (verified)
        {
            Message = "Phone verified successfully!";
            ShowOtp = false;
        }
        else
        {
            logger.LogWarning("Invalid or expired OTP verification attempt for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
            IsError = true;
            Message = "Invalid or expired verification code.";
            ShowOtp = true;
        }

        await LoadUserStateAsync();
        return Page();
    }

    private async Task LoadUserStateAsync()
    {
        ViewData["ActiveNav"] = "phone";
        var user = await userManager.GetUserAsync(User);
        if (user is not { IsPhoneVerified: true })
            return;

        IsAlreadyVerified = true;
        VerifiedPhoneNumber = user.PhoneNumber;
    }
}
