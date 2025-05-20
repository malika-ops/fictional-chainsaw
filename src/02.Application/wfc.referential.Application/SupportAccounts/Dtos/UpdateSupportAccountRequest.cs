namespace wfc.referential.Application.SupportAccounts.Dtos;

public record UpdateSupportAccountRequest
{
    /// <summary>
    /// The ID of the Support Account to update.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f7</example>
    public Guid SupportAccountId { get; init; }

    /// <summary>
    /// A unique code identifier for the Support Account.
    /// </summary>
    /// <example>SUPAC001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// The name of the Support Account.
    /// </summary>
    /// <example>Support Principal</example>
    public string Name { get; init; } = string.Empty;

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
    /// The ID of the Partner this account belongs to.
    /// </summary>
    /// <example>8c583b69-6e16-5b2c-9c8f-69627ee725d5</example>
    public Guid PartnerId { get; init; }

    /// <summary>
    /// The type of Support Account (Commun or Individuel).
    /// </summary>
    /// <example>Commun</example>
    public string SupportAccountType { get; init; } = string.Empty;

    /// <summary>
    /// Whether the account is enabled or not.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; } = true;
}