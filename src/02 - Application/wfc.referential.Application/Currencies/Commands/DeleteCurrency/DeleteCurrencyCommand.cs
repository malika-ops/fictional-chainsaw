using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Currencies.Commands.DeleteCurrency;

public class DeleteCurrencyCommand : ICommand<Result<bool>>
{
    public string CurrencyId { get; set; }

    public DeleteCurrencyCommand(string currencyId)
    {
        CurrencyId = currencyId;
    }
}