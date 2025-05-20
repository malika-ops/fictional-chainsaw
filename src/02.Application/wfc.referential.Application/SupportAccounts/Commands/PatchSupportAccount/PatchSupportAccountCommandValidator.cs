using FluentValidation;

namespace wfc.referential.Application.SupportAccounts.Commands.PatchSupportAccount;

public class PatchSupportAccountCommandValidator : AbstractValidator<PatchSupportAccountCommand>
{
    public PatchSupportAccountCommandValidator()
    {
        RuleFor(x => x.SupportAccountId)
            .NotEqual(Guid.Empty).WithMessage("SupportAccountId cannot be empty");

        // If code is provided, check not empty
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty().WithMessage("Code cannot be empty if provided");
        });

        // If name is provided, check not empty
        When(x => x.Name is not null, () => {
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("Name cannot be empty if provided");
        });

        // If accounting number is provided, check not empty
        When(x => x.AccountingNumber is not null, () => {
            RuleFor(x => x.AccountingNumber!)
                .NotEmpty().WithMessage("Accounting number cannot be empty if provided");
        });

        // If threshold is provided, check non-negative
        When(x => x.Threshold.HasValue, () => {
            RuleFor(x => x.Threshold!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Threshold must be non-negative if provided");
        });

        // If limit is provided, check non-negative
        When(x => x.Limit.HasValue, () => {
            RuleFor(x => x.Limit!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Limit must be non-negative if provided");
        });

        // If account balance is provided, check non-negative
        When(x => x.AccountBalance.HasValue, () => {
            RuleFor(x => x.AccountBalance!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Account balance must be non-negative if provided");
        });

        // If partner id is provided, check not empty
        When(x => x.PartnerId.HasValue, () => {
            RuleFor(x => x.PartnerId!.Value)
                .NotEqual(Guid.Empty).WithMessage("PartnerId cannot be empty if provided");
        });
    }
}