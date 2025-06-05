using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.AffiliateAggregate.Events;

public record AffiliateActivatedEvent(
    Guid AffiliateId,
    DateTime OccurredOn) : IDomainEvent;