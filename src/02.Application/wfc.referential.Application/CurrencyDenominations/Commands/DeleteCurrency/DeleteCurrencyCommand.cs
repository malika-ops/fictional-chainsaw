using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Currencies.Commands.DeleteCurrency;

public record DeleteCurrencyCommand : ICommand<Result<bool>>
{
    public Guid CurrencyId { get; }
    public DeleteCurrencyCommand(Guid currencyId) => CurrencyId = currencyId;
}