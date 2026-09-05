using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyFreelance.Domain.Entities;

namespace MyFreelance.Web.Filters;

public class SuspendedAccountFilter(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : IAsyncPageFilter
{
    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var httpUser = context.HttpContext.User;
        if (httpUser.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        var user = await userManager.GetUserAsync(httpUser);
        if (user is not { IsSuspended: true })
        {
            await next();
            return;
        }

        await signInManager.SignOutAsync();
        context.Result = new RedirectToPageResult("/Account/Login", new { blocked = true });
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;
}
