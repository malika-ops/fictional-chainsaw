using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.MonetaryZone.Events;
using wfc.referential.Domain.MonetaryZoneAggregate.Events;

namespace wfc.referential.Domain.MonetaryZoneAggregate;

public class MonetaryZone : Aggregate<MonetaryZoneId>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;
    public List<Country> Countries { get; private set; } = [];

    private MonetaryZone() { }


    public static MonetaryZone Create(MonetaryZoneId id, string code, string name, string description, List<Country> countries)
    {
        var monetaryZone = new MonetaryZone
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            Countries = countries
        };

        // raise the creation event
        monetaryZone.AddDomainEvent(new MonetaryZoneCreatedEvent(
            monetaryZone.Id.Value,
            monetaryZone.Code,
            monetaryZone.Name,
            monetaryZone.Description,
            monetaryZone.IsEnabled,
            DateTime.UtcNow
        ));
        return monetaryZone;
    }

    public void Update(
            string code,
            string name,
            string description,
            bool isEnabled)
    {
        Code = code;
        Name = name;
        Description = description;
        IsEnabled = isEnabled;

        // raise the update event
        AddDomainEvent(new MonetaryZoneUpdatedEvent(
            Id.Value,
            Code,
            Name,
            Description,
            DateTime.UtcNow
        ));
    }

    public void Patch(string? code,
            string? name,
            string? description,
            bool? isEnabled)
    {
        Code = code == null ? Code : code;
        Name = name == null ? Name : name;
        Description = description == null ? Description : description;
        IsEnabled = isEnabled.HasValue ? isEnabled.Value : IsEnabled;

        AddDomainEvent(new MonetaryZonePatchedEvent(
            Id.Value,
            Code,
            Name,
            Description,
            DateTime.UtcNow
        ));
    }

    public void Disable()
    {
        IsEnabled = false;

        // raise the disable event
        AddDomainEvent(new MonetaryZoneDisabledEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }

}
