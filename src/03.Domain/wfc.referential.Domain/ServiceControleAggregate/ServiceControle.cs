using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceControleAggregate.Events;

namespace wfc.referential.Domain.ServiceControleAggregate;

public class ServiceControle : Aggregate<ServiceControleId>
{
    public ServiceId ServiceId { get; private set; }
    public ControleId ControleId { get; private set; }
    public ParamTypeId ChannelId { get; private set; }

    public int ExecOrder { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    public Service? Service { get; private set; }
    public Controle? Controle { get; private set; }
    public ParamType? Channel { get; private set; }

    private ServiceControle() { }

    public static ServiceControle Create(
        ServiceControleId id,
        ServiceId serviceId,
        ControleId controleId,
        ParamTypeId channelId,
        int execOrder)
    {
        var entity = new ServiceControle
        {
            Id = id,
            ServiceId = serviceId,
            ControleId = controleId,
            ChannelId = channelId,
            ExecOrder = execOrder
        };

        entity.AddDomainEvent(new ServiceControleCreatedEvent(
            entity.Id.Value,
            entity.ServiceId.Value,
            entity.ControleId.Value,
            entity.ChannelId.Value,
            entity.ExecOrder,
            DateTime.UtcNow));

        return entity;
    }

    public void Update(
        ServiceId serviceId,
        ControleId controleId,
        ParamTypeId channelId,
        int execOrder,
        bool isEnabled)
    {
        ServiceId = serviceId;
        ControleId = controleId;
        ChannelId = channelId;
        ExecOrder = execOrder;
        IsEnabled = isEnabled;

        AddDomainEvent(new ServiceControleUpdatedEvent(
            Id!.Value,
            ServiceId.Value,
            ControleId.Value,
            ChannelId.Value,
            ExecOrder,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Patch(
        ServiceId? serviceId,
        ControleId? controleId,
        ParamTypeId? channelId,
        int? execOrder,
        bool? isEnabled)
    {
        ServiceId = serviceId ?? ServiceId;
        ControleId = controleId ?? ControleId;
        ChannelId = channelId ?? ChannelId;
        ExecOrder = execOrder ?? ExecOrder;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new ServiceControlePatchedEvent(
            Id!.Value,
            ServiceId.Value,
            ControleId.Value,
            ChannelId.Value,
            ExecOrder,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;
        AddDomainEvent(new ServiceControleDisabledEvent(Id!.Value, DateTime.UtcNow));
    }
}