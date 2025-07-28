using FluentValidation;

namespace wfc.referential.Application.CurrencyDenominations.Commands.DeleteCurrencyDenomination;

public class DeleteCurrencyDenominationCommandValidator : AbstractValidator<DeleteCurrencyDenominationCommand>
{
    public DeleteCurrencyDenominationCommandValidator()
    {
        RuleFor(x => x.CurrencyDenominationId)
            .NotEqual(Guid.Empty)
            .WithMessage("CurrencyDenominationId must be a non-empty GUID.");
    }
}