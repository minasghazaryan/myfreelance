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

namespace MyFreelance.Web.Areas.Admin.Pages.Users;

public class IndexModel(
    UserManager<ApplicationUser> userManager,
    IReferralService referralService,
    IUnitOfWork unitOfWork,
    INotificationService notificationService) : PageModel
{
    public IList<UserItem> Users { get; set; } = [];

    [BindProperty]
    public CreateUserInput CreateInput { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Search { get; set; }

    public record UserItem(string Id, string Name, string Email, bool IsSuspended, bool IsKycApproved, DateTime CreatedAt);

    public class CreateUserInput
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

        public bool IsKycApproved { get; set; }

        public bool IsPhoneVerified { get; set; }
    }

    public async Task OnGetAsync(string? search)
    {
        SuccessMessage = TempData["SuccessMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        Search = search;
        await LoadUsersAsync(search);
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(" ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errors) ? "Please check the form and try again." : errors;
            return RedirectToPage();
        }

        ApplicationUser? referrer = null;
        if (!string.IsNullOrWhiteSpace(CreateInput.ReferralCode))
        {
            referrer = await userManager.Users.FirstOrDefaultAsync(u => u.ReferralCode == CreateInput.ReferralCode.Trim());
            if (referrer is null)
            {
                TempData["ErrorMessage"] = "Referral code not found.";
                return RedirectToPage();
            }
        }

        var existing = await userManager.FindByEmailAsync(CreateInput.Email.Trim());
        if (existing is not null)
        {
            TempData["ErrorMessage"] = "A user with this email already exists.";
            return RedirectToPage();
        }

        var user = new ApplicationUser
        {
            UserName = CreateInput.Email.Trim(),
            Email = CreateInput.Email.Trim(),
            EmailConfirmed = true,
            FirstName = CreateInput.FirstName.Trim(),
            LastName = CreateInput.LastName.Trim(),
            PhoneNumber = CreateInput.PhoneNumber.Trim(),
            ReferredByUserId = referrer?.Id,
            CountryCode = "GH",
            IsKycApproved = CreateInput.IsKycApproved,
            IsPhoneVerified = CreateInput.IsPhoneVerified
        };

        var result = await userManager.CreateAsync(user, CreateInput.Password);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToPage();
        }

        await userManager.AddToRoleAsync(user, AppRoles.Investor);
        await referralService.GenerateReferralCodeAsync(user.Id);
        await unitOfWork.Repository<UserWallet>().AddAsync(new UserWallet { UserId = user.Id });
        await unitOfWork.SaveChangesAsync();

        await notificationService.SendEventNotificationAsync(
            user.Id,
            NotificationEventType.Registration,
            new Dictionary<string, string>
            {
                ["Status"] = "Completed",
                ["Description"] = "Your AurumWealth account was created by an administrator."
            });

        TempData["SuccessMessage"] = $"User {user.FullName} ({user.Email}) created successfully.";
        return RedirectToPage();
    }

    private async Task LoadUsersAsync(string? search)
    {
        var query = userManager.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Email!.Contains(search) || u.FirstName.Contains(search) || u.LastName.Contains(search));

        Users = await query.OrderByDescending(u => u.CreatedAt)
            .Take(100)
            .Select(u => new UserItem(u.Id, u.FullName, u.Email!, u.IsSuspended, u.IsKycApproved, u.CreatedAt))
            .ToListAsync();
    }
}
