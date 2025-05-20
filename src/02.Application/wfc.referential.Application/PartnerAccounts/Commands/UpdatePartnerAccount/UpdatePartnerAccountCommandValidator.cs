using FluentValidation;

namespace wfc.referential.Application.PartnerAccounts.Commands.UpdatePartnerAccount;

public class UpdatePartnerAccountCommandValidator : AbstractValidator<UpdatePartnerAccountCommand>
{
    public UpdatePartnerAccountCommandValidator()
    {
        RuleFor(x => x.PartnerAccountId)
            .NotEqual(Guid.Empty).WithMessage("PartnerAccountId cannot be empty");

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required");

        RuleFor(x => x.RIB)
            .NotEmpty().WithMessage("RIB is required");

        RuleFor(x => x.BankId)
            .NotEmpty().WithMessage("Bank ID is required");

        RuleFor(x => x.AccountTypeId)
           .NotEmpty().WithMessage("Account type ID is required");
    }
}