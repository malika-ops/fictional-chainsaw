using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.AffiliateAggregate.Events;

public record AffiliateDisabledEvent(
    Guid AffiliateId,
    DateTime OccurredOn) : IDomainEvent;