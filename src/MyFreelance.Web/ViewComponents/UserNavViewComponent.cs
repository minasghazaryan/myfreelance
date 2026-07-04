using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Entities;

namespace MyFreelance.Web.ViewComponents;

public class UserNavViewComponent(
    IDashboardService dashboardService,
    UserManager<ApplicationUser> userManager) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user is null)
            return Content(string.Empty);

        var portfolio = await dashboardService.GetPortfolioOverviewAsync(user.Id);
        return View(new UserNavViewModel(
            user.FirstName,
            user.FullName,
            portfolio.CurrentBalance,
            portfolio.AvailableBalance,
            portfolio.InvestedCapital));
    }
}

public record UserNavViewModel(
    string FirstName,
    string FullName,
    decimal CurrentBalance,
    decimal AvailableBalance,
    decimal InvestedBalance);
