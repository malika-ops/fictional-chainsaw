using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.CurrencyDenominations.Commands.UpdateCurrencyDenomination;

public record UpdateCurrencyDenominationCommand : ICommand<Result<bool>>
{
    public Guid CurrencyDenominationId { get; init; }
    public Guid CurrencyId { get; init; }
    public CurrencyDenominationType Type { get; init; }
    public decimal Value { get; init; }
    public bool IsEnabled { get; init; }
}