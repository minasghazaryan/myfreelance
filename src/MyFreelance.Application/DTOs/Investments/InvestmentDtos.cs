namespace MyFreelance.Application.DTOs.Investments;

public record InvestmentTierDto(
    Guid Id,
    string Name,
    string Description,
    string? PackageDetails,
    string RiskLevel,
    decimal ProjectedYieldPercent,
    decimal MinInvestment,
    decimal MaxInvestment,
    string? AccentColor,
    string? InsuranceNotice);
public record CreateInvestmentDto(Guid InvestmentTierId, decimal Amount);
public record InvestmentDto(
    Guid Id,
    string TierName,
    decimal Amount,
    decimal ProjectedYieldPercent,
    string Status,
    DateTime CreatedAt,
    int AccrualDaysCompleted,
    decimal AccruedAmount);
