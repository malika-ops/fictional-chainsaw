using FluentValidation;

namespace wfc.referential.Application.CurrencyDenominations.Commands.UpdateCurrencyDenomination;

public class UpdateCurrencyDenominationCommandValidator : AbstractValidator<UpdateCurrencyDenominationCommand>
{
    public UpdateCurrencyDenominationCommandValidator()
    {
        RuleFor(x => x.CurrencyDenominationId)
            .NotEqual(Guid.Empty)
            .WithMessage("CurrencyDenominationId cannot be empty.");

        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("CurrencyId is required.");

        RuleFor(x => x.Type)
            .NotNull()
            .WithMessage("Type is required.")
            .IsInEnum()
            .WithMessage("Type must be a valid value.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required.");

    }
}