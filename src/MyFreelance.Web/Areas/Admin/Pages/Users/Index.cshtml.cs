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

    [BindProperty]
    public CreateAdminFormInput CreateAdminInput { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Search { get; set; }

    public bool CanManageUsers => User.IsInRole(AppRoles.Admin);

    public record UserItem(string Id, string Name, string Email, string Role, bool IsSuspended, bool IsKycApproved, DateTime CreatedAt);

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

    public class CreateAdminFormInput
    {
        [Required, Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password)), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string AdminRole { get; set; } = AppRoles.AdminReadOnly;
    }

    public async Task OnGetAsync(string? search)
    {
        SuccessMessage = TempData["SuccessMessage"] as string;
        ErrorMessage = TempData["ErrorMessage"] as string;
        Search = search;
        await LoadUsersAsync(search);
    }

    public Task<IActionResult> OnPostCreateAsync() => CreateAccountAsync(
        CreateInput.FirstName,
        CreateInput.LastName,
        CreateInput.Email,
        CreateInput.Password,
        CreateInput.ConfirmPassword,
        phoneNumber: CreateInput.PhoneNumber,
        referralCode: CreateInput.ReferralCode,
        isKycApproved: CreateInput.IsKycApproved,
        isPhoneVerified: CreateInput.IsPhoneVerified,
        role: AppRoles.Investor,
        createWallet: true,
        successMessage: user => $"Investor {user.FullName} ({user.Email}) created successfully.");

    public Task<IActionResult> OnPostCreateAdminAsync() => CreateAccountAsync(
        CreateAdminInput.FirstName,
        CreateAdminInput.LastName,
        CreateAdminInput.Email,
        CreateAdminInput.Password,
        CreateAdminInput.ConfirmPassword,
        phoneNumber: null,
        referralCode: null,
        isKycApproved: true,
        isPhoneVerified: true,
        role: CreateAdminInput.AdminRole,
        createWallet: false,
        successMessage: user => $"Admin {user.FullName} ({user.Email}) created with {FormatAdminRole(CreateAdminInput.AdminRole)} access.");

    private async Task<IActionResult> CreateAccountAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string confirmPassword,
        string? phoneNumber,
        string? referralCode,
        bool isKycApproved,
        bool isPhoneVerified,
        string role,
        bool createWallet,
        Func<ApplicationUser, string> successMessage)
    {
        if (!User.IsInRole(AppRoles.Admin))
        {
            TempData["ErrorMessage"] = "Only full admins can create users.";
            return RedirectToPage();
        }

        if (!AppRoles.CreatableAdminRoles.Contains(role) && role != AppRoles.Investor)
        {
            TempData["ErrorMessage"] = "Invalid role selected.";
            return RedirectToPage();
        }

        if (password != confirmPassword)
        {
            TempData["ErrorMessage"] = "Passwords do not match.";
            return RedirectToPage();
        }

        ApplicationUser? referrer = null;
        if (!string.IsNullOrWhiteSpace(referralCode))
        {
            referrer = await userManager.Users.FirstOrDefaultAsync(u => u.ReferralCode == referralCode.Trim());
            if (referrer is null)
            {
                TempData["ErrorMessage"] = "Referral code not found.";
                return RedirectToPage();
            }
        }

        var normalizedEmail = email.Trim();
        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            TempData["ErrorMessage"] = "A user with this email already exists.";
            return RedirectToPage();
        }

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            ReferredByUserId = referrer?.Id,
            CountryCode = "GH",
            IsKycApproved = isKycApproved,
            IsPhoneVerified = isPhoneVerified
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToPage();
        }

        await userManager.AddToRoleAsync(user, role);

        if (createWallet)
        {
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
        }

        TempData["SuccessMessage"] = successMessage(user);
        return RedirectToPage();
    }

    private async Task LoadUsersAsync(string? search)
    {
        var query = userManager.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                u.Email!.Contains(search)
                || u.FirstName.Contains(search)
                || u.LastName.Contains(search));
        }

        var users = await query.OrderByDescending(u => u.CreatedAt).Take(100).ToListAsync();
        Users = [];

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var role = roles.Contains(AppRoles.Admin) ? "Full Admin"
                : roles.Contains(AppRoles.AdminReadOnly) ? "Read-Only Admin"
                : roles.Contains(AppRoles.Investor) ? "Investor"
                : roles.FirstOrDefault() ?? "—";

            Users.Add(new UserItem(
                user.Id,
                user.FullName,
                user.Email!,
                role,
                user.IsSuspended,
                user.IsKycApproved,
                user.CreatedAt));
        }
    }

    private static string FormatAdminRole(string role) =>
        role == AppRoles.Admin ? "full admin" : "read-only admin";
}
