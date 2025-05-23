using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.AgencyAggregate.Events;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.SectorAggregate;

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
    public string MoneyGramReferenceNumber { get; private set; } = string.Empty; 
    public string MoneyGramPassword { get; private set; } = string.Empty;  
    public string PostalCode { get; private set; } = string.Empty;  
    public string PermissionOfficeChange { get; private set; } = string.Empty;  
    public decimal? Latitude { get; private set; }  
    public decimal? Longitude { get; private set; }  
    public bool IsEnabled { get; private set; } = true;

    public ParamTypeId? AgencyTypeId { get; private set; }
    public ParamType? AgencyType { get; private set; }
    public CityId? CityId { get; private set; }
    public City? City { get; private set; }
    public SectorId? SectorId { get; private set; }
    public Sector? Sector { get; private set; }

    public string? SupportAccountId { get; private set; } 
    public string? PartnerId { get; private set; }  

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
        string moneyGramReferenceNumber,
        string moneyGramPassword,
        string postalCode,
        string permissionOfficeChange,
        decimal? latitude,
        decimal? longitude,
        CityId? cityId,
        SectorId? sectorId,
        ParamTypeId? agencyTypeId,
        string? supportAccountId,
        string? partnerId)
    {

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
            MoneyGramReferenceNumber = moneyGramReferenceNumber,
            MoneyGramPassword = moneyGramPassword,
            PostalCode = postalCode,
            PermissionOfficeChange = permissionOfficeChange,
            Latitude = latitude,
            Longitude = longitude,
            CityId = cityId,
            SectorId = sectorId,
            AgencyTypeId = agencyTypeId,

            SupportAccountId = supportAccountId,
            PartnerId = partnerId
        };

        agency.AddDomainEvent(new AgencyCreatedEvent(
            agency.Id.Value,
            agency.Code,
            agency.Name,
            DateTime.UtcNow));

        return agency;
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
    string moneyGramReferenceNumber,
    string moneyGramPassword,
    string postalCode,
    string permissionOfficeChange,
    decimal? latitude,
    decimal? longitude,
    Guid? cityId,
    Guid? sectorId,
    Guid? agencyTypeId,
    string? supportAccountId,
    string? partnerId,
    bool? isEnabled)
    {
        Code = code;
        Name = name;
        Abbreviation  = abbreviation;
        Address1 = address1;
        Address2 = address2 ?? Address2;
        Phone = phone;
        Fax = fax;
        AccountingSheetName = accountingSheetName;
        AccountingAccountNumber = accountingAccountNumber;
        MoneyGramReferenceNumber = moneyGramReferenceNumber;
        MoneyGramPassword = moneyGramPassword;
        PostalCode = postalCode;
        PermissionOfficeChange = permissionOfficeChange;
        Latitude = latitude ?? Latitude;
        Longitude = longitude ?? Longitude;

        CityId = cityId.HasValue ? CityId.Of(cityId.Value) : CityId;
        SectorId = sectorId.HasValue ? SectorId.Of(sectorId.Value) : SectorId;
        AgencyTypeId = agencyTypeId.HasValue ? ParamTypeId.Of(agencyTypeId.Value) : AgencyTypeId;
        SupportAccountId = supportAccountId ?? SupportAccountId;
        PartnerId = partnerId ?? PartnerId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new AgencyUpdatedEvent(
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

    public void Patch()
    {
       
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
    string? moneyGramReferenceNumber,
    string? moneyGramPassword,
    string? postalCode,
    string? permissionOfficeChange,
    decimal? latitude,
    decimal? longitude,
    Guid? cityId,
    Guid? sectorId,
    Guid? agencyTypeId,
    string? supportAccountId,
    string? partnerId,
    bool? isEnabled)
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
        MoneyGramReferenceNumber = moneyGramReferenceNumber ?? MoneyGramReferenceNumber;
        MoneyGramPassword = moneyGramPassword ?? MoneyGramPassword;
        PostalCode = postalCode ?? PostalCode;
        PermissionOfficeChange = permissionOfficeChange ?? PermissionOfficeChange;
        Latitude = latitude ?? Latitude;
        Longitude = longitude ?? Longitude;

        CityId = cityId.HasValue ? CityId.Of(cityId.Value) : CityId;
        SectorId = sectorId.HasValue ? SectorId.Of(sectorId.Value) : SectorId;
        AgencyTypeId = agencyTypeId.HasValue ? ParamTypeId.Of(agencyTypeId.Value) : AgencyTypeId;
        SupportAccountId = supportAccountId ?? SupportAccountId;
        PartnerId = partnerId ?? PartnerId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new AgencyPatchedEvent(
           Id!.Value,
           Code,
           Name,
           DateTime.UtcNow));
    }

}
