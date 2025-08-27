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

        // If BankId is provided, check not empty
        When(x => x.BankId.HasValue, () => {
            RuleFor(x => x.BankId!.Value)
                .NotEqual(Guid.Empty).WithMessage("BankId cannot be empty if provided");
        });

        // If AccountTypeId is provided, check not empty
        When(x => x.PartnerAccountType.HasValue, () => {
            RuleFor(x => x.PartnerAccountType)
            .NotEmpty()
            .IsInEnum()
            .WithMessage("Partner Account Type must be in Enum.");
        });
    }
}