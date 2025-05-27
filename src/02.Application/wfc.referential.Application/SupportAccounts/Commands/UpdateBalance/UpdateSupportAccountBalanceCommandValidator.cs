using FluentValidation;

namespace wfc.referential.Application.SupportAccounts.Commands.UpdateBalance;

public class UpdateSupportAccountBalanceCommandValidator : AbstractValidator<UpdateSupportAccountBalanceCommand>
{
    public UpdateSupportAccountBalanceCommandValidator()
    {
        RuleFor(x => x.SupportAccountId)
            .NotEqual(Guid.Empty)
            .WithMessage("SupportAccountId must be a non-empty GUID.");

        RuleFor(x => x.NewBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Balance cannot be negative.");
    }
}