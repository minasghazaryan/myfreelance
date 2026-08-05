using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Infrastructure.Background;

public class YieldAccrualBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<YieldAccrualBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var accrualService = scope.ServiceProvider.GetRequiredService<IYieldAccrualService>();
                await accrualService.ProcessDueAccrualsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Daily yield accrual failed.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
