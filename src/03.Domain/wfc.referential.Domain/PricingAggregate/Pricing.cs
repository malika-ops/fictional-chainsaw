using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.PricingAggregate.Events;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Domain.PricingAggregate;

public class Pricing : Aggregate<PricingId>
{
    public string Code { get; private set; } = string.Empty;       
    public string Channel { get; private set; } = string.Empty;    

    public decimal MinimumAmount { get; private set; }
    public decimal MaximumAmount { get; private set; }

    public decimal? FixedAmount { get; private set; }
    public decimal? Rate { get; private set; }

    public bool IsEnabled { get; private set; } = true;

    public CorridorId CorridorId { get; private set; }         
    public ServiceId ServiceId { get; private set; }            
    public AffiliateId? AffiliateId { get; private set; }     

    public Service? Service { get; private set; }
    public Affiliate? Affiliate { get; private set; }
    public Corridor? Corridor { get; private set; }

    private Pricing() { }

    public static Pricing Create(
        PricingId id,
        string code,
        string channel,
        decimal minimumAmount,
        decimal maximumAmount,
        decimal? fixedAmount,
        decimal? rate,
        CorridorId corridorId,
        ServiceId serviceId,
        AffiliateId? affiliateId)
    {
        var entity = new Pricing
        {
            Id = id,
            Code = code,
            Channel = channel,
            MinimumAmount = minimumAmount,
            MaximumAmount = maximumAmount,
            FixedAmount = fixedAmount,
            Rate = rate,
            CorridorId = corridorId,
            ServiceId = serviceId,
            AffiliateId = affiliateId,
        };

        entity.AddDomainEvent(new PricingCreatedEvent(
            entity.Id,
            entity.Code,
            entity.ServiceId,
            entity.CorridorId,
            DateTime.UtcNow));

        return entity;
    }

}