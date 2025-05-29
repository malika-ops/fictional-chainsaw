using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAccountAggregate.Events;

public record PartnerAccountBalanceUpdatedEvent(
    Guid PartnerAccountId,
    decimal OldBalance,
    decimal NewBalance,
    DateTime OccurredOn) : IDomainEvent;