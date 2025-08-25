using System.ComponentModel.DataAnnotations;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.SupportAccounts.Dtos;

public record CreateSupportAccountRequest
{
    /// <summary>
    /// A unique code identifier for the Support Account.
    /// </summary>
    /// <example>SUPAC001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// The description of the Support Account.
    /// </summary>
    /// <example>Support Principal</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The threshold amount for this Support Account.
    /// </summary>
    /// <example>10000.00</example>
    public decimal Threshold { get; init; }

    /// <summary>
    /// The limit amount for this Support Account.
    /// </summary>
    /// <example>20000.00</example>
    public decimal Limit { get; init; }

    /// <summary>
    /// The current balance of the account.
    /// </summary>
    /// <example>25000.00</example>
    public decimal AccountBalance { get; init; }

    /// <summary>
    /// The accounting number associated with this Support Account.
    /// </summary>
    /// <example>ACC123456</example>
    public string AccountingNumber { get; init; } = string.Empty;

    /// <summary>
    /// The Support Account Type associated with this Support Account.
    /// </summary>
    /// <example>Commun</example>
    public SupportAccountTypeEnum SupportAccountType { get; init; }
}