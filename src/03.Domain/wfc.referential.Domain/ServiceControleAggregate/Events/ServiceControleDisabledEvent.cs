using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ServiceControleAggregate.Events;

public class ServiceControleDisabledEvent : IDomainEvent
{
    public Guid ServiceControleId { get; }
    public DateTime OccurredOn { get; }

    public ServiceControleDisabledEvent(Guid serviceControleId, DateTime occurredOn)
    {
        ServiceControleId = serviceControleId;
        OccurredOn = occurredOn;
    }
}