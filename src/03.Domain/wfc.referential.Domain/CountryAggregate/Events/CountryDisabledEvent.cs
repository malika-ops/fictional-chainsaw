using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.Countries.Events;

public class CountryDisabledEvent : IDomainEvent
{
    public Guid CountryId { get; }
    public DateTime OccurredOn { get; }

    public CountryDisabledEvent(Guid countryId, DateTime occurredOn)
    {
        CountryId = countryId;
        OccurredOn = occurredOn;
    }
}
