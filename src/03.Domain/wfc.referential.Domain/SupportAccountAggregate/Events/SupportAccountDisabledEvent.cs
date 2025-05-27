using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SupportAccountAggregate.Events;

public record SupportAccountDisabledEvent(
    Guid SupportAccountId,
    DateTime OccurredOn) : IDomainEvent;