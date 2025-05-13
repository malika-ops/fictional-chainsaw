using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate.Events;

namespace wfc.referential.Domain.RegionAggregate;

public class Region : Aggregate<RegionId>
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public CountryId CountryId { get; private set; } // Clé étrangère
    public List<City> Cities { get; private set; } = new();
    public bool IsEnabled { get; private set; } = true;


    private Region() { }

    public static Region Create(RegionId id, string code, string name, CountryId countryId)
    {
        var region = new Region
        {
            Id = id,
            Code = code,
            Name = name,
            CountryId = countryId
        };

        // raise the creation event
        region.AddDomainEvent(new RegionCreatedEvent(
            region.Id.Value,
            region.Code,
            region.Name,
            region.IsEnabled,
            countryId
        ));
        return region;
    }

    public void SetInactive()
    {
        IsEnabled = false;

        // Raise the status changed event
        AddDomainEvent(new RegionStatusChangedEvent(
            Id.Value,
            IsEnabled,
            DateTime.UtcNow
        ));
    }
    public void Update(string code, string name, bool status, CountryId countryId)
    {
        Code = code;
        Name = name;
        IsEnabled = status;
        CountryId = countryId;

        // raise the update event
        AddDomainEvent(new RegionUpdatedEvent(
            Id.Value,
            Code,
            Name,
            IsEnabled,
            CountryId,
            DateTime.UtcNow
        ));
    }
    public void Patch()
    {
        AddDomainEvent(new RegionPatchedEvent(
            Id.Value,
            Code,
            Name,
            IsEnabled,
            CountryId,
            DateTime.UtcNow
        ));
    }

}
