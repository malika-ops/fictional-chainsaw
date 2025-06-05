using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.AffiliateAggregate.Events;
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

    public static Affiliate Create(
        AffiliateId id,
        string code,
        string name,
        string abbreviation,
        DateTime? openingDate,
        string cancellationDay,
        string logo,
        decimal thresholdBilling,
        string accountingDocumentNumber,
        string accountingAccountNumber,
        string stampDutyMention,
        CountryId countryId)
    {
        var affiliate = new Affiliate
        {
            Id = id,
            Code = code,
            Name = name,
            Abbreviation = abbreviation,
            OpeningDate = openingDate,
            CancellationDay = cancellationDay,
            Logo = logo,
            ThresholdBilling = thresholdBilling,
            AccountingDocumentNumber = accountingDocumentNumber,
            AccountingAccountNumber = accountingAccountNumber,
            StampDutyMention = stampDutyMention,
            CountryId = countryId,
        };

        affiliate.AddDomainEvent(new AffiliateCreatedEvent(
            affiliate.Id.Value,
            affiliate.Code,
            affiliate.Name,
            affiliate.Abbreviation,
            affiliate.OpeningDate,
            affiliate.CancellationDay,
            affiliate.Logo,
            affiliate.ThresholdBilling,
            affiliate.AccountingDocumentNumber,
            affiliate.AccountingAccountNumber,
            affiliate.StampDutyMention,
            affiliate.CountryId,
            affiliate.IsEnabled,
            DateTime.UtcNow));

        return affiliate;
    }

    public void Update(
        string code,
        string name,
        string abbreviation,
        DateTime? openingDate,
        string cancellationDay,
        string logo,
        decimal thresholdBilling,
        string accountingDocumentNumber,
        string accountingAccountNumber,
        string stampDutyMention,
        CountryId countryId,
        bool? isEnabled 
        )
    {
        Code = code;
        Name = name;
        Abbreviation = abbreviation;
        OpeningDate = openingDate;
        CancellationDay = cancellationDay;
        Logo = logo;
        ThresholdBilling = thresholdBilling;
        AccountingDocumentNumber = accountingDocumentNumber;
        AccountingAccountNumber = accountingAccountNumber;
        StampDutyMention = stampDutyMention;
        CountryId = countryId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new AffiliateUpdatedEvent(
            Id.Value,
            Code,
            Name,
            Abbreviation,
            OpeningDate,
            CancellationDay,
            Logo,
            ThresholdBilling,
            AccountingDocumentNumber,
            AccountingAccountNumber,
            StampDutyMention,
            CountryId,
            IsEnabled,
            DateTime.UtcNow));
    }

    public void Patch(
        string? code,
        string? name,
        string? abbreviation,
        DateTime? openingDate,
        string? cancellationDay,
        string? logo,
        decimal? thresholdBilling,
        string? accountingDocumentNumber,
        string? accountingAccountNumber,
        string? stampDutyMention,
        CountryId? countryId,
        bool? isEnabled)
    {
        Code = code ?? Code;
        Name = name ?? Name;
        Abbreviation = abbreviation ?? Abbreviation;
        OpeningDate = openingDate ?? OpeningDate;
        CancellationDay = cancellationDay ?? CancellationDay;
        Logo = logo ?? Logo;
        ThresholdBilling = thresholdBilling ?? ThresholdBilling;
        AccountingDocumentNumber = accountingDocumentNumber ?? AccountingDocumentNumber;
        AccountingAccountNumber = accountingAccountNumber ?? AccountingAccountNumber;
        StampDutyMention = stampDutyMention ?? StampDutyMention;
        CountryId = countryId ?? CountryId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new AffiliatePatchedEvent(
            Id.Value,
            Code,
            Name,
            Abbreviation,
            OpeningDate,
            CancellationDay,
            Logo,
            ThresholdBilling,
            AccountingDocumentNumber,
            AccountingAccountNumber,
            StampDutyMention,
            CountryId,
            IsEnabled,
            DateTime.UtcNow));
    }

    // ParamType setter for AffiliateType (enum replacement)
    public void SetAffiliateType(ParamTypeId affiliateTypeId)
    {
        AffiliateTypeId = affiliateTypeId;
    }

    public void SetCountry(CountryId countryId, Country? country = null)
    {
        CountryId = countryId;
        if (country != null)
            Country = country;
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new AffiliateDisabledEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new AffiliateActivatedEvent(
            Id.Value,
            DateTime.UtcNow));
    }
}