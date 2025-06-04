using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Domain.AffiliateAggregate;

public class Affiliate : Aggregate<AffiliateId>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Abbreviation { get; private set; } = string.Empty;
    public DateTime? OpeningDate { get; private set; }
    public string CancellationDay { get; private set; } = string.Empty;
    public string Logo { get; private set; } = string.Empty;
    public decimal ThresholdBilling { get; private set; }
    public string AccountingDocumentNumber { get; private set; } = string.Empty;
    public string AccountingAccountNumber { get; private set; } = string.Empty;
    public string StampDutyMention { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;

    // Foreign key relationship to Country
    public CountryId CountryId { get; private set; }
    public Country? Country { get; private set; }

    // ParamType relationships for AffiliateType 
    public ParamTypeId? AffiliateTypeId { get; private set; }
    public ParamType? AffiliateType { get; private set; }

    private Affiliate() { }
}

