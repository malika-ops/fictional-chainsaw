using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CityAggregate.Events;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Domain.CityAggregate;
public class City : Aggregate<CityId>
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Abbreviation { get; private set; }
    public RegionId RegionId { get; private set; } = default!;
    public string TimeZone { get; private set; } = default!;
    public bool IsEnabled { get; private set; } = true;
    public List<Sector> Sectors { get; set; } = [];

    private City() { }
    public static City Create(CityId id, string cityCode, string cityName, string timeZone, RegionId regionId, string abbreviation)
    {
        var city = new City
        {
            Id = id,
            Code = cityCode,
            Name = cityName,
            TimeZone = timeZone,
            RegionId = regionId,
            Abbreviation = abbreviation
        };

        // Raise the creation event
        city.AddDomainEvent(new CityCreatedEvent(
            city.Id.Value,
            city.Code,
            city.Name,
            city.Abbreviation,
            city.TimeZone,
            city.IsEnabled,
            regionId,
            DateTime.UtcNow
        ));
        return city;
    }

    public void Update(string cityCode, string cityName, string abbreviation, string timeZone, RegionId regionId)
    {
        Code = cityCode;
        Name = cityName;
        TimeZone = timeZone;
        RegionId = regionId;
        Abbreviation = abbreviation;

        // raise the update event
        AddDomainEvent(new CityUpdatedEvent(
            Id!.Value,
            Code,
            Name,
            Abbreviation,
            TimeZone,
            IsEnabled,
            RegionId,
            DateTime.UtcNow
        ));
    }
    public void SetInactive()
    {
        IsEnabled = false;

        // Raise the status changed event
        AddDomainEvent(new CityStatusChangedEvent(
            Id.Value,
            IsEnabled,
            DateTime.UtcNow
        ));
    }
    public void Patch()
    {
        AddDomainEvent(new CityPatchedEvent(
            Id.Value,
            Code,
            Name,
            Abbreviation,
            TimeZone,
            IsEnabled,
            RegionId,
            DateTime.UtcNow
        ));
    }

}