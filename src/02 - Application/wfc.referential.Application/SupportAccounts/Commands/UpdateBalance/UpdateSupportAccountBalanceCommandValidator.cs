using FluentValidation;

namespace wfc.referential.Application.SupportAccounts.Commands.UpdateBalance;

public class UpdateSupportAccountBalanceCommandValidator : AbstractValidator<UpdateSupportAccountBalanceCommand>
{
    public UpdateSupportAccountBalanceCommandValidator()
    {
        RuleFor(x => x.SupportAccountId)
            .NotEqual(Guid.Empty).WithMessage("SupportAccountId cannot be empty");

        RuleFor(x => x.NewBalance)
            .NotNull().WithMessage("New balance is required")
            .GreaterThanOrEqualTo(0).WithMessage("Balance cannot be negative");
    }
}