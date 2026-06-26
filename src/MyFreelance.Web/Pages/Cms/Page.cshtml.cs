using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Cms;

public class CmsPageModel(ICmsService cmsService) : PageModel
{
    public Application.DTOs.Cms.CmsPageDto? PageContent { get; set; }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        PageContent = await cmsService.GetPageBySlugAsync(slug);
        if (PageContent is null) return NotFound();
        ViewData["Title"] = PageContent.Title;
        return Page();
    }
}
