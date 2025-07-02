namespace wfc.referential.Application.Partners.Dtos;

public record GetPartnersResponse
{
    /// <summary>
    /// Unique identifier of the partner.
    /// </summary>
    /// <example>c1a2b3d4-e5f6-7890-abcd-1234567890ef</example>
    public Guid PartnerId { get; init; }

    /// <summary>
    /// Unique code of the partner.
    /// </summary>
    /// <example>PART001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Name of the partner.
    /// </summary>
    /// <example>Acme Corporation</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Type of person (e.g., Individual, Company).
    /// </summary>
    /// <example>Company</example>
    public string PersonType { get; init; } = string.Empty;

    /// <summary>
    /// Professional tax number of the partner.
    /// </summary>
    /// <example>PTX123456</example>
    public string ProfessionalTaxNumber { get; init; } = string.Empty;

    /// <summary>
    /// Withholding tax rate applied to the partner.
    /// </summary>
    /// <example>10%</example>
    public string WithholdingTaxRate { get; init; } = string.Empty;

    /// <summary>
    /// City where the partner's headquarters is located.
    /// </summary>
    /// <example>Casablanca</example>
    public string HeadquartersCity { get; init; } = string.Empty;

    /// <summary>
    /// Address of the partner's headquarters.
    /// </summary>
    /// <example>123 Main St, Casablanca</example>
    public string HeadquartersAddress { get; init; } = string.Empty;

    /// <summary>
    /// Last name of the contact person.
    /// </summary>
    /// <example>Smith</example>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// First name of the contact person.
    /// </summary>
    /// <example>John</example>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Phone number of the contact person.
    /// </summary>
    /// <example>+212600000000</example>
    public string PhoneNumberContact { get; init; } = string.Empty;

    /// <summary>
    /// Email address of the contact person.
    /// </summary>
    /// <example>john.smith@example.com</example>
    public string MailContact { get; init; } = string.Empty;

    /// <summary>
    /// Function or role of the contact person.
    /// </summary>
    /// <example>Manager</example>
    public string FunctionContact { get; init; } = string.Empty;

    /// <summary>
    /// Type of transfer associated with the partner.
    /// </summary>
    /// <example>BankTransfer</example>
    public string TransferType { get; init; } = string.Empty;

    /// <summary>
    /// Authentication mode used by the partner.
    /// </summary>
    /// <example>Password</example>
    public string AuthenticationMode { get; init; } = string.Empty;

    /// <summary>
    /// Tax identification number of the partner.
    /// </summary>
    /// <example>TIN987654</example>
    public string TaxIdentificationNumber { get; init; } = string.Empty;

    /// <summary>
    /// Tax regime of the partner.
    /// </summary>
    /// <example>Simplified</example>
    public string TaxRegime { get; init; } = string.Empty;

    /// <summary>
    /// Auxiliary account number of the partner.
    /// </summary>
    /// <example>ACC123456</example>
    public string AuxiliaryAccount { get; init; } = string.Empty;

    /// <summary>
    /// ICE (Identifiant Commun de l'Entreprise) of the partner.
    /// </summary>
    /// <example>ICE123456789</example>
    public string ICE { get; init; } = string.Empty;

    /// <summary>
    /// URL or path to the partner's logo.
    /// </summary>
    /// <example>https://example.com/logo.png</example>
    public string Logo { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the partner is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}