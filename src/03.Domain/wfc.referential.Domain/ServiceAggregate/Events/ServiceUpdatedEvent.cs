using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ServiceAggregate.Events;

public class ServiceUpdatedEvent : IDomainEvent
{
    public Guid ServiceId { get; }
    public string Code { get; }
    public string Name { get; }
    public FlowDirection FlowDirection { get; }
    public bool IsEnabled { get; }
    public Guid ProductId { get; }
    public DateTime OccurredOn { get; }

    public ServiceUpdatedEvent(Guid serviceId, string code, string name, FlowDirection flowDirection, bool isEnabled, DateTime occurredOn, Guid productId)
    {
        ServiceId = serviceId;
        Code = code;
        Name = name;
        FlowDirection = flowDirection;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
        ProductId = productId;
    }
}
