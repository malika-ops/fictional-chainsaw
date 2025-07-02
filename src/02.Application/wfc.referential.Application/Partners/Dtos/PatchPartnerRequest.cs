namespace wfc.referential.Application.Partners.Dtos;

public record PatchPartnerRequest
{
    /// <summary>
    /// If provided, updates the code. If omitted, code remains unchanged.
    /// </summary>
    /// <example>PTN002</example>
    public string? Code { get; init; }

    /// <summary>
    /// If provided, updates the name. If omitted, name remains unchanged.
    /// </summary>
    /// <example>Service Premium</example>
    public string? Name { get; init; }

    /// <summary>
    /// If provided, updates the person type. If omitted, person type remains unchanged.
    /// </summary>
    /// <example>Legal Person</example>
    public string? PersonType { get; init; }

    /// <summary>
    /// If provided, updates the professional tax number. If omitted, professional tax number remains unchanged.
    /// </summary>
    /// <example>PTX654321</example>
    public string? ProfessionalTaxNumber { get; init; }

    /// <summary>
    /// If provided, updates the withholding tax rate. If omitted, withholding tax rate remains unchanged.
    /// </summary>
    /// <example>12.5</example>
    public string? WithholdingTaxRate { get; init; }

    /// <summary>
    /// If provided, updates the headquarters city. If omitted, headquarters city remains unchanged.
    /// </summary>
    /// <example>Rabat</example>
    public string? HeadquartersCity { get; init; }

    /// <summary>
    /// If provided, updates the headquarters address. If omitted, headquarters address remains unchanged.
    /// </summary>
    /// <example>456 New Avenue</example>
    public string? HeadquartersAddress { get; init; }

    /// <summary>
    /// If provided, updates the last name. If omitted, last name remains unchanged.
    /// </summary>
    /// <example>Smith</example>
    public string? LastName { get; init; }

    /// <summary>
    /// If provided, updates the first name. If omitted, first name remains unchanged.
    /// </summary>
    /// <example>Jane</example>
    public string? FirstName { get; init; }

    /// <summary>
    /// If provided, updates the phone number contact. If omitted, phone number contact remains unchanged.
    /// </summary>
    /// <example>+212687654321</example>
    public string? PhoneNumberContact { get; init; }

    /// <summary>
    /// If provided, updates the mail contact. If omitted, mail contact remains unchanged.
    /// </summary>
    /// <example>newcontact@partner.com</example>
    public string? MailContact { get; init; }

    /// <summary>
    /// If provided, updates the function contact. If omitted, function contact remains unchanged.
    /// </summary>
    /// <example>Director</example>
    public string? FunctionContact { get; init; }

    /// <summary>
    /// If provided, updates the transfer type. If omitted, transfer type remains unchanged.
    /// </summary>
    /// <example>Wire Transfer</example>
    public string? TransferType { get; init; }

    /// <summary>
    /// If provided, updates the authentication mode. If omitted, authentication mode remains unchanged.
    /// </summary>
    /// <example>Email</example>
    public string? AuthenticationMode { get; init; }

    /// <summary>
    /// If provided, updates the identification number. If omitted, identification number remains unchanged.
    /// </summary>
    /// <example>TAX654321</example>
    public string? TaxIdentificationNumber { get; init; }

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
    /// If provided, updates the logo. If omitted, logo remains unchanged.
    /// </summary>
    /// <example>/logos/partner002.png</example>
    public string? Logo { get; init; }

    /// <summary>
    /// If provided, updates the enabled status. If omitted, enabled status remains unchanged.
    /// </summary>
    /// <example>false</example>
    public bool? IsEnabled { get; init; }

    /// <summary>
    /// If provided, updates the Network Mode ID. If omitted, Network Mode remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? NetworkModeId { get; init; }

    /// <summary>
    /// If provided, updates the Payment Mode ID. If omitted, Payment Mode remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa7</example>
    public Guid? PaymentModeId { get; init; }

    /// <summary>
    /// If provided, updates the Partner Type ID. If omitted, Partner Type remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa8</example>
    public Guid? PartnerTypeId { get; init; }

    /// <summary>
    /// If provided, updates the Support Account Type ID. If omitted, Support Account Type remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa9</example>
    public Guid? SupportAccountTypeId { get; init; }

    /// <summary>
    /// If provided, updates the Commission Account ID. If omitted, Commission Account remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afaa</example>
    public Guid? CommissionAccountId { get; init; }

    /// <summary>
    /// If provided, updates the Activity Account ID. If omitted, Activity Account remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afab</example>
    public Guid? ActivityAccountId { get; init; }

    /// <summary>
    /// If provided, updates the Support Account ID. If omitted, Support Account remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afac</example>
    public Guid? SupportAccountId { get; init; }

    /// <summary>
    /// If provided, updates the Parent Partner ID. If omitted, Parent Partner remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afad</example>
    public Guid? IdParent { get; init; }
}