using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Currencies.Commands.DeleteCurrency;

public record DeleteCurrencyCommand : ICommand<Result<bool>>
{
    public string CurrencyId { get; init; }

    public DeleteCurrencyCommand(string currencyId)
    {
        CurrencyId = currencyId;
    }
}