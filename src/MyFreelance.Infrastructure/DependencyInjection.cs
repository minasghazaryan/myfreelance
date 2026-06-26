using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyFreelance.Application;
using MyFreelance.Application.Interfaces;
using MyFreelance.Domain.Interfaces;
using MyFreelance.Infrastructure.Persistence;
using MyFreelance.Infrastructure.Repositories;
using MyFreelance.Infrastructure.Services;

namespace MyFreelance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IDepositService, DepositService>();
        services.AddScoped<IWithdrawalService, WithdrawalService>();
        services.AddScoped<IInvestmentService, InvestmentService>();
        services.AddScoped<IKycService, KycService>();
        services.AddScoped<IReferralService, ReferralService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPhoneVerificationService, PhoneVerificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ICmsService, CmsService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        services.AddApplication();

        return services;
    }
}
