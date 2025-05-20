namespace wfc.referential.Application.Agencies.Dtos;

public record GetAgenciesResponse(
   Guid AgencyId,
    string Code,
    string Name,
    string Abbreviation,
    string Address1,
    string? Address2,
    string Phone,
    string Fax,
    string AccountingSheetName,
    string AccountingAccountNumber,
    string MoneyGramReferenceNumber,
    string PostalCode,
    string PermissionOfficeChange,
    Guid? CityId,
    Guid? SectorId,
    Guid? AgencyTypeId,
    string? AgencyTypeLibelle,
    string? AgencyTypeValue,
    bool IsEnabled,
    decimal? Latitude,
    decimal? Longitude
);
