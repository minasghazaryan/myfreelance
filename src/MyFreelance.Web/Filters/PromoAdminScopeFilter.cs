using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Domain.Constants;

namespace MyFreelance.Web.Filters;

public class PromoAdminScopeFilter : IAsyncPageFilter
{
    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        var promoOnly = user.IsInRole(AppRoles.PromoAdmin)
            && !user.IsInRole(AppRoles.Admin)
            && !user.IsInRole(AppRoles.AdminReadOnly);

        if (!promoOnly)
        {
            await next();
            return;
        }

        var page = context.RouteData.Values["page"]?.ToString() ?? string.Empty;
        if (page.StartsWith("/PromoAnalytics", StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        context.Result = new RedirectToPageResult("/PromoAnalytics/Index", new { area = "Admin" });
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;
}
