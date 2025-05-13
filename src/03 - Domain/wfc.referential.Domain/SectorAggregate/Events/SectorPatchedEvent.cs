using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SectorAggregate.Events;

public record SectorPatchedEvent : IDomainEvent
{
    public Guid SectorId { get; }
    public string Code { get; }
    public string Name { get; }
    public Guid CityId { get; }
    public DateTime OccurredOn { get; }

    public SectorPatchedEvent(
        Guid sectorId,
        string code,
        string name,
        Guid cityId,
        DateTime occurredOn)
    {
        SectorId = sectorId;
        Code = code;
        Name = name;
        CityId = cityId;
        OccurredOn = occurredOn;
    }
}