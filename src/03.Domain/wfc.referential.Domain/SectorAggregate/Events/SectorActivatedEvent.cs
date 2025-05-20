using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SectorAggregate.Events;

public record SectorActivatedEvent : IDomainEvent
{
    public Guid SectorId { get; }
    public DateTime OccurredOn { get; }

    public SectorActivatedEvent(
        Guid sectorId,
        DateTime occurredOn)
    {
        SectorId = sectorId;
        OccurredOn = occurredOn;
    }
}