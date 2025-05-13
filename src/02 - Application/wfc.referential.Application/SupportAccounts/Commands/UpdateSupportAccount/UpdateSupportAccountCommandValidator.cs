using FluentValidation;

namespace wfc.referential.Application.SupportAccounts.Commands.UpdateSupportAccount;

public class UpdateSupportAccountCommandValidator : AbstractValidator<UpdateSupportAccountCommand>
{
    public UpdateSupportAccountCommandValidator()
    {
        RuleFor(x => x.SupportAccountId)
            .NotEqual(Guid.Empty).WithMessage("SupportAccountId cannot be empty");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.AccountingNumber)
            .NotEmpty().WithMessage("Accounting number is required");

        RuleFor(x => x.Threshold)
            .GreaterThanOrEqualTo(0).WithMessage("Threshold must be non-negative");

        RuleFor(x => x.Limit)
            .GreaterThanOrEqualTo(0).WithMessage("Limit must be non-negative");

        RuleFor(x => x.AccountBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Account balance must be non-negative");

        RuleFor(x => x.PartnerId)
            .NotEmpty().WithMessage("Partner ID is required");
    }
}