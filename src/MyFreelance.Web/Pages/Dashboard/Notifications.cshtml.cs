using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Notifications;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Dashboard;

public class NotificationsModel(INotificationService notificationService) : PageModel
{
    public IReadOnlyList<NotificationDto> Notifications { get; set; } = [];

    public async Task OnGetAsync()
    {
        ViewData["ActiveNav"] = "notifications";
        Notifications = await notificationService.GetUserNotificationsAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    public async Task<IActionResult> OnPostMarkReadAsync(Guid id)
    {
        await notificationService.MarkAsReadAsync(id);
        return RedirectToPage();
    }
}
