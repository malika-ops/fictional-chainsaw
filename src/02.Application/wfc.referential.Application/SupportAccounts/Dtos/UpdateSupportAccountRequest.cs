namespace wfc.referential.Application.SupportAccounts.Dtos;

public record UpdateSupportAccountRequest
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
    /// Whether the account is enabled or not.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; } = true;
}