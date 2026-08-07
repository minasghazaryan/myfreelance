using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Domain.Constants;

namespace MyFreelance.Web.Filters;

public class AdminWriteAuthorizationFilter : IAsyncPageFilter
{
    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!string.Equals(context.RouteData.Values["area"]?.ToString(), "Admin", StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        var method = context.HttpContext.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method))
        {
            await next();
            return;
        }

        if (context.HttpContext.User.IsInRole(AppRoles.Admin))
        {
            await next();
            return;
        }

        context.Result = new RedirectToPageResult("/Account/AccessDenied");
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;
}
