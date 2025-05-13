using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.PartnerAccounts.Dtos;

public record CreatePartnerAccountRequest
{
    /// <summary>
    /// A unique account number for the Partner Account.
    /// </summary>
    /// <example>000123456789</example>
    public string AccountNumber { get; init; } = string.Empty;

    /// <summary>
    /// The RIB (Relevé d'Identité Bancaire) for the account.
    /// </summary>
    /// <example>12345678901234567890123</example>
    public string RIB { get; init; } = string.Empty;

    /// <summary>
    /// The branch or location of the bank where the account is held.
    /// </summary>
    /// <example>Casablanca Centre</example>
    public string Domiciliation { get; init; } = string.Empty;

    /// <summary>
    /// The full business name associated with the account.
    /// </summary>
    /// <example>Wafa Cash Services</example>
    public string BusinessName { get; init; } = string.Empty;

    /// <summary>
    /// A shortened version of the business name.
    /// </summary>
    /// <example>WCS</example>
    public string ShortName { get; init; } = string.Empty;

    /// <summary>
    /// The current balance of the account.
    /// </summary>
    /// <example>50000.00</example>
    public decimal AccountBalance { get; init; }

    /// <summary>
    /// The ID of the Bank this account belongs to.
    /// </summary>
    /// <example>7b583b69-6e16-5b2c-9c8f-69627ee725d4</example>
    public Guid BankId { get; init; }

    /// <summary>
    /// The type of account (Activité or Commission).
    /// </summary>
    /// <example>Activité</example>
    public string AccountType { get; init; } = string.Empty;
}