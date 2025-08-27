using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAccountAggregate.Events;

public record PartnerAccountPatchedEvent(
    Guid PartnerAccountId,
    string AccountNumber,
    string RIB,
    string Domiciliation,
    string BusinessName,
    string ShortName,
    decimal AccountBalance,
    Guid BankId,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;