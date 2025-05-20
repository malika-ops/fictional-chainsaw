using System;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.Countries.Events;

public class CountryUpdatedEvent : IDomainEvent
{
    public Guid CountryId { get; }
    public string Code { get; }
    public string Name { get; }
    public DateTime OccurredOn { get; }

    public CountryUpdatedEvent(Guid countryId, string code, string name, DateTime occurredOn)
    {
        CountryId = countryId;
        Code = code;
        Name = name;
        OccurredOn = occurredOn;
    }
}
