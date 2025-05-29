namespace wfc.referential.Application.Partners.Dtos;

public record UpdatePartnerRequest
{
    /// <summary>
    /// The ID of the Partner to update.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f8</example>
    public Guid PartnerId { get; init; }

    /// <summary>
    /// A unique code identifier for the Partner.
    /// </summary>
    /// <example>PTN001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// The name of the Partner.
    /// </summary>
    /// <example>Service Express</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The person type of the Partner.
    /// </summary>
    /// <example>Natural Person</example>
    public string PersonType { get; init; } = string.Empty;

    /// <summary>
    /// The professional tax number of the Partner.
    /// </summary>
    /// <example>PTX123456</example>
    public string ProfessionalTaxNumber { get; init; } = string.Empty;

    /// <summary>
    /// The withholding tax rate of the Partner.
    /// </summary>
    /// <example>10.5</example>
    public string WithholdingTaxRate { get; init; } = string.Empty;

    /// <summary>
    /// The headquarters city of the Partner.
    /// </summary>
    /// <example>Casablanca</example>
    public string HeadquartersCity { get; init; } = string.Empty;

    /// <summary>
    /// The headquarters address of the Partner.
    /// </summary>
    /// <example>123 Main Street</example>
    public string HeadquartersAddress { get; init; } = string.Empty;

    /// <summary>
    /// The last name of the Partner contact.
    /// </summary>
    /// <example>Doe</example>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// The first name of the Partner contact.
    /// </summary>
    /// <example>John</example>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// The phone number contact of the Partner.
    /// </summary>
    /// <example>+212612345678</example>
    public string PhoneNumberContact { get; init; } = string.Empty;

    /// <summary>
    /// The mail contact of the Partner.
    /// </summary>
    /// <example>contact@partner.com</example>
    public string MailContact { get; init; } = string.Empty;

    /// <summary>
    /// The function contact of the Partner.
    /// </summary>
    /// <example>Manager</example>
    public string FunctionContact { get; init; } = string.Empty;

    /// <summary>
    /// The transfer type of the Partner.
    /// </summary>
    /// <example>Bank Transfer</example>
    public string TransferType { get; init; } = string.Empty;

    /// <summary>
    /// The authentication mode of the Partner.
    /// </summary>
    /// <example>SMS</example>
    public string AuthenticationMode { get; init; } = string.Empty;

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
    /// Path or URL to the logo of the Partner.
    /// </summary>
    /// <example>/logos/partner001.png</example>
    public string Logo { get; init; } = string.Empty;

    /// <summary>
    /// Whether the Partner is enabled or not.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// The ID of the Network Mode parameter type.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? NetworkModeId { get; init; }

    /// <summary>
    /// The ID of the Payment Mode parameter type.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa7</example>
    public Guid? PaymentModeId { get; init; }

    /// <summary>
    /// The ID of the Partner Type parameter type.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa8</example>
    public Guid? PartnerTypeId { get; init; }

    /// <summary>
    /// The ID of the Support Account Type parameter type.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa9</example>
    public Guid? SupportAccountTypeId { get; init; }

    /// <summary>
    /// The ID of the Commission Account.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afaa</example>
    public Guid? CommissionAccountId { get; init; }

    /// <summary>
    /// The ID of the Activity Account.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afab</example>
    public Guid? ActivityAccountId { get; init; }

    /// <summary>
    /// The ID of the Support Account.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afac</example>
    public Guid? SupportAccountId { get; init; }

    /// <summary>
    /// The ID of the parent Partner (for hierarchical relationships).
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afad</example>
    public Guid? IdParent { get; init; }
}
