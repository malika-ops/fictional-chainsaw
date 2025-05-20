using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.AgencyTierAggregate.Events;

public class AgencyTierUpdatedEvent : IDomainEvent
{
    public Guid AgencyTierId { get; }
    public Guid AgencyId { get; }
    public Guid TierId { get; }
    public string Code { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public AgencyTierUpdatedEvent(
        Guid agencyTierId,
        Guid agencyId,
        Guid tierId,
        string code,
        bool isEnabled,
        DateTime occurredOn)
    {
        AgencyTierId = agencyTierId;
        AgencyId = agencyId;
        TierId = tierId;
        Code = code;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}