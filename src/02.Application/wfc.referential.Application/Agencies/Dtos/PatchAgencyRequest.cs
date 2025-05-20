using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Agencies.Dtos;

public record PatchAgencyRequest
{
    /// <summary>Agency GUID (from route).</summary>
    [Required] public Guid AgencyId { get; init; }

    /// <summary>Unique agency code.</summary>
    public string? Code { get; init; }

    /// <summary>Display name.</summary>
    public string? Name { get; init; }

    /// <summary>Short abbreviation.</summary>
    public string? Abbreviation { get; init; }

    /// <summary>Primary street address.</summary>
    public string? Address1 { get; init; }

    /// <summary>Optional secondary address.</summary>
    public string? Address2 { get; init; }

    /// <summary>Phone number.</summary>
    public string? Phone { get; init; }

    /// <summary>Fax number.</summary>
    public string? Fax { get; init; }

    /// <summary>Accounting sheet name.</summary>
    public string? AccountingSheetName { get; init; }

    /// <summary>Accounting account number.</summary>
    public string? AccountingAccountNumber { get; init; }

    /// <summary>MoneyGram reference.</summary>
    public string? MoneyGramReferenceNumber { get; init; }

    /// <summary>MoneyGram password.</summary>
    public string? MoneyGramPassword { get; init; }

    /// <summary>Postal / ZIP code.</summary>
    public string? PostalCode { get; init; }

    /// <summary>Permission office change string.</summary>
    public string? PermissionOfficeChange { get; init; }

    /// <summary>Latitude (decimal degrees).</summary>
    public decimal? Latitude { get; init; }

    /// <summary>Longitude (decimal degrees).</summary>
    public decimal? Longitude { get; init; }

    /// <summary>City identifier (exclusive with SectorId).</summary>
    public Guid? CityId { get; init; }

    /// <summary>Sector identifier (exclusive with CityId).</summary>
    public Guid? SectorId { get; init; }

    /// <summary>Agency-type ParamType identifier.</summary>
    public Guid? AgencyTypeId { get; init; }

    /// <summary>External support-account Id.</summary>
    public string? SupportAccountId { get; init; }

    /// <summary>External partner Id.</summary>
    public string? PartnerId { get; init; }

    /// <summary>Agency status (enabled/disabled).</summary>
    public bool? IsEnabled { get; init; } = true;
}
