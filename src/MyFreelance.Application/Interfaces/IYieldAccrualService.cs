namespace MyFreelance.Application.Interfaces;

public interface IYieldAccrualService
{
    Task ProcessDueAccrualsAsync(CancellationToken cancellationToken = default);
    Task AccrueInvestmentForDateAsync(Guid investmentId, DateOnly accrualDate, CancellationToken cancellationToken = default);
}
