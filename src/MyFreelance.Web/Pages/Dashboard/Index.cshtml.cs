using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Dashboard;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Dashboard;

public class IndexModel(IDashboardService dashboardService) : PageModel
{
    public PortfolioOverviewDto Portfolio { get; set; } = new(0, 0, 0, 0, 0);
    public PortfolioAnalyticsDto Analytics { get; set; } = new([], [], [], []);
    public IReadOnlyList<ActivityFeedItemDto> Activity { get; set; } = [];

    public async Task OnGetAsync()
    {
        ViewData["ActiveNav"] = "dashboard";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Portfolio = await dashboardService.GetPortfolioOverviewAsync(userId);
        Analytics = await dashboardService.GetPortfolioAnalyticsAsync(userId);
        Activity = await dashboardService.GetActivityFeedAsync(userId);
    }
}
