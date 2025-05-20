using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SectorAggregate.Events;

public record SectorCreatedEvent : IDomainEvent
{
    public Guid SectorId { get; }
    public string Code { get; }
    public string Name { get; }
    public Guid CityId { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public SectorCreatedEvent(
        Guid sectorId,
        string code,
        string name,
        Guid cityId,
        bool isEnabled,
        DateTime occurredOn)
    {
        SectorId = sectorId;
        Code = code;
        Name = name;
        CityId = cityId;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}