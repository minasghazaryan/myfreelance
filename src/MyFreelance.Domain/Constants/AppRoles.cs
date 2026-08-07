namespace MyFreelance.Domain.Constants;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string AdminReadOnly = "AdminReadOnly";
    public const string Investor = "Investor";
    public const string Compliance = "Compliance";
    public const string Support = "Support";

    public static IReadOnlyList<string> AdminAreaRoles { get; } = [Admin, AdminReadOnly];

    public static IReadOnlyList<string> CreatableAdminRoles { get; } = [Admin, AdminReadOnly];

    public static bool IsAdminAreaRole(string role) =>
        role is Admin or AdminReadOnly;
}
