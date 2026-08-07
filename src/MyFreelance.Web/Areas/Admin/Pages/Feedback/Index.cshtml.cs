using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Constants;

namespace MyFreelance.Web.Areas.Admin.Pages.Feedback;

public class IndexModel(IFeedbackService feedbackService) : PageModel
{
    public IList<Application.DTOs.Feedback.ClientFeedbackDto> Items { get; set; } = [];

    [BindProperty]
    public PublishFeedbackForm PublishInput { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public class PublishFeedbackForm
    {
        public Guid FeedbackId { get; set; }
        public string? DisplayName { get; set; }
        public string? AuthorSubtitle { get; set; }
        public string? Location { get; set; }
    }

    public async Task OnGetAsync()
    {
        SuccessMessage = TempData["SuccessMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        Items = (await feedbackService.GetAllFeedbackAsync()).ToList();
    }

    public async Task<IActionResult> OnPostPublishAsync()
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        try
        {
            await feedbackService.PublishFeedbackAsync(
                PublishInput.FeedbackId,
                PublishInput.DisplayName,
                PublishInput.AuthorSubtitle,
                PublishInput.Location);
            TempData["SuccessMessage"] = "Feedback published on the homepage.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnpublishAsync(Guid id)
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        await feedbackService.UnpublishFeedbackAsync(id);
        TempData["SuccessMessage"] = "Feedback removed from homepage.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        await feedbackService.DeleteFeedbackAsync(id);
        TempData["SuccessMessage"] = "Feedback deleted.";
        return RedirectToPage();
    }
}
