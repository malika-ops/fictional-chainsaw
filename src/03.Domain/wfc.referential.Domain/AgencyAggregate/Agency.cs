using BuildingBlocks.Core.Abstraction.Domain;
using System.Security.Cryptography;
using wfc.referential.Domain.AgencyAggregate.Events;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Domain.AgencyAggregate;

public class Agency : Aggregate<AgencyId>
{
    public string Code { get; private set; } = string.Empty; 
    public string Name { get; private set; } = string.Empty;  
    public string Abbreviation { get; private set; } = string.Empty; 
    public string Address1 { get; private set; } = string.Empty;  
    public string? Address2 { get; private set; }  
    public string Phone { get; private set; } = string.Empty; 
    public string Fax { get; private set; } = string.Empty;  
    public string AccountingSheetName { get; private set; } = string.Empty;  
    public string AccountingAccountNumber { get; private set; } = string.Empty;   
    public string PostalCode { get; private set; } = string.Empty;  
    public decimal? Latitude { get; private set; }  
    public decimal? Longitude { get; private set; }
    public string? CashTransporter { get; private set; }
    public string? ExpenseFundAccountingSheet { get; private set; }
    public string? ExpenseFundAccountNumber { get; private set; }
    public string? MadAccount { get; private set; }
    public decimal? FundingThreshold { get; private set; }
    public bool IsEnabled { get; private set; } = true;  

    public ParamTypeId? AgencyTypeId { get; private set; }   
    public ParamType? AgencyType { get; private set; }
    public ParamTypeId? TokenUsageStatusId { get; private set; }
    public ParamType? TokenUsageStatus { get; private set; }
    public ParamTypeId? FundingTypeId { get; private set; }
    public ParamType? FundingType { get; private set; }


    public CityId? CityId { get; private set; }   
    public City? City { get; private set; }
    public SectorId? SectorId { get; private set; }   
    public Sector? Sector { get; private set; }
    public PartnerId? PartnerId { get; private set; }
    public Partner? Partner { get; private set; }
    public SupportAccountId? SupportAccountId { get; private set; }
    public SupportAccount? SupportAccount { get; private set; }


    private Agency() { }

    public static Agency Create(
        AgencyId id,
        string code,
        string name,
        string abbreviation,
        string address1,
        string? address2,
        string phone,
        string fax,
        string accountingSheetName,
        string accountingAccountNumber,
        string postalCode,
        decimal? latitude,
        decimal? longitude,
        string? cashTransporter,
        string? expenseFundAccountingSheet,
        string? expenseFundAccountNumber,
        string? madAccount,
        decimal? fundingThreshold,
        CityId? cityId,
        SectorId? sectorId,
        ParamTypeId? agencyTypeId,
        ParamTypeId? tokenUsageStatusId,
        ParamTypeId? fundingTypeId,
        PartnerId? partnerId,
        SupportAccountId? supportAccountId)
    {
        if ((cityId is null && sectorId is null) || (cityId is not null && sectorId is not null))
            throw new ArgumentException("Exactly one of CityId or SectorId must be provided.");

        var agency = new Agency
        {
            Id = id,
            Code = code,
            Name = name,
            Abbreviation = abbreviation,
            Address1 = address1,
            Address2 = address2,
            Phone = phone,
            Fax = fax,
            AccountingSheetName = accountingSheetName,
            AccountingAccountNumber = accountingAccountNumber,
            PostalCode = postalCode,
            Latitude = latitude,
            Longitude = longitude,
            CashTransporter = cashTransporter,
            ExpenseFundAccountingSheet = expenseFundAccountingSheet,
            ExpenseFundAccountNumber = expenseFundAccountNumber,
            MadAccount = madAccount,
            FundingThreshold = fundingThreshold,
            CityId = cityId,
            SectorId = sectorId,
            AgencyTypeId = agencyTypeId,
            TokenUsageStatusId = tokenUsageStatusId,
            FundingTypeId = fundingTypeId,
            PartnerId = partnerId,
            SupportAccountId = supportAccountId
        };

        agency.AddDomainEvent(new AgencyCreatedEvent(
            agency.Id.Value,
            agency.Code,
            agency.Name,
            DateTime.UtcNow));

        return agency;
    }

    /// <summary>
    /// generates a random 6-digit agency code.
    /// example returned values: "000001", "123456", "999999"
    /// </summary>
    public static string GenerateAgencyCode()
    {
        // 000000 → 999999  (6-digit zero-padded)
        Span<byte> buf = stackalloc byte[4];
        RandomNumberGenerator.Fill(buf);
        var n = BitConverter.ToUInt32(buf) % 1_000_000U;
        return n.ToString("D6");
    }

    public void Update(
    string code,
    string name,
    string abbreviation,
    string address1,
    string? address2,
    string phone,
    string fax,
    string accountingSheetName,
    string accountingAccountNumber,
    string postalCode,
    decimal? latitude,
    decimal? longitude,
    string? cashTransporter,
    string? expenseFundAccountingSheet,
    string? expenseFundAccountNumber,
    string? madAccount,
    decimal? fundingThreshold,
    bool? isEnabled,
    Guid? cityId,
    Guid? sectorId,
    Guid? agencyTypeId,
    Guid? tokenUsageStatusId,
    Guid? fundingTypeId,
    Guid? partnerId,
    Guid? supportAccountId)
    {
        if ((cityId.HasValue && sectorId.HasValue) ||
            (!cityId.HasValue && !sectorId.HasValue &&
             CityId is null && SectorId is null))
            throw new ArgumentException("Exactly one of CityId or SectorId must be provided.");

        Code = code;
        Name = name;
        Abbreviation = abbreviation;
        Address1 = address1;
        Address2 = address2 ?? Address2;
        Phone = phone;
        Fax = fax;
        AccountingSheetName = accountingSheetName;
        AccountingAccountNumber = accountingAccountNumber;
        PostalCode = postalCode;
        Latitude = latitude ?? Latitude;
        Longitude = longitude ?? Longitude;
        CashTransporter = cashTransporter ?? CashTransporter;
        ExpenseFundAccountingSheet = expenseFundAccountingSheet ?? ExpenseFundAccountingSheet;
        ExpenseFundAccountNumber = expenseFundAccountNumber ?? ExpenseFundAccountNumber;
        MadAccount = madAccount ?? MadAccount;
        FundingThreshold = fundingThreshold ?? FundingThreshold;
        if (isEnabled.HasValue) IsEnabled = isEnabled.Value;

        CityId = cityId.HasValue ? CityId.Of(cityId.Value) : CityId;
        SectorId = sectorId.HasValue ? SectorId.Of(sectorId.Value) : SectorId;
        AgencyTypeId = agencyTypeId.HasValue ? ParamTypeId.Of(agencyTypeId.Value) : AgencyTypeId;
        TokenUsageStatusId = tokenUsageStatusId.HasValue ? ParamTypeId.Of(tokenUsageStatusId.Value) : TokenUsageStatusId;
        FundingTypeId = fundingTypeId.HasValue ? ParamTypeId.Of(fundingTypeId.Value) : FundingTypeId;
        PartnerId = partnerId.HasValue ? PartnerId.Of(partnerId.Value) : PartnerId;
        SupportAccountId = supportAccountId.HasValue ? SupportAccountId.Of(supportAccountId.Value) : SupportAccountId;

        AddDomainEvent(new AgencyUpdatedEvent(
            Id!.Value,
            Code,
            Name,
            DateTime.UtcNow));
    }

    public void Patch(
    string? code,
    string? name,
    string? abbreviation,
    string? address1,
    string? address2,
    string? phone,
    string? fax,
    string? accountingSheetName,
    string? accountingAccountNumber,
    string? postalCode,
    decimal? latitude,
    decimal? longitude,
    string? cashTransporter,
    string? expenseFundAccountingSheet,
    string? expenseFundAccountNumber,
    string? madAccount,
    decimal? fundingThreshold,
    bool? isEnabled,
    Guid? cityId,
    Guid? sectorId,
    Guid? agencyTypeId,
    Guid? tokenUsageStatusId,
    Guid? fundingTypeId,
    Guid? partnerId,
    Guid? supportAccountId)
    {

        Code = code ?? Code;
        Name = name ?? Name;
        Abbreviation = abbreviation ?? Abbreviation;
        Address1 = address1 ?? Address1;
        Address2 = address2 ?? Address2;
        Phone = phone ?? Phone;
        Fax = fax ?? Fax;
        AccountingSheetName = accountingSheetName ?? AccountingSheetName;
        AccountingAccountNumber = accountingAccountNumber ?? AccountingAccountNumber;
        PostalCode = postalCode ?? PostalCode;
        Latitude = latitude ?? Latitude;
        Longitude = longitude ?? Longitude;
        CashTransporter = cashTransporter ?? CashTransporter;
        ExpenseFundAccountingSheet = expenseFundAccountingSheet ?? ExpenseFundAccountingSheet;
        ExpenseFundAccountNumber = expenseFundAccountNumber ?? ExpenseFundAccountNumber;
        MadAccount = madAccount ?? MadAccount;
        FundingThreshold = fundingThreshold ?? FundingThreshold;
        if (isEnabled.HasValue) IsEnabled = isEnabled.Value;

        CityId = cityId.HasValue ? CityId.Of(cityId.Value) : CityId;
        SectorId = sectorId.HasValue ? SectorId.Of(sectorId.Value) : SectorId;
        AgencyTypeId = agencyTypeId.HasValue ? ParamTypeId.Of(agencyTypeId.Value) : AgencyTypeId;
        TokenUsageStatusId = tokenUsageStatusId.HasValue ? ParamTypeId.Of(tokenUsageStatusId.Value) : TokenUsageStatusId;
        FundingTypeId = fundingTypeId.HasValue ? ParamTypeId.Of(fundingTypeId.Value) : FundingTypeId;
        PartnerId = partnerId.HasValue ? PartnerId.Of(partnerId.Value) : PartnerId;
        SupportAccountId = supportAccountId.HasValue ? SupportAccountId.Of(supportAccountId.Value) : SupportAccountId;

        AddDomainEvent(new AgencyPatchedEvent(
            Id!.Value,
            Code,
            Name,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new AgencyDisabledEvent(
            Id.Value,
            DateTime.UtcNow
        ));
    }

    
}
