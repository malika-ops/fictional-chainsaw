using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SectorAggregate.Events;

public record SectorUpdatedEvent : IDomainEvent
{
    public Guid SectorId { get; }
    public string Code { get; } = string.Empty;
    public string Name { get; } = string.Empty;
    public Guid CityId { get; }
    public DateTime OccurredOn { get; }

    public SectorUpdatedEvent(
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