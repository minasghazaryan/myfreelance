using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Infrastructure.Persistence;

namespace MyFreelance.Web.Areas.Admin.Pages.Kyc;

public class DocumentModel(ApplicationDbContext db) : PageModel
{
    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var doc = await db.KycDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (doc is null || string.IsNullOrWhiteSpace(doc.FileContentBase64))
            return NotFound();

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(doc.FileContentBase64);
        }
        catch (FormatException)
        {
            return NotFound();
        }

        var contentType = string.IsNullOrWhiteSpace(doc.ContentType) ? "application/octet-stream" : doc.ContentType;
        return File(bytes, contentType, doc.FileName);
    }
}
