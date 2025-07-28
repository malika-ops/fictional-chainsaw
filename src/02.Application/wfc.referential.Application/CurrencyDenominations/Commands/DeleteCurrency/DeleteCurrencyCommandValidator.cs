using FluentValidation;

namespace wfc.referential.Application.Currencies.Commands.DeleteCurrency;

public class DeleteCurrencyCommandValidator : AbstractValidator<DeleteCurrencyCommand>
{
    public DeleteCurrencyCommandValidator()
    {
        RuleFor(x => x.CurrencyId)
            .NotEqual(Guid.Empty)
            .WithMessage("CurrencyId must be a non-empty GUID.");
    }
}