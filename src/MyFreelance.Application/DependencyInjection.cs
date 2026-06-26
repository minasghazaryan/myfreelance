using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyFreelance.Application.Validators;

namespace MyFreelance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SubmitKycValidator>();
        return services;
    }
}
