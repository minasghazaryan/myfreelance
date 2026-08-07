using Microsoft.AspNetCore.Mvc;
using MyFreelance.Application.DTOs.Dashboard;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Constants;

namespace MyFreelance.Web.ViewComponents;

public class AdminPendingBadgeViewComponent(IDashboardService dashboardService) : ViewComponent
{
    private const string CountsCacheKey = "AdminPendingCounts";

    public async Task<IViewComponentResult> InvokeAsync(string section)
    {
        if (HttpContext.User.Identity?.IsAuthenticated != true
            || (!HttpContext.User.IsInRole(AppRoles.Admin) && !HttpContext.User.IsInRole(AppRoles.AdminReadOnly)))
            return Content(string.Empty);

        var counts = await GetCountsAsync();
        var count = section.ToLowerInvariant() switch
        {
            "kyc" => counts.PendingKyc,
            "deposits" => counts.PendingDeposits,
            "withdrawals" => counts.PendingWithdrawals,
            _ => 0
        };

        return View("~/Pages/Shared/Components/NotificationBadge/Default.cshtml", count);
    }

    private async Task<AdminPendingCountsDto> GetCountsAsync()
    {
        if (HttpContext.Items.TryGetValue(CountsCacheKey, out var cached) && cached is AdminPendingCountsDto counts)
            return counts;

        counts = await dashboardService.GetAdminPendingCountsAsync();
        HttpContext.Items[CountsCacheKey] = counts;
        return counts;
    }
}
