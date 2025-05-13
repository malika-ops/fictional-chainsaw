using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.AgencyTierAggregate.Events;

public class AgencyTierCreatedEvent : IDomainEvent
{
    public Guid AgencyTierId { get; }
    public Guid AgencyId { get; }
    public Guid TierId { get; }
    public string Code { get; }
    public DateTime OccurredOn { get; }

    public AgencyTierCreatedEvent(
        Guid agencyTierId,
        Guid agencyId,
        Guid tierId,
        string code,
        DateTime occurredOn)
    {
        AgencyTierId = agencyTierId;
        AgencyId = agencyId;
        TierId = tierId;
        Code = code;
        OccurredOn = occurredOn;
    }
}