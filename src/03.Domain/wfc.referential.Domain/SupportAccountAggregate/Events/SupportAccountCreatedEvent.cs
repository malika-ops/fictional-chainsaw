using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SupportAccountAggregate.Events;

public record SupportAccountCreatedEvent(
    Guid SupportAccountId,
    string Code,
    string Description,
    decimal Threshold,
    decimal Limit,
    decimal AccountBalance,
    string AccountingNumber,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;