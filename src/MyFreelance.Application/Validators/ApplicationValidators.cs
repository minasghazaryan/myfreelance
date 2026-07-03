using FluentValidation;
using MyFreelance.Application.DTOs.Investments;
using MyFreelance.Application.DTOs.Kyc;
using MyFreelance.Application.DTOs.Withdrawals;
using MyFreelance.Domain.Enums;

namespace MyFreelance.Application.Validators;

public class SubmitKycValidator : AbstractValidator<SubmitKycDto>
{
    public SubmitKycValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.").MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.").MaximumLength(100);
        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required.")
            .LessThan(DateTime.UtcNow.AddYears(-18)).WithMessage("You must be at least 18 years old.");
        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .Must(g => Enum.TryParse<Gender>(g, true, out _))
            .WithMessage("Please select a valid gender.");
        RuleFor(x => x.Nationality).NotEmpty().WithMessage("Nationality is required.");
        RuleFor(x => x.Country).NotEmpty().WithMessage("Country is required.");
        RuleFor(x => x.City).NotEmpty().WithMessage("City is required.");
        RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.").EmailAddress().WithMessage("Enter a valid email address.");
        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .MinimumLength(10).WithMessage("Mobile number must be at least 10 digits.");
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
