using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SectorAggregate.Events;

namespace wfc.referential.Domain.SectorAggregate;

public class Sector : Aggregate<SectorId>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public City City { get; private set; }
    public CityId CityId { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    private Sector() { }

    public static Sector Create(SectorId id, string code, string name, City city)
    {
        var sector = new Sector
        {
            Id = id,
            Code = code,
            Name = name,
            City = city,
            IsEnabled = true
        };

        // raise the creation event
        sector.AddDomainEvent(new SectorCreatedEvent(
            sector.Id.Value,
            sector.Code,
            sector.Name,
            sector.City.Id.Value,
            sector.IsEnabled,
            DateTime.UtcNow
        ));
        return sector;
    }

    public void Update(
            string code,
            string name,
            City city)
    {
        Code = code;
        Name = name;
        City = city;

        // raise the update event
        AddDomainEvent(new SectorUpdatedEvent(
            Id.Value,
            Code,
            Name,
            City.Id.Value,
            DateTime.UtcNow
        ));
    }

    public void Patch()
    {
        AddDomainEvent(new SectorPatchedEvent(
            Id.Value,
            Code,
            Name,
            City.Id.Value,
            DateTime.UtcNow
        ));
    }

    public void Disable()
    {
        IsEnabled = false;

        // raise the disable event
        AddDomainEvent(new SectorDisabledEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }

    public void Activate()
    {
        IsEnabled = true;

        // raise the activate event
        AddDomainEvent(new SectorActivatedEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }
}