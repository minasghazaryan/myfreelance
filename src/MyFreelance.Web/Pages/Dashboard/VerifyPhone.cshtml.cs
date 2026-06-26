using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Dashboard;

public class VerifyPhoneModel(IPhoneVerificationService phoneService) : PageModel
{
    [BindProperty] public string PhoneNumber { get; set; } = string.Empty;
    [BindProperty] public string OtpCode { get; set; } = string.Empty;
    public string? Message { get; set; }
    public bool ShowOtp { get; set; }

    public void OnGet() => ViewData["ActiveNav"] = "phone";

    public async Task<IActionResult> OnPostSendAsync()
    {
        ViewData["ActiveNav"] = "phone";
        await phoneService.SendOtpAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, PhoneNumber);
        ShowOtp = true;
        Message = "OTP sent to your phone.";
        return Page();
    }

    public async Task<IActionResult> OnPostVerifyAsync()
    {
        ViewData["ActiveNav"] = "phone";
        var verified = await phoneService.VerifyOtpAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, OtpCode);
        Message = verified ? "Phone verified successfully!" : "Invalid or expired OTP.";
        ShowOtp = !verified;
        return Page();
    }
}
