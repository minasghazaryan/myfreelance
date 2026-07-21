using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.ViewComponents;

public class NotificationBadgeViewComponent(INotificationService notificationService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (HttpContext.User.Identity?.IsAuthenticated != true)
            return Content(string.Empty);

        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Content(string.Empty);

        var unreadCount = await notificationService.GetUnreadCountAsync(userId);
        return View(unreadCount);
    }
}
