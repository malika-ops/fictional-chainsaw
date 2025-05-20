using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Partners.Dtos;

public record CreatePartnerRequest
{
    /// <summary>
    /// A unique code identifier for the Partner.
    /// </summary>
    /// <example>PTN001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// The label or name of the Partner.
    /// </summary>
    /// <example>Service Express</example>
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// The type of the Partner.
    /// </summary>
    /// <example>3G - Kiosque - Mobile</example>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// The network mode of the Partner.
    /// </summary>
    /// <example>Franchise</example>
    public string NetworkMode { get; init; } = string.Empty;

    /// <summary>
    /// The payment mode of the Partner.
    /// </summary>
    /// <example>PrePaye</example>
    public string PaymentMode { get; init; } = string.Empty;

    /// <summary>
    /// The internal parent partner identifier.
    /// </summary>
    /// <example>456e35ab-8791-4235-9d44-facfd89f324c</example>
    public Guid? IdParent { get; init; }

    /// <summary>
    /// The support account type associated with this Partner.
    /// </summary>
    /// <example>Commun</example>
    public string SupportAccountType { get; init; } = string.Empty;

    /// <summary>
    /// The tax identification number of the Partner.
    /// </summary>
    /// <example>TAX123456</example>
    public string TaxIdentificationNumber { get; init; } = string.Empty;

    /// <summary>
    /// The tax regime of the Partner.
    /// </summary>
    /// <example>Standard</example>
    public string TaxRegime { get; init; } = string.Empty;

    /// <summary>
    /// The auxiliary account reference.
    /// </summary>
    /// <example>AUX001</example>
    public string AuxiliaryAccount { get; init; } = string.Empty;

    /// <summary>
    /// The ICE (Identifiant Commun de l'Entreprise) of the Partner.
    /// </summary>
    /// <example>ICE123456789</example>
    public string ICE { get; init; } = string.Empty;

    /// <summary>
    /// The RAS Rate of the Partner.
    /// </summary>
    /// <example>10.5</example>
    public string RASRate { get; init; } = string.Empty;

    /// <summary>
    /// Path or URL to the logo of the Partner.
    /// </summary>
    /// <example>/logos/partner001.png</example>
    public string Logo { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the Commission Account related to this Partner.
    /// </summary>
    /// <example>7c583b69-6e16-5b2c-9c8f-69627ee725d3</example>
    public Guid? CommissionAccountId { get; init; }

    /// <summary>
    /// The ID of the Activity Account related to this Partner.
    /// </summary>
    /// <example>8c583b69-6e16-5b2c-9c8f-69627ee725d4</example>
    public Guid? ActivityAccountId { get; init; }

    /// <summary>
    /// The ID of the Support Account related to this Partner.
    /// </summary>
    /// <example>9c583b69-6e16-5b2c-9c8f-69627ee725d5</example>
    public Guid? SupportAccountId { get; init; }
}