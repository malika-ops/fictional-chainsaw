using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ServiceControleAggregate.Events;

public class ServiceControleCreatedEvent : IDomainEvent
{
    public Guid ServiceControleId { get; }
    public Guid ServiceId { get; }
    public Guid ControleId { get; }
    public Guid ChannelId { get; }
    public int ExecOrder { get; }
    public DateTime OccurredOn { get; }

    public ServiceControleCreatedEvent(
        Guid serviceControleId,
        Guid serviceId,
        Guid controleId,
        Guid channelId,
        int execOrder,
        DateTime occurredOn)
    {
        ServiceControleId = serviceControleId;
        ServiceId = serviceId;
        ControleId = controleId;
        ChannelId = channelId;
        ExecOrder = execOrder;
        OccurredOn = occurredOn;
    }
}