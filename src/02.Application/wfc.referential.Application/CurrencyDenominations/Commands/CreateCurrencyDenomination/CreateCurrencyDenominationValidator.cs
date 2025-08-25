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
            .NotNull()
            .WithMessage("Type is required.")
            .IsInEnum()
            .WithMessage("Type must be a valid value.");

        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage("Value is required.");
    }
}