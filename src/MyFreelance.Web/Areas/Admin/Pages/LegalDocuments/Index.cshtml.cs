using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Constants;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Web.Areas.Admin.Pages.LegalDocuments;

public class IndexModel(ILegalDocumentService legalDocumentService) : PageModel
{
    public IList<Application.DTOs.Legal.LegalDocumentListItemDto> Documents { get; set; } = [];

    [BindProperty]
    public UploadInput Input { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public class UploadInput
    {
        public LegalDocumentType DocumentType { get; set; }
        public string Title { get; set; } = string.Empty;
        public IFormFile? File { get; set; }
    }

    public async Task OnGetAsync()
    {
        SuccessMessage = TempData["SuccessMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        Documents = (await legalDocumentService.GetAllDocumentsAsync()).ToList();
    }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        if (Input.File is null || Input.File.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a PDF file to upload.";
            return RedirectToPage();
        }

        if (!Input.File.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
            && !Input.File.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Only PDF files are allowed.";
            return RedirectToPage();
        }

        try
        {
            await using var stream = Input.File.OpenReadStream();
            await legalDocumentService.UploadDocumentAsync(
                Input.DocumentType,
                Input.Title,
                stream,
                Input.File.FileName,
                Input.File.ContentType);

            TempData["SuccessMessage"] = "Document uploaded successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (!User.IsInRole(AppRoles.Admin))
            return Forbid();

        await legalDocumentService.DeleteDocumentAsync(id);
        TempData["SuccessMessage"] = "Document deleted.";
        return RedirectToPage();
    }
}
