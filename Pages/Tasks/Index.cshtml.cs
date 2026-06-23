using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Data;
using MyFreelance.Models;

namespace MyFreelance.Pages.Tasks;

[Authorize]
public class IndexModel(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : PageModel
{
    public IList<TaskViewModel> Tasks { get; private set; } = [];
    public bool CanManage { get; private set; }

    public class TaskViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public WorkTaskStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return;
        }

        CanManage = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);

        var query = dbContext.WorkTasks
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .AsQueryable();

        if (!CanManage)
        {
            query = query.Where(t => t.AssignedToUserId == user.Id);
        }

        Tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                DueDate = t.DueDate,
                AssigneeName = t.AssignedTo.FullName,
                CreatedByName = t.CreatedBy.FullName
            })
            .ToListAsync();
    }
}
