using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Data;
using MyFreelance.Models;

namespace MyFreelance.Pages.Tasks;

[Authorize]
public class DetailsModel(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : PageModel
{
    public WorkTask TaskItem { get; private set; } = null!;
    public bool CanManage { get; private set; }
    public bool IsAssignee { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var task = await dbContext.WorkTasks
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
        {
            return NotFound();
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToPage("/Account/Login");
        }

        CanManage = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
        IsAssignee = task.AssignedToUserId == user.Id;

        if (!CanManage && !IsAssignee)
        {
            return Forbid();
        }

        TaskItem = task;
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, WorkTaskStatus status)
    {
        var task = await dbContext.WorkTasks.FindAsync(id);
        if (task is null)
        {
            return NotFound();
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToPage("/Account/Login");
        }

        CanManage = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
        IsAssignee = task.AssignedToUserId == user.Id;

        if (!CanManage && !IsAssignee)
        {
            return Forbid();
        }

        if (!CanManage && status == WorkTaskStatus.Cancelled)
        {
            return Forbid();
        }

        task.Status = status;
        await dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Task status updated.";
        return RedirectToPage(new { id });
    }
}
