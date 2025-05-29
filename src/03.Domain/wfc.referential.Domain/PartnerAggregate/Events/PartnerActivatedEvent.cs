using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAggregate.Events;

public record PartnerActivatedEvent(
    Guid PartnerId,
    DateTime OccurredOn) : IDomainEvent;