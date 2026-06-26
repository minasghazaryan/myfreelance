using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Constants;
using MyFreelance.Domain.Entities;
using MyFreelance.Domain.Enums;
using MyFreelance.Domain.Interfaces;

namespace MyFreelance.Web.Pages.Account;

public class RegisterModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IReferralService referralService,
    INotificationService notificationService,
    IUnitOfWork unitOfWork) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReferralCode { get; set; }

    public class InputModel
    {
        [Required, Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password)), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ReferralCode { get; set; }
    }

    public void OnGet(string? refCode)
    {
        ReferralCode = refCode;
        Input.ReferralCode = refCode;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        ApplicationUser? referrer = null;
        if (!string.IsNullOrEmpty(Input.ReferralCode))
            referrer = await userManager.Users.FirstOrDefaultAsync(u => u.ReferralCode == Input.ReferralCode);

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            PhoneNumber = Input.PhoneNumber,
            ReferredByUserId = referrer?.Id,
            CountryCode = "GH"
        };

        var result = await userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        await userManager.AddToRoleAsync(user, AppRoles.Investor);
        await referralService.GenerateReferralCodeAsync(user.Id);
        await unitOfWork.Repository<UserWallet>().AddAsync(new UserWallet { UserId = user.Id });
        await unitOfWork.SaveChangesAsync();

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        // Email confirmation would be sent here via INotificationService

        await notificationService.SendEventNotificationAsync(user.Id, NotificationEventType.Registration);
        await signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToPage("/Dashboard/Index");
    }
}
