using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAggregate.Events;

public record PartnerDisabledEvent(
    Guid PartnerId,
    DateTime OccurredOn) : IDomainEvent;