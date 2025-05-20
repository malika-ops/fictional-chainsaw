using FluentValidation;

namespace wfc.referential.Application.PartnerAccounts.Commands.UpdateBalance;

public class UpdateBalanceCommandValidator : AbstractValidator<UpdateBalanceCommand>
{
    public UpdateBalanceCommandValidator()
    {
        RuleFor(x => x.PartnerAccountId)
            .NotEqual(Guid.Empty).WithMessage("PartnerAccountId cannot be empty");

        RuleFor(x => x.NewBalance)
            .NotNull().WithMessage("New balance is required")
            .GreaterThanOrEqualTo(0).WithMessage("Balance cannot be negative");
    }
}