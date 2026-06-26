using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.SmartContracts;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Areas.Admin.Pages.SmartContracts;

public class IndexModel(IDashboardService dashboardService) : PageModel
{
    public SmartContractDashboardDto Data { get; set; } = new(0, 0, [], []);
    public async Task OnGetAsync() => Data = await dashboardService.GetSmartContractDashboardAsync();
}
