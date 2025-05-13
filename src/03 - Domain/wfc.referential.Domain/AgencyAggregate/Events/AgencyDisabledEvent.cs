using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.AgencyAggregate.Events;

public class AgencyDisabledEvent : IDomainEvent
{
    public Guid AgencyId { get; }
    public DateTime OccurredOn { get; }

    public AgencyDisabledEvent(Guid agencyId, DateTime occurredOn)
    {
        AgencyId = agencyId;
        OccurredOn = occurredOn;
    }
}
