using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TaxAggregate.Events;

public record TaxUpdatedEvent(Tax Tax) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.Now;
}
