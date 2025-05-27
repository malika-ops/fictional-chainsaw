using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SupportAccountAggregate.Events;

public record SupportAccountActivatedEvent(
    Guid SupportAccountId,
    DateTime OccurredOn) : IDomainEvent;