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

    public static Sector Create(
        SectorId id,
        string code,
        string name,
        CityId cityId)
    {
        var sector = new Sector
        {
            Id = id,
            Code = code,
            Name = name,
            CityId = cityId,
            IsEnabled = true
        };

        sector.AddDomainEvent(new SectorCreatedEvent(
            sector.Id.Value,
            sector.Code,
            sector.Name,
            sector.CityId.Value,
            DateTime.UtcNow));

        return sector;
    }

    public void Update(
        string code,
        string name,
        CityId cityId,
        bool? isEnabled)
    {
        Code = code;
        Name = name;
        CityId = cityId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new SectorUpdatedEvent(
            Id.Value,
            Code,
            Name,
            CityId.Value,
            DateTime.UtcNow));
    }

    public void Patch(
        string? code,
        string? name,
        CityId? cityId,
        bool? isEnabled)
    {
        Code = code ?? Code;
        Name = name ?? Name;
        CityId = cityId ?? CityId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new SectorPatchedEvent(
            Id.Value,
            Code,
            Name,
            CityId.Value,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new SectorDisabledEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new SectorActivatedEvent(
            Id.Value,
            DateTime.UtcNow));
    }
}