using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ServiceAggregate.Events;

public class ServiceCreatedEvent : IDomainEvent
{
    public Guid ServiceId { get; }
    public string Code { get; }
    public string Name { get; }
    public bool IsEnabled { get; }
    public Guid ProductId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public ServiceCreatedEvent(Guid serviceId, string code, string name, bool isEnabled, Guid productId)
    {
        ServiceId = serviceId;
        Code = code;
        Name = name;
        IsEnabled = isEnabled;
        ProductId = productId;
    }
}