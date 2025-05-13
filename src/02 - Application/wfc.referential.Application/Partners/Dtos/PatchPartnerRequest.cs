namespace wfc.referential.Application.Partners.Dtos;

public record PatchPartnerRequest
{
    /// <summary>
    /// The ID of the Partner to patch.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f8</example>
    public Guid PartnerId { get; init; }

    /// <summary>
    /// If provided, updates the code. If omitted, code remains unchanged.
    /// </summary>
    /// <example>PTN002</example>
    public string? Code { get; init; }

    /// <summary>
    /// If provided, updates the label. If omitted, label remains unchanged.
    /// </summary>
    /// <example>Service Premium</example>
    public string? Label { get; init; }

    /// <summary>
    /// If provided, updates the network mode. If omitted, network mode remains unchanged.
    /// </summary>
    /// <example>Succursale</example>
    public string? NetworkMode { get; init; }

    /// <summary>
    /// If provided, updates the payment mode. If omitted, payment mode remains unchanged.
    /// </summary>
    /// <example>PostPaye</example>
    public string? PaymentMode { get; init; }

    /// <summary>
    /// If provided, updates the internal partner identifier. If omitted, partner ID remains unchanged.
    /// </summary>
    /// <example>ID002</example>
    public string? IdPartner { get; init; }

    /// <summary>
    /// If provided, updates the support account type. If omitted, support account type remains unchanged.
    /// </summary>
    /// <example>Individuel</example>
    public string? SupportAccountType { get; init; }

    /// <summary>
    /// If provided, updates the identification number. If omitted, identification number remains unchanged.
    /// </summary>
    /// <example>TAX654321</example>
    public string? IdentificationNumber { get; init; }

    /// <summary>
    /// If provided, updates the tax regime. If omitted, tax regime remains unchanged.
    /// </summary>
    /// <example>Simplified</example>
    public string? TaxRegime { get; init; }

    /// <summary>
    /// If provided, updates the auxiliary account. If omitted, auxiliary account remains unchanged.
    /// </summary>
    /// <example>AUX002</example>
    public string? AuxiliaryAccount { get; init; }

    /// <summary>
    /// If provided, updates the ICE. If omitted, ICE remains unchanged.
    /// </summary>
    /// <example>ICE987654321</example>
    public string? ICE { get; init; }

    /// <summary>
    /// If provided, updates the enabled status. If omitted, enabled status remains unchanged.
    /// </summary>
    /// <example>false</example>
    public bool? IsEnabled { get; init; }

    /// <summary>
    /// If provided, updates the logo. If omitted, logo remains unchanged.
    /// </summary>
    /// <example>/logos/partner002.png</example>
    public string? Logo { get; init; }

    /// <summary>
    /// If provided, updates the sector. If omitted, sector remains unchanged.
    /// </summary>
    /// <example>8c583b69-6e16-5b2c-9c8f-69627ee725d3</example>
    public Guid? SectorId { get; init; }

    /// <summary>
    /// If provided, updates the city. If omitted, city remains unchanged.
    /// </summary>
    /// <example>9c583b69-6e16-5b2c-9c8f-69627ee725d4</example>
    public Guid? CityId { get; init; }
}