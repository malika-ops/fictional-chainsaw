using FluentValidation;

namespace wfc.referential.Application.SupportAccounts.Commands.CreateSupportAccount;

public class CreateSupportAccountCommandValidator : AbstractValidator<CreateSupportAccountCommand>
{
    public CreateSupportAccountCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty()
            .WithMessage("Support account code is required.");
        RuleFor(x => x.Description).NotEmpty()
            .WithMessage("Support account description is required.");
        RuleFor(x => x.AccountingNumber).NotEmpty()
            .WithMessage("Accounting number is required.");
    }
}