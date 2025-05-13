using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Agencies.Dtos;

public record CreateAgencyRequest
{
    /// <summary>Unique agency code.</summary>
    /// <example>AGD01</example>
    [Required]
    public string Code { get; init; } = string.Empty;

    /// <summary> agency Name.</summary>
    /// <example>AGD01</example>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>Short abbreviation.</summary>
    /// <example>AGD</example>
    [Required]
    public string Abbreviation { get; init; } = string.Empty;

    /// <summary>Full address.</summary>
    /// <example>12 Hassan II Av., Rabat</example>
    [Required]
    public string Address1 { get; init; } = string.Empty;

    /// <summary>Full second address.</summary>
    /// <example>12 Hassan II Av., Rabat</example>
    public string? Address2 { get; init; }

    /// <summary>Contact phone number.</summary>
    /// <example>+212537123456</example>
    [Required]
    public string Phone { get; init; } = string.Empty;

    /// <summary>Contact Fax number.</summary>
    /// <example>+212537123456</example>
    public string Fax { get; init; } = string.Empty;

    /// <summary>Accounting sheet name.</summary>
    /// <example>FIN‑Sheet</example>
    [Required]
    public string AccountingSheetName { get; init; } = string.Empty;

    /// <summary>Accounting account number.</summary>
    /// <example>401122</example>
    [Required]
    public string AccountingAccountNumber { get; init; } = string.Empty;


    /// <summary>MoneyGram reference number.</summary>
    /// <example>MG‑SEQ</example>
    public string MoneyGramReferenceNumber { get; init; } = string.Empty;

    /// <summary>MoneyGram password.</summary>
    /// <example>secret!</example>
    public string MoneyGramPassword { get; init; } = string.Empty;

    /// <summary>Postal code.</summary>
    /// <example>10000</example>
    [Required]
    public string PostalCode { get; init; } = string.Empty;

    /// <summary>Exchange‑office agency code.</summary>
    /// <example>EXC‑01</example>
    public string PermissionOfficeChange { get; init; } = string.Empty;

    /// <summary>Latitude in decimal degrees.</summary>
    /// <example>34.020882</example>
    public decimal? Latitude { get; init; }

    /// <summary>Longitude in decimal degrees.</summary>
    /// <example>-6.841650</example>
    public decimal? Longitude { get; init; }

    /// <summary>Link to a city or sector — exactly one must be set.</summary>
    public Guid? CityId { get; init; }

    /// <summary>Link to a city or sector — exactly one must be set.</summary>
    public Guid? SectorId { get; init; }

    /// <summary>Optional Agency-type (ParamType) reference.</summary>
    public Guid? AgencyTypeId { get; init; }

    public string? SupportAccountId { get; init; }
    public string? PartnerId { get; init; }


}
