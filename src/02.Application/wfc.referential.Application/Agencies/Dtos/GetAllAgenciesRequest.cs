using wfc.referential.Domain.AgencyAggregate;

namespace wfc.referential.Application.Agencies.Dtos;

public record GetAllAgenciesRequest
{

    /// <summary>Page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Page size  (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Filter by agency code.</summary>
    public string? Code { get; init; }

    /// <summary>Filter by agency name.</summary>
    public string? Name { get; init; }

    /// <summary>Filter by abbreviation.</summary>
    public string? Abbreviation { get; init; }

    /// <summary>Filter by address (Address1 or Address2).</summary>
    public string? Address { get; init; }

    /// <summary>Filter by phone.</summary>
    public string? Phone { get; init; }

    /// <summary>Filter by fax.</summary>
    public string? Fax { get; init; }

    /// <summary>Filter by accounting sheet name.</summary>
    public string? AccountingSheetName { get; init; }

    /// <summary>Filter by accounting account number.</summary>
    public string? AccountingAccountNumber { get; init; }

    /// <summary>Filter by MoneyGram reference number.</summary>
    public string? MoneyGramReferenceNumber { get; init; }

    /// <summary>Filter by postal code.</summary>
    public string? PostalCode { get; init; }

    /// <summary>Filter by CityId.</summary>
    public Guid? CityId { get; init; }

    /// <summary>Filter by SectorId.</summary>
    public Guid? SectorId { get; init; }

    /// <summary>Filter by AgencyTypeId.</summary>
    public Guid? AgencyTypeId { get; init; }

    /// <summary>Filter by Agency-type value (e.g. “3G”).</summary>
    public string? AgencyTypeValue { get; init; }

    /// <summary>Filter by Agency-type libellé (e.g. “AgencyType”).</summary>
    public string? AgencyTypeLibelle { get; init; }

    /// <summary>Status filter (Enabled/Disabled).</summary>
    public bool? IsEnabled { get; init; } = true;
}
