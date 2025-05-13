using FluentValidation;

namespace wfc.referential.Application.Currencies.Commands.DeleteCurrency;

public class DeleteCurrencyCommandValidator : AbstractValidator<DeleteCurrencyCommand>
{
    public DeleteCurrencyCommandValidator()
    {
        // Ensure the ID is non-empty & parseable as a GUID
        RuleFor(x => x.CurrencyId)
            .Must(guidStr => Guid.TryParse(guidStr, out var parsed) && parsed != Guid.Empty)
            .WithMessage("CurrencyId must be a valid, non-empty GUID.");
    }
}