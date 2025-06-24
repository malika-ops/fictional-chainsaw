using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Domain.PricingAggregate.Events;

public record PricingPatchedEvent : IDomainEvent
{
    public PricingId PricingId { get; }
    public string Code { get; }
    public ServiceId ServiceId { get; }
    public CorridorId CorridorId { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public PricingPatchedEvent(
        PricingId pricingId,
        string code,
        ServiceId serviceId,
        CorridorId corridorId,
        bool isEnabled,
        DateTime occurredOn)
    {
        PricingId = pricingId;
        Code = code;
        ServiceId = serviceId;
        CorridorId = corridorId;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}