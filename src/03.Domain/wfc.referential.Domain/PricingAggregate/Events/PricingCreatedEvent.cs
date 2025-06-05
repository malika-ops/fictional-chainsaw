using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Domain.PricingAggregate.Events;

public record PricingCreatedEvent : IDomainEvent
{
    public PricingId PricingId { get; }
    public string Code { get; }
    public ServiceId ServiceId { get; }
    public CorridorId CorridorId { get; }
    public DateTime OccurredOn { get; }

    public PricingCreatedEvent(PricingId pricingId, string code, ServiceId serviceId, CorridorId corridorId, DateTime occurredOn)
    {
        PricingId = pricingId;
        Code = code;
        ServiceId = serviceId;
        CorridorId = corridorId;
        OccurredOn = occurredOn;
    }
}