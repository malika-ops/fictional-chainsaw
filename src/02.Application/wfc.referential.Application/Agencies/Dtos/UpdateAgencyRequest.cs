using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Agencies.Dtos;

public record UpdateAgencyRequest
{
    /// <summary>Agency GUID (taken from the route).</summary>
    [Required] public Guid AgencyId { get; init; }

    /// <summary>Unique agency code.</summary>
    [Required] public string Code { get; init; } = string.Empty;

    /// <summary>Display name.</summary>
    [Required] public string Name { get; init; } = string.Empty;

    /// <summary>Short abbreviation.</summary>
    [Required] public string Abbreviation { get; init; } = string.Empty;

    /// <summary>Primary street address.</summary>
    [Required] public string Address1 { get; init; } = string.Empty;

    /// <summary>Optional secondary address.</summary>
    public string? Address2 { get; init; }

    /// <summary>Phone number.</summary>
    [Required] public string Phone { get; init; } = string.Empty;

    /// <summary>Fax number.</summary>
    public string Fax { get; init; } = string.Empty;

    /// <summary>Accounting sheet name.</summary>
    public string AccountingSheetName { get; init; } = string.Empty;

    /// <summary>Accounting account number.</summary>
    public string AccountingAccountNumber { get; init; } = string.Empty;

    /// <summary>MoneyGram reference number.</summary>
    public string MoneyGramReferenceNumber { get; init; } = string.Empty;

    /// <summary>MoneyGram password (encrypted at rest).</summary>
    public string MoneyGramPassword { get; init; } = string.Empty;

    /// <summary>Postal / ZIP code.</summary>
    public string PostalCode { get; init; } = string.Empty;

    /// <summary>Permission office change reference.</summary>
    public string PermissionOfficeChange { get; init; } = string.Empty;

    /// <summary>Latitude in decimal degrees.</summary>
    public decimal? Latitude { get; init; }

    /// <summary>Longitude in decimal degrees.</summary>
    public decimal? Longitude { get; init; }

    /// <summary>City identifier (mutually exclusive with SectorId).</summary>
    public Guid? CityId { get; init; }

    /// <summary>Sector identifier (mutually exclusive with CityId).</summary>
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
