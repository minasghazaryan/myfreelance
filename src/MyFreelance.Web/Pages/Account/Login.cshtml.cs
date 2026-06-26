using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Web.Pages.Account;

public class LoginModel(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IAuditService auditService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        if (!ModelState.IsValid) return Page();

        var user = await userManager.FindByEmailAsync(Input.Email);
        if (user?.IsSuspended == true)
        {
            ModelState.AddModelError(string.Empty, "Your account has been suspended.");
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            if (user is not null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await userManager.UpdateAsync(user);
                await auditService.LogAsync(user.Id, null, AuditAction.Login, nameof(ApplicationUser), user.Id, "User logged in");
            }
            return LocalRedirect(returnUrl ?? "/Dashboard/Index");
        }

        ModelState.AddModelError(string.Empty, result.IsLockedOut ? "Account locked. Try again later." : "Invalid login attempt.");
        return Page();
    }
}
