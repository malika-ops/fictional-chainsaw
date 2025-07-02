using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ServiceControleAggregate.Events;

public class ServiceControlePatchedEvent : IDomainEvent
{
    public Guid ServiceControleId { get; }
    public Guid ServiceId { get; }
    public Guid ControleId { get; }
    public Guid ChannelId { get; }
    public int ExecOrder { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public ServiceControlePatchedEvent(
        Guid serviceControleId,
        Guid serviceId,
        Guid controleId,
        Guid channelId,
        int execOrder,
        bool isEnabled,
        DateTime occurredOn)
    {
        ServiceControleId = serviceControleId;
        ServiceId = serviceId;
        ControleId = controleId;
        ChannelId = channelId;
        ExecOrder = execOrder;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}