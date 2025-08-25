using FluentValidation;

namespace wfc.referential.Application.SupportAccounts.Commands.PatchSupportAccount;

public class PatchSupportAccountCommandValidator : AbstractValidator<PatchSupportAccountCommand>
{
    public PatchSupportAccountCommandValidator()
    {
        RuleFor(x => x.SupportAccountId)
            .NotEqual(Guid.Empty).WithMessage("SupportAccountId cannot be empty.");

        // If code is provided (not null), check it's not empty
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty()
                .WithMessage("Code cannot be empty if provided.");
        });

        // If description is provided, check not empty
        When(x => x.Description is not null, () => {
            RuleFor(x => x.Description!)
                .NotEmpty()
                .WithMessage("Description cannot be empty if provided.");
        });

        // If accounting number is provided, check not empty
        When(x => x.AccountingNumber is not null, () => {
            RuleFor(x => x.AccountingNumber!)
                .NotEmpty()
                .WithMessage("Accounting number cannot be empty if provided.");
        });

        // If Support Account Type is provided, check is in enum
        When(x => x.SupportAccountType is not null, () => {
            RuleFor(x => x.SupportAccountType!)
                .NotEmpty()
                .IsInEnum()
                .WithMessage("Support Account Type must be in Enum.");
        });
    }
}