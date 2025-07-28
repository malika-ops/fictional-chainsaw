using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.CurrencyDenominations.Commands.CreateCurrencyDenomination;

public record CreateCurrencyDenominationCommand : ICommand<Result<Guid>>
{
    public Guid CurrencyId { get; init; } 
    public CurrencyDenominationType Type { get; init; } 
    public decimal Value { get; init; } 
}