using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAccountAggregate.Events;

public record PartnerAccountActivatedEvent(
    Guid PartnerAccountId,
    DateTime OccurredOn) : IDomainEvent;