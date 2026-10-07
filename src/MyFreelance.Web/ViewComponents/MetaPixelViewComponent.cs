using Microsoft.AspNetCore.Mvc;

namespace MyFreelance.Web.ViewComponents;

public class MetaPixelViewComponent : ViewComponent
{
    public const string PixelId = "1661311945626039";

    public IViewComponentResult Invoke()
    {
        var path = HttpContext.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            return Content(string.Empty);

        return View(model: PixelId);
    }
}
