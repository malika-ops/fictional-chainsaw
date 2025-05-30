using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Agencies.Dtos;

public record UpdateAgencyRequest
{

    /// <summary>Agency GUID (from route).</summary>
    [Required] public Guid AgencyId { get; init; }

    /// <summary>Agency code – exactly 6 digits.</summary>
    [Required] public string Code { get; init; } = string.Empty;

    /// <summary>Display name.</summary>
    [Required] public string Name { get; init; } = string.Empty;

    /// <summary>Short abbreviation.</summary>
    [Required] public string Abbreviation { get; init; } = string.Empty;

    /// <summary>Primary address line.</summary>
    [Required] public string Address1 { get; init; } = string.Empty;

    /// <summary>Secondary address line (optional).</summary>
    public string? Address2 { get; init; }

    /// <summary>Phone number.</summary>
    [Required] public string Phone { get; init; } = string.Empty;

    /// <summary>Fax number.</summary>
    public string Fax { get; init; } = string.Empty;

    /// <summary>Accounting sheet name.</summary>
    [Required] public string AccountingSheetName { get; init; } = string.Empty;

    /// <summary>Accounting account number.</summary>
    [Required] public string AccountingAccountNumber { get; init; } = string.Empty;

    /// <summary>Sheet name for expense fund (optional).</summary>
    public string? ExpenseFundAccountingSheet { get; init; }

    /// <summary>Account number for expense fund (optional).</summary>
    public string? ExpenseFundAccountNumber { get; init; }

    /// <summary>MAD (dirham) account identifier (optional).</summary>
    public string? MadAccount { get; init; }

    /// <summary>Postal / ZIP code.</summary>
    [Required] public string PostalCode { get; init; } = string.Empty;

    /// <summary>Name of the cash-transport provider (optional).</summary>
    public string? CashTransporter { get; init; }

    /// <summary>Threshold amount (optional).</summary>
    public decimal? FundingThreshold { get; init; }

    /// <summary>Latitude in decimal degrees (optional).</summary>
    public decimal? Latitude { get; init; }

    /// <summary>Longitude in decimal degrees (optional).</summary>
    public decimal? Longitude { get; init; }

    /// <summary>City identifier (mutually exclusive with SectorId).</summary>
    public Guid? CityId { get; init; }

    /// <summary>Sector identifier (mutually exclusive with CityId).</summary>
    public Guid? SectorId { get; init; }

    /// <summary>Agency-type ParamType identifier (optional).</summary>
    public Guid? AgencyTypeId { get; init; }

    /// <summary>Funding-type ParamType identifier (optional).</summary>
    public Guid? FundingTypeId { get; init; }

    /// <summary>Token-usage-status ParamType identifier (optional).</summary>
    public Guid? TokenUsageStatusId { get; init; }

    /// <summary>Partner identifier (optional).</summary>
    public Guid? PartnerId { get; init; }

    /// <summary>Support account identifier (optional).</summary>
    public Guid? SupportAccountId { get; init; }

    /// <summary>Enable / disable the agency.</summary>
    public bool? IsEnabled { get; init; } = true;

}
