using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Domain.AffiliateAggregate.Events;

public record AffiliatePatchedEvent(
    Guid AffiliateId,
    string Code,
    string Name,
    string Abbreviation,
    DateTime? OpeningDate,
    string CancellationDay,
    string Logo,
    decimal ThresholdBilling,
    string AccountingDocumentNumber,
    string AccountingAccountNumber,
    string StampDutyMention,
    CountryId CountryId,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;