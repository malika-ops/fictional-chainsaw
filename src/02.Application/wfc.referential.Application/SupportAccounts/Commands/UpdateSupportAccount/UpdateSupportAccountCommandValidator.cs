using FluentValidation;

namespace wfc.referential.Application.SupportAccounts.Commands.UpdateSupportAccount;

public class UpdateSupportAccountCommandValidator : AbstractValidator<UpdateSupportAccountCommand>
{
    public UpdateSupportAccountCommandValidator()
    {
        RuleFor(x => x.SupportAccountId)
            .NotEqual(Guid.Empty)
            .WithMessage("SupportAccountId cannot be empty.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.AccountingNumber)
            .NotEmpty().WithMessage("Accounting number is required.");
    }
}