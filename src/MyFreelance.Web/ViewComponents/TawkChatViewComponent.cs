using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Constants;
using MyFreelance.Domain.Entities;

namespace MyFreelance.Web.ViewComponents;

public class TawkChatViewComponent(
    ICmsService cmsService,
    UserManager<ApplicationUser> userManager) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var settings = await cmsService.GetSupportChatSettingsAsync();
        if (settings is null || !settings.IsEnabled || string.IsNullOrWhiteSpace(settings.ScriptContent))
            return Content(string.Empty);

        var path = HttpContext.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            return Content(string.Empty);

        var isLanding = path == "/" || path.Equals("/Index", StringComparison.OrdinalIgnoreCase);
        var isDashboard = path.StartsWith("/Dashboard", StringComparison.OrdinalIgnoreCase);
        var shouldShow = (isLanding && settings.ShowOnLanding) || (isDashboard && settings.ShowOnDashboard);
        if (!shouldShow)
            return Content(string.Empty);

        TawkChatVisitor? visitor = null;
        if (HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user is not null && !await userManager.IsInRoleAsync(user, AppRoles.Admin)
                && !await userManager.IsInRoleAsync(user, AppRoles.AdminReadOnly))
            {
                var name = string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? "Investor" : user.FullName;
                visitor = new TawkChatVisitor(user.Id, name, user.Email ?? string.Empty, user.PhoneNumber);
            }
        }

        return View(new TawkChatViewModel(settings.ScriptContent, visitor));
    }
}

public record TawkChatViewModel(string ScriptContent, TawkChatVisitor? Visitor);

public record TawkChatVisitor(string UserId, string Name, string Email, string? PhoneNumber);
