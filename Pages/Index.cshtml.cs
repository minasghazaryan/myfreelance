using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Data;
using MyFreelance.Models;

namespace MyFreelance.Pages;

public class IndexModel(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : PageModel
{
    public bool IsAuthenticated { get; private set; }
    public bool CanManage { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public int HiredCount { get; private set; }
    public int ActiveTasks { get; private set; }
    public int MyTasks { get; private set; }
    public int CompletedTasks { get; private set; }

    public async Task OnGetAsync()
    {
        IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
        if (!IsAuthenticated)
        {
            return;
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return;
        }

        UserName = user.FullName;
        CanManage = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);

        if (CanManage)
        {
            HiredCount = await userManager.Users.CountAsync(u => u.IsHired);
            ActiveTasks = await dbContext.WorkTasks.CountAsync(t =>
                t.Status != WorkTaskStatus.Completed && t.Status != WorkTaskStatus.Cancelled);
            CompletedTasks = await dbContext.WorkTasks.CountAsync(t => t.Status == WorkTaskStatus.Completed);
        }
        else
        {
            MyTasks = await dbContext.WorkTasks.CountAsync(t =>
                t.AssignedToUserId == user.Id &&
                t.Status != WorkTaskStatus.Completed &&
                t.Status != WorkTaskStatus.Cancelled);
            CompletedTasks = await dbContext.WorkTasks.CountAsync(t =>
                t.AssignedToUserId == user.Id && t.Status == WorkTaskStatus.Completed);
        }
    }
}
