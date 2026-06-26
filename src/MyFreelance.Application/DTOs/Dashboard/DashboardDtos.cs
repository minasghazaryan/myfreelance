namespace MyFreelance.Application.DTOs.Dashboard;

public record PortfolioOverviewDto(decimal CurrentBalance, decimal InvestedCapital, decimal ProjectedEarnings, decimal ReferralEarnings, decimal AvailableBalance);
public record PortfolioAnalyticsDto(IReadOnlyList<ChartDataPoint> Growth, IReadOnlyList<ChartDataPoint> Earnings, IReadOnlyList<ChartDataPoint> Deposits, IReadOnlyList<ChartDataPoint> Withdrawals);
public record ChartDataPoint(string Label, decimal Value);
public record ActivityFeedItemDto(string Type, string Description, decimal Amount, DateTime Timestamp, string Status);
public record AdminDashboardDto(int TotalInvestors, decimal TotalDeposits, decimal TotalWithdrawals, int PendingKyc, int PendingDeposits, int PendingWithdrawals, decimal Revenue);
