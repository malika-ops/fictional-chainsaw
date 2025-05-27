using FluentValidation;

namespace wfc.referential.Application.Currencies.Commands.UpdateCurrency;

public class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyCommandValidator()
    {
        RuleFor(x => x.CurrencyId)
            .NotEqual(Guid.Empty)
            .WithMessage("CurrencyId cannot be empty.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.CodeAR)
            .NotEmpty().WithMessage("CodeAR is required.");

        RuleFor(x => x.CodeEN)
            .NotEmpty().WithMessage("CodeEN is required.");

        RuleFor(x => x.CodeIso)
            .InclusiveBetween(0, 999).WithMessage("CodeIso must be a 3-digit number between 0 and 999.");
    }
}