using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAccountAggregate.Events;

public record PartnerAccountUpdatedEvent(
    Guid PartnerAccountId,
    string AccountNumber,
    string RIB,
    string Domiciliation,
    string BusinessName,
    string ShortName,
    decimal AccountBalance,
    Guid BankId,
    Guid AccountTypeId,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;