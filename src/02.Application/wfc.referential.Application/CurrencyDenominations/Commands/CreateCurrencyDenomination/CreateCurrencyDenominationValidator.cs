using FluentValidation;

namespace wfc.referential.Application.CurrencyDenominations.Commands.CreateCurrencyDenomination;

public class CreateCurrencyDenominationValidator : AbstractValidator<CreateCurrencyDenominationCommand>
{
    public CreateCurrencyDenominationValidator()
    {
        RuleFor(x => x.CurrencyId)
            .NotEmpty()
            .WithMessage("CurrencyId is required.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Type is required.");

        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage("Value is required.");
    }
}