using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.CorridorAggregate;
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

}