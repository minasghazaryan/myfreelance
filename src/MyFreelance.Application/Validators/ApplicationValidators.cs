using FluentValidation;
using MyFreelance.Application.DTOs.Investments;
using MyFreelance.Application.DTOs.Kyc;
using MyFreelance.Application.DTOs.Withdrawals;

namespace MyFreelance.Application.Validators;

public class SubmitKycValidator : AbstractValidator<SubmitKycDto>
{
    public SubmitKycValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth).LessThan(DateTime.UtcNow.AddYears(-18)).WithMessage("You must be at least 18 years old.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.MobileNumber).NotEmpty().MinimumLength(10);
        RuleFor(x => x.Country).NotEmpty();
        RuleFor(x => x.Address).NotEmpty();
    }
}

public class CreateInvestmentValidator : AbstractValidator<CreateInvestmentDto>
{
    public CreateInvestmentValidator()
    {
        RuleFor(x => x.InvestmentTierId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class CreateWithdrawalValidator : AbstractValidator<CreateWithdrawalDto>
{
    public CreateWithdrawalValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.WalletAddress).NotEmpty().MinimumLength(20);
        RuleFor(x => x.DepositNetworkId).NotEmpty();
    }
}
