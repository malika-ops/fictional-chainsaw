using FluentValidation;

namespace wfc.referential.Application.Currencies.Commands.CreateCurrency;

public class CreateCurrencyValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyValidator()
    {
        RuleFor(x => x.Code).NotEmpty()
            .WithMessage("Currency code is required.");
        RuleFor(x => x.Name).NotEmpty()
            .WithMessage("Currency Name is required.");
        RuleFor(x => x.CodeAR).NotEmpty()
            .WithMessage("CodeAR is required.");
        RuleFor(x => x.CodeEN).NotEmpty()
            .WithMessage("CodeEN is required.");
        RuleFor(x => x.CodeIso)
            .InclusiveBetween(0, 999).WithMessage("CodeIso must be a 3-digit number between 0 and 999.");
    }
}