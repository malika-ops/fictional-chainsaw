using FluentValidation;

namespace wfc.referential.Application.Banks.Commands.UpdateBank;

public class UpdateBankCommandValidator : AbstractValidator<UpdateBankCommand>
{
    public UpdateBankCommandValidator()
    {
        RuleFor(x => x.BankId)
            .NotEqual(Guid.Empty).WithMessage("BankId cannot be empty");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Abbreviation)
            .NotEmpty().WithMessage("Abbreviation is required");
    }
}