namespace wfc.referential.Application.Agencies.Dtos;

public record GetAgenciesResponse
{
    public Guid AgencyId { get;  init; } 
    public string Code { get; init; } = string.Empty;
    public string Name { get;  init; } = string.Empty;
    public string Abbreviation { get;  init; } = string.Empty;
    public string Address1 { get;  init; } = string.Empty;
    public string? Address2 { get;  init; } = string.Empty;
    public string Phone { get;  init; } = string.Empty;
    public string Fax { get;  init; } = string.Empty;
    public string AccountingSheetName { get;  init; } = string.Empty;
    public string AccountingAccountNumber { get;  init; } = string.Empty;
    public string MoneyGramReferenceNumber { get; init; } = string.Empty;
    public string PostalCode { get;  init; } = string.Empty;
    public string PermissionOfficeChange { get;  init; } = string.Empty;
    public Guid? CityId { get;  init; } 
    public Guid? SectorId { get;  init; } 
    public Guid? AgencyTypeId { get;  init; } 
    public string? AgencyTypeLibelle { get;  init; } = string.Empty;
    public string? AgencyTypeValue { get;  init; } = string.Empty;
    public bool IsEnabled { get;  init; } 
    public decimal? Latitude { get;  init; } 
    public decimal? Longitude { get;  init; }

};
