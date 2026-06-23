using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Data;
using MyFreelance.Models;

namespace MyFreelance.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class RolesModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public IList<UserRoleViewModel> Users { get; private set; } = [];

    [BindProperty]
    public string? SelectedUserId { get; set; }

    [BindProperty]
    public IList<string> SelectedRoles { get; set; } = [];

    public class UserRoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = [];
    }

    public async Task OnGetAsync()
    {
        await LoadUsersAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedUserId))
        {
            ModelState.AddModelError(string.Empty, "Select a user.");
            await LoadUsersAsync();
            return Page();
        }

        var user = await userManager.FindByIdAsync(SelectedUserId);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "User not found.");
            await LoadUsersAsync();
            return Page();
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var desiredRoles = SelectedRoles.Intersect(AppRoles.All).ToList();

        var toRemove = currentRoles.Except(desiredRoles).ToList();
        var toAdd = desiredRoles.Except(currentRoles).ToList();

        if (toRemove.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, toRemove);
        }

        if (toAdd.Count > 0)
        {
            await userManager.AddToRolesAsync(user, toAdd);
        }

        TempData["SuccessMessage"] = $"Roles updated for {user.FullName}.";
        return RedirectToPage();
    }

    private async Task LoadUsersAsync()
    {
        var users = await userManager.Users.OrderBy(u => u.FullName).ToListAsync();
        Users = [];

        foreach (var user in users)
        {
            Users.Add(new UserRoleViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = await userManager.GetRolesAsync(user)
            });
        }
    }
}
