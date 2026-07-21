using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Notifications;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Dashboard;

public class NotificationDetailsModel(INotificationService notificationService) : PageModel
{
    public NotificationDetailDto? Notification { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        ViewData["ActiveNav"] = "notifications";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Notification = await notificationService.GetNotificationDetailAsync(userId, id);

        if (Notification is null)
            return Page();

        await notificationService.MarkAsReadAsync(id);
        Notification = Notification with { IsRead = true };

        return Page();
    }
}
