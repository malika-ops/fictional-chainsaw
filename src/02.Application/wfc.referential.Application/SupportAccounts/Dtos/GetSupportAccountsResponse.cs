namespace wfc.referential.Application.SupportAccounts.Dtos;

public record GetSupportAccountsResponse
{
    /// <summary>
    /// Unique identifier of the support account.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid SupportAccountId { get; init; }

    /// <summary>
    /// Unique code of the support account.
    /// </summary>
    /// <example>SUPPORT001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Description of the support account.
    /// </summary>
    /// <example>Main support account for operations</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Threshold value for the support account.
    /// </summary>
    /// <example>1000.00</example>
    public decimal Threshold { get; init; }

    /// <summary>
    /// Limit value for the support account.
    /// </summary>
    /// <example>5000.00</example>
    public decimal Limit { get; init; }

    /// <summary>
    /// Current balance of the support account.
    /// </summary>
    /// <example>2500.50</example>
    public decimal AccountBalance { get; init; }

    /// <summary>
    /// Accounting number associated with the support account.
    /// </summary>
    /// <example>401200</example>
    public string AccountingNumber { get; init; } = string.Empty;

    /// <summary>
    /// Unique identifier of the partner, if applicable.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid? PartnerId { get; init; }

    /// <summary>
    /// Unique identifier of the support account type, if applicable.
    /// </summary>
    /// <example>0a1b2c3d-4e5f-6789-abcd-ef0123456789</example>
    public Guid? SupportAccountTypeId { get; init; }

    /// <summary>
    /// Indicates whether the support account is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}