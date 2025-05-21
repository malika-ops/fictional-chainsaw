using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.TaxAggregate.Events;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Domain.TaxAggregate;
public class Tax : Aggregate<TaxId>
{
    public string Code { get; private set; }
    public string CodeEn { get; private set; }
    public string CodeAr { get; private set; }
    public string Description { get; private set; }
    public double? FixedAmount { get; private set; }
    public double? Rate { get; private set; }
    public bool IsEnabled { get; private set; } = true;

    public ICollection<TaxRuleDetail> TaxRuleDetails { get; set; }
    private Tax() { }

    public static Tax Create(TaxId id, string code, string codeEn, string codeAr, string description,
        double? fixedAmount, double? rate)
    {
        var tax = new Tax
        {
            Id = id,
            Code = code,
            CodeEn = codeEn,
            CodeAr = codeAr,
            Description = description,
            FixedAmount = fixedAmount,
            Rate = rate,
        };

        tax.AddDomainEvent(new TaxCreatedEvent(tax));
        return tax;
    }

    public void Update()
    {
        AddDomainEvent(new TaxUpdatedEvent(this));
    }
    public void SetInactive()
    {
        IsEnabled = false;

        AddDomainEvent(new TaxStatusChangedEvent(this));
    }
    public void Patch()
    {
        AddDomainEvent(new TaxPatchedEvent(this));
    }

}