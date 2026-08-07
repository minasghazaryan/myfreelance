using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Dashboard;

[Authorize(Policy = "InvestorOnly")]
public class FeedbackModel(IFeedbackService feedbackService) : PageModel
{
    public IReadOnlyList<Application.DTOs.Feedback.ClientFeedbackItemDto> MyFeedback { get; set; } = [];

    [BindProperty]
    [Required, StringLength(2000, MinimumLength = 10)]
    public string FeedbackContent { get; set; } = string.Empty;

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["ActiveNav"] = "feedback";
        SuccessMessage = TempData["SuccessMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        MyFeedback = await feedbackService.GetUserFeedbackAsync(UserId());
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        try
        {
            await feedbackService.SubmitFeedbackAsync(UserId(), FeedbackContent);
            TempData["SuccessMessage"] = "Thank you! Your feedback was submitted and is pending review.";
            return RedirectToPage();
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage();
        }
    }

    private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
