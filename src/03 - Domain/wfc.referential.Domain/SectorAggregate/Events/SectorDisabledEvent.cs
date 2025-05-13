using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SectorAggregate.Events;

public record SectorDisabledEvent : IDomainEvent
{
    public Guid SectorId { get; }
    public DateTime OccurredOn { get; }

    public SectorDisabledEvent(
        Guid sectorId,
        DateTime occurredOn)
    {
        SectorId = sectorId;
        OccurredOn = occurredOn;
    }
}