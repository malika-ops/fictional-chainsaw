using FluentValidation;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Currencies.Commands.PatchCurrency;

public class PatchCurrencyValidator : AbstractValidator<PatchCurrencyCommand>
{
    public PatchCurrencyValidator(ICurrencyRepository currencyRepository)
    {
        // If code is provided (not null), check it's not empty
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty().WithMessage("Code cannot be empty if provided.");
        });

        // If name is provided, check not empty
        When(x => x.Name is not null, () => {
            RuleFor(x => x.Name!)
                .NotEmpty().WithMessage("Name cannot be empty if provided.");
        });

        // If CodeAR is provided, check not empty
        When(x => x.CodeAR is not null, () => {
            RuleFor(x => x.CodeAR!)
                .NotEmpty().WithMessage("CodeAR cannot be empty if provided.");
        });

        // If CodeEN is provided, check not empty
        When(x => x.CodeEN is not null, () => {
            RuleFor(x => x.CodeEN!)
                .NotEmpty().WithMessage("CodeEN cannot be empty if provided.");
        });

        // If CodeIso is provided, check that it's a valid 3-digit number
        When(x => x.CodeIso.HasValue, () => {
            RuleFor(x => x.CodeIso!.Value)
                .InclusiveBetween(0, 999).WithMessage("CodeIso must be a 3-digit number between 0 and 999");
        });
    }
}