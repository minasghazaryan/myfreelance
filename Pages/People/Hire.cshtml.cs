using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Data;
using MyFreelance.Models;

namespace MyFreelance.Pages.People;

[Authorize(Policy = "ManageTeam")]
public class HireModel(UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Display(Name = "Job title")]
        public string JobTitle { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var existing = await userManager.FindByEmailAsync(Input.Email);
        if (existing is not null)
        {
            existing.FullName = Input.FullName;
            existing.JobTitle = Input.JobTitle;
            existing.IsHired = true;
            existing.HiredAt = DateTime.UtcNow;

            var updateResult = await userManager.UpdateAsync(existing);
            if (!updateResult.Succeeded)
            {
                AddErrors(updateResult);
                return Page();
            }

            if (!await userManager.IsInRoleAsync(existing, AppRoles.Freelancer))
            {
                await userManager.AddToRoleAsync(existing, AppRoles.Freelancer);
            }

            TempData["SuccessMessage"] = $"{existing.FullName} is now on your team.";
            return RedirectToPage("./Index");
        }

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            EmailConfirmed = true,
            FullName = Input.FullName,
            JobTitle = Input.JobTitle,
            IsHired = true,
            HiredAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, AppRoles.Freelancer);
            TempData["SuccessMessage"] = $"{user.FullName} was hired successfully.";
            return RedirectToPage("./Index");
        }

        AddErrors(result);
        return Page();
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
