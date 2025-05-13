using FluentValidation;

namespace wfc.referential.Application.PartnerAccounts.Commands.PatchPartnerAccount;

public class PatchPartnerAccountCommandValidator : AbstractValidator<PatchPartnerAccountCommand>
{
    public PatchPartnerAccountCommandValidator()
    {
        RuleFor(x => x.PartnerAccountId)
            .NotEqual(Guid.Empty).WithMessage("PartnerAccountId cannot be empty");

        // If account number is provided, check not empty
        When(x => x.AccountNumber is not null, () => {
            RuleFor(x => x.AccountNumber!)
                .NotEmpty().WithMessage("Account number cannot be empty if provided");
        });

        // If RIB is provided, check not empty
        When(x => x.RIB is not null, () => {
            RuleFor(x => x.RIB!)
                .NotEmpty().WithMessage("RIB cannot be empty if provided");
        });

        // If domiciliation is provided, check not empty
        When(x => x.Domiciliation is not null, () => {
            RuleFor(x => x.Domiciliation!)
                .NotEmpty().WithMessage("Domiciliation cannot be empty if provided");
        });

        // If business name is provided, check not empty
        When(x => x.BusinessName is not null, () => {
            RuleFor(x => x.BusinessName!)
                .NotEmpty().WithMessage("Business name cannot be empty if provided");
        });

        // If short name is provided, check not empty
        When(x => x.ShortName is not null, () => {
            RuleFor(x => x.ShortName!)
                .NotEmpty().WithMessage("Short name cannot be empty if provided");
        });

        // If BankId is provided, check not empty
        When(x => x.BankId.HasValue, () => {
            RuleFor(x => x.BankId!.Value)
                .NotEqual(Guid.Empty).WithMessage("BankId cannot be empty if provided");
        });
    }
}