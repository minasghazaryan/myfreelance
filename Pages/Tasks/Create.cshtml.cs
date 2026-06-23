using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Data;
using MyFreelance.Models;

namespace MyFreelance.Pages.Tasks;

[Authorize(Policy = "ManageTeam")]
public class CreateModel(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Assignees { get; private set; } = null!;

    public class InputModel
    {
        [Required, StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Assign to")]
        public string AssignedToUserId { get; set; } = string.Empty;

        [Display(Name = "Due date")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadAssigneesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAssigneesAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return RedirectToPage("/Account/Login");
        }

        var assignee = await userManager.FindByIdAsync(Input.AssignedToUserId);
        if (assignee is null || !assignee.IsHired)
        {
            ModelState.AddModelError(nameof(Input.AssignedToUserId), "Select a hired team member.");
            return Page();
        }

        dbContext.WorkTasks.Add(new WorkTask
        {
            Title = Input.Title,
            Description = Input.Description,
            AssignedToUserId = Input.AssignedToUserId,
            CreatedByUserId = currentUser.Id,
            DueDate = Input.DueDate?.ToUniversalTime(),
            Status = WorkTaskStatus.Pending
        });

        await dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Task assigned successfully.";
        return RedirectToPage("./Index");
    }

    private async Task LoadAssigneesAsync()
    {
        var assignees = await userManager.Users
            .Where(u => u.IsHired)
            .OrderBy(u => u.FullName)
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync();

        Assignees = new SelectList(assignees, "Id", "FullName", Input.AssignedToUserId);
    }
}
