using MyFreelance.Application.DTOs.Investments;
using MyFreelance.Domain.Entities;

namespace MyFreelance.Application.Interfaces;

public interface IInvestmentService
{
    Task<IReadOnlyList<InvestmentTierDto>> GetActiveTiersAsync(CancellationToken cancellationToken = default);
    Task<Investment> CreateInvestmentAsync(string userId, CreateInvestmentDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvestmentDto>> GetUserInvestmentsAsync(string userId, CancellationToken cancellationToken = default);
}
