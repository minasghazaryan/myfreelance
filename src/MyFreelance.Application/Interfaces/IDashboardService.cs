using MyFreelance.Application.DTOs.Dashboard;
using MyFreelance.Application.DTOs.SmartContracts;

namespace MyFreelance.Application.Interfaces;

public interface IDashboardService
{
    Task<PortfolioOverviewDto> GetPortfolioOverviewAsync(string userId, CancellationToken cancellationToken = default);
    Task<PortfolioAnalyticsDto> GetPortfolioAnalyticsAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActivityFeedItemDto>> GetActivityFeedAsync(string userId, int take = 20, CancellationToken cancellationToken = default);
    Task<SmartContractDashboardDto> GetSmartContractDashboardAsync(CancellationToken cancellationToken = default);
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken cancellationToken = default);
}
