using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Legal;

public class DownloadModel(ILegalDocumentService legalDocumentService, IFileStorageService fileStorage) : PageModel
{
    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var file = await legalDocumentService.GetDocumentFileAsync(id);
        if (file is null)
            return NotFound();

        var absolutePath = fileStorage.GetAbsolutePath(file.Value.StoredPath);
        return PhysicalFile(absolutePath, file.Value.ContentType, file.Value.FileName);
    }
}
