using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TaxAggregate.Events;

public record TaxPatchedEvent(Tax Tax) : IDomainEvent;