using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Data;
using MyFreelance.Models;

namespace MyFreelance.Pages.People;

[Authorize(Policy = "ManageTeam")]
public class IndexModel(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext) : PageModel
{
    public IList<PersonViewModel> People { get; private set; } = [];

    public class PersonViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public DateTime? HiredAt { get; set; }
        public IList<string> Roles { get; set; } = [];
        public int ActiveTasks { get; set; }
    }

    public async Task OnGetAsync()
    {
        var hiredPeople = await userManager.Users
            .Where(u => u.IsHired)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var taskCounts = await dbContext.WorkTasks
            .Where(t => t.Status != WorkTaskStatus.Completed && t.Status != WorkTaskStatus.Cancelled)
            .GroupBy(t => t.AssignedToUserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        foreach (var person in hiredPeople)
        {
            var roles = await userManager.GetRolesAsync(person);
            People.Add(new PersonViewModel
            {
                Id = person.Id,
                FullName = person.FullName,
                Email = person.Email ?? string.Empty,
                JobTitle = person.JobTitle,
                HiredAt = person.HiredAt,
                Roles = roles,
                ActiveTasks = taskCounts.GetValueOrDefault(person.Id)
            });
        }
    }
}
