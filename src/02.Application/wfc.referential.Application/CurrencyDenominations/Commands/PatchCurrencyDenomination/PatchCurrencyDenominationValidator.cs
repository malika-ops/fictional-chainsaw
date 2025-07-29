using FluentValidation;

namespace wfc.referential.Application.CurrencyDenominations.Commands.PatchCurrencyDenomination;

public class PatchCurrencyDenominationCommandValidator : AbstractValidator<PatchCurrencyDenominationCommand>
{
    public PatchCurrencyDenominationCommandValidator()
    {
        RuleFor(x => x.CurrencyDenominationId)
            .NotEqual(Guid.Empty).WithMessage("CurrencyDenominationId cannot be empty.");

        // If code is provided (not null), check it's not empty
        When(x => x.CurrencyId is not null, () =>
        {
            RuleFor(x => x.CurrencyId!)
                .NotEmpty().WithMessage("CurrencyId cannot be empty if provided.");
        });

        // If name is provided, check not empty
        When(x => x.Type is not null, () =>
        {
            RuleFor(x => x.Type!)
                .NotEmpty()
                .IsInEnum()
                .WithMessage("Type cannot be empty if provided.");
        });

        // If CodeAR is provided, check not empty
        When(x => x.Value is not null, () =>
        {
            RuleFor(x => x.Value!)
                .NotEmpty().WithMessage("Value cannot be empty if provided.");
        });
    }
}