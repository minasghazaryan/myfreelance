namespace MyFreelance.Domain.Constants;

public static class InvestmentConstants
{
    public const int PlanDurationDays = 30;

    public static decimal CalculateDailyYield(decimal amount, decimal yieldPercent)
        => amount * (yieldPercent / PlanDurationDays / 100m);
}
