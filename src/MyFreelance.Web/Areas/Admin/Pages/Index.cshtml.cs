using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Dashboard;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Areas.Admin.Pages;

public class IndexModel(IDashboardService dashboardService) : PageModel
{
    public AdminDashboardDto Stats { get; set; } = new(0, 0, 0, 0, 0, 0, 0);

    public async Task OnGetAsync() => Stats = await dashboardService.GetAdminDashboardAsync();
}
