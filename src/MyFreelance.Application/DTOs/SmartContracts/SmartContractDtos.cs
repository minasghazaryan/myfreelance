namespace MyFreelance.Application.DTOs.SmartContracts;

public record SmartContractDashboardDto(decimal TotalAllocatedCapital, int ActiveStrategies, IReadOnlyList<StrategyDistributionDto> StrategyDistribution, IReadOnlyList<PerformanceDataPoint> HistoricalPerformance);
public record StrategyDistributionDto(string Name, decimal AllocationPercent, decimal HistoricalReturnPercent, decimal RiskScore);
public record PerformanceDataPoint(string Label, decimal Value);
