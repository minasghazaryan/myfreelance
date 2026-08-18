using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Constants;

namespace MyFreelance.Web.Areas.Admin.Pages.Feedback;

public class IndexModel(
    IFeedbackService feedbackService,
    UserManager<Domain.Entities.ApplicationUser> userManager) : PageModel
{
    public IList<Application.DTOs.Feedback.ClientFeedbackDto> Items { get; set; } = [];

    [BindProperty]
    public PublishFeedbackForm PublishInput { get; set; } = new();

    [BindProperty]
    public FeaturedReviewForm FeaturedInput { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public class PublishFeedbackForm
    {
        public Guid FeedbackId { get; set; }
        public string? DisplayName { get; set; }
        public string? AuthorSubtitle { get; set; }
        public string? Location { get; set; }
        public IFormFile? MediaFile { get; set; }
    }

    public class FeaturedReviewForm
    {
        public string Content { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? AuthorSubtitle { get; set; }
        public string? Location { get; set; }
        public IFormFile? MediaFile { get; set; }
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
            Stream? stream = null;
            if (PublishInput.MediaFile is { Length: > 0 })
                stream = PublishInput.MediaFile.OpenReadStream();

            await feedbackService.PublishFeedbackAsync(
                PublishInput.FeedbackId,
                PublishInput.DisplayName,
                PublishInput.AuthorSubtitle,
                PublishInput.Location,
                stream,
                PublishInput.MediaFile?.FileName,
                PublishInput.MediaFile?.ContentType);

            TempData["SuccessMessage"] = "Feedback published on the homepage.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateFeaturedAsync()
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        try
        {
            Stream? stream = null;
            if (FeaturedInput.MediaFile is { Length: > 0 })
                stream = FeaturedInput.MediaFile.OpenReadStream();

            await feedbackService.CreateFeaturedReviewAsync(
                userManager.GetUserId(User)!,
                FeaturedInput.Content,
                FeaturedInput.DisplayName,
                FeaturedInput.AuthorSubtitle,
                FeaturedInput.Location,
                stream,
                FeaturedInput.MediaFile?.FileName,
                FeaturedInput.MediaFile?.ContentType);

            TempData["SuccessMessage"] = "Featured review published on the homepage.";
        }
        catch (Exception ex)
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
