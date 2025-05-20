using FluentValidation;

namespace wfc.referential.Application.PartnerAccounts.Commands.CreatePartnerAccount;

public class CreatePartnerAccountCommandValidator : AbstractValidator<CreatePartnerAccountCommand>
{
    public CreatePartnerAccountCommandValidator()
    {
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