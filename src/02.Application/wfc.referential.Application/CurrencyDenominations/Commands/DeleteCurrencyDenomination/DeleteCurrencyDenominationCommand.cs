using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.CurrencyDenominations.Commands.DeleteCurrencyDenomination;

public record DeleteCurrencyDenominationCommand : ICommand<Result<bool>>
{
    public Guid CurrencyDenominationId { get; }
    public DeleteCurrencyDenominationCommand(Guid currencyDenominationId) => CurrencyDenominationId = currencyDenominationId;
}