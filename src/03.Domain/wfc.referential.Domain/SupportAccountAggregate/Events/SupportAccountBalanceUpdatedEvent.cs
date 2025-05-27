using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SupportAccountAggregate.Events;

public record SupportAccountBalanceUpdatedEvent(
    Guid SupportAccountId,
    decimal OldBalance,
    decimal NewBalance,
    DateTime OccurredOn) : IDomainEvent;