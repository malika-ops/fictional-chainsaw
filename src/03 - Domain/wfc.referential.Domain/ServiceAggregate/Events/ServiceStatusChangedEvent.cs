using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ServiceAggregate.Events;

public class ServiceStatusChangedEvent : IDomainEvent
{
    public Guid ServiceId { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public ServiceStatusChangedEvent(Guid serviceId, bool isEnabled, DateTime occurredOn)
    {
        ServiceId = serviceId;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}