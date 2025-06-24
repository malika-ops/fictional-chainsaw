namespace wfc.referential.Application.Agencies.Dtos;

public record GetFiltredAgenciesRequest : FilterRequest
{

    /// <summary>Filter by agency code.</summary>            
    public string? Code { get; init; }
    /// <summary>Filter by agency name.</summary>            
    public string? Name { get; init; }
    /// <summary>Filter by abbreviation.</summary>           
    public string? Abbreviation { get; init; }
    /// <summary>Filter by address (Address1 or Address2).</summary> 
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? Fax { get; init; }

    public string? AccountingSheetName { get; init; }
    public string? AccountingAccountNumber { get; init; }

    public string? PostalCode { get; init; }
    public string? CashTransporter { get; init; }
    public decimal? FundingThreshold { get; init; }

    public Guid? CityId { get; init; }
    public Guid? SectorId { get; init; }

    public Guid? AgencyTypeId { get; init; }
    public string? AgencyTypeValue { get; init; }
    public string? AgencyTypeLibelle { get; init; }

    public Guid? FundingTypeId { get; init; }
    public Guid? TokenUsageStatusId { get; init; }

}
