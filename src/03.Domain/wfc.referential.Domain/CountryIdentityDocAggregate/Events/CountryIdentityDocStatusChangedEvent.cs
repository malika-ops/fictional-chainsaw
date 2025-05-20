using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CountryIdentityDocAggregate.Events;

public record CountryIdentityDocStatusChangedEvent : IDomainEvent
{
    public Guid CountryIdentityDocId { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public CountryIdentityDocStatusChangedEvent(
        Guid countryIdentityDocId,
        bool isEnabled,
        DateTime occurredOn)
    {
        CountryIdentityDocId = countryIdentityDocId;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}