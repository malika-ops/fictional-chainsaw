using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxAggrTaxRuleDetailsAggregateegate.Events;
using wfc.referential.Domain.TaxRuleDetailAggregate.Events;

namespace wfc.referential.Domain.TaxRuleDetailAggregate;
public class TaxRuleDetail : Aggregate<TaxRuleDetailsId>
{
    public CorridorId CorridorId { get; private set; }
    public TaxId TaxId { get; private set; }
    public ServiceId ServiceId { get; private set; }
    public ApplicationRule AppliedOn { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    public Corridor Corridor { get; private set; }
    public Tax Tax { get; private set; }
    public Service Service { get; private set; }

    private TaxRuleDetail() { }

    public static TaxRuleDetail Create(TaxRuleDetailsId id, CorridorId corridorId, TaxId taxId,
        ServiceId serviceId, ApplicationRule appliedOn, bool isEnabled = true)
    {
        var tax = new TaxRuleDetail
        {
            Id = id,
            CorridorId = corridorId,
            TaxId = taxId,
            ServiceId = serviceId,
            AppliedOn = appliedOn,
            IsEnabled = isEnabled
        };

        tax.AddDomainEvent(new TaxRuleDetailCreatedEvent(tax));
        return tax;
    }

    public void Update(TaxRuleDetailsId taxRuleDetailsId, CorridorId corridorId, TaxId taxId, ServiceId serviceId, ApplicationRule? appliedOn, bool isEnabled)
    {
        Id = taxRuleDetailsId;
        CorridorId = corridorId;
        TaxId = taxId;
        ServiceId = serviceId;
        AppliedOn = appliedOn ?? AppliedOn;
        IsEnabled = isEnabled;

        AddDomainEvent(new TaxRuleDetailUpdatedEvent(this));
    }
    public void SetInactive()
    {
        IsEnabled = false;

        AddDomainEvent(new TaxRuleDetailStatusChangedEvent(this));
    }
    public void Patch(TaxRuleDetailsId taxRuleDetailsId, CorridorId? corridorId, TaxId? taxId, ServiceId? serviceId, ApplicationRule? appliedOn, bool? isEnabled)
    {
        Id = taxRuleDetailsId;
        CorridorId = corridorId ?? CorridorId;
        TaxId = taxId ?? TaxId;
        ServiceId = serviceId ?? ServiceId;
        AppliedOn = appliedOn ?? AppliedOn;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new TaxRuleDetailPatchedEvent(this));
    }

}