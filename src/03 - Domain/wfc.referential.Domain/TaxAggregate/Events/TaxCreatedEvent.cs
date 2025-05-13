using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TaxAggregate.Events;
public record TaxCreatedEvent(Tax Tax) : IDomainEvent;
