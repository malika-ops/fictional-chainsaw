using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAccountAggregate.Events;

public record PartnerAccountDisabledEvent(
    Guid PartnerAccountId,
    DateTime OccurredOn) : IDomainEvent;