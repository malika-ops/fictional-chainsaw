using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CountryIdentityDocAggregate.Events;

public record CountryIdentityDocCreatedEvent : IDomainEvent
{
    public Guid CountryIdentityDocId { get; }
    public Guid CountryId { get; }
    public Guid IdentityDocumentId { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public CountryIdentityDocCreatedEvent(
        Guid countryIdentityDocId,
        Guid countryId,
        Guid identityDocumentId,
        bool isEnabled,
        DateTime occurredOn)
    {
        CountryIdentityDocId = countryIdentityDocId;
        CountryId = countryId;
        IdentityDocumentId = identityDocumentId;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}