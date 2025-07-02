namespace wfc.referential.Application.PartnerAccounts.Dtos;

public record PartnerAccountResponse
{
    /// <summary>
    /// Unique identifier of the partner account.
    /// </summary>
    /// <example>c1a2b3d4-e5f6-7890-abcd-1234567890ef</example>
    public Guid PartnerAccountId { get; init; }

    /// <summary>
    /// Account number of the partner account.
    /// </summary>
    /// <example>1234567890</example>
    public string AccountNumber { get; init; } = string.Empty;

    /// <summary>
    /// RIB (Bank Account Identifier) of the partner account.
    /// </summary>
    /// <example>RIB1234567890</example>
    public string RIB { get; init; } = string.Empty;

    /// <summary>
    /// Domiciliation of the partner account.
    /// </summary>
    /// <example>Main Branch</example>
    public string? Domiciliation { get; init; }

    /// <summary>
    /// Business name associated with the partner account.
    /// </summary>
    /// <example>Acme Corp</example>
    public string? BusinessName { get; init; }

    /// <summary>
    /// Short name for the partner account.
    /// </summary>
    /// <example>ACME</example>
    public string? ShortName { get; init; }

    /// <summary>
    /// Current balance of the partner account.
    /// </summary>
    /// <example>10000.50</example>
    public decimal AccountBalance { get; init; }

    /// <summary>
    /// Unique identifier of the bank.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid BankId { get; init; }

    /// <summary>
    /// Name of the bank.
    /// </summary>
    /// <example>National Bank</example>
    public string BankName { get; init; } = string.Empty;

    /// <summary>
    /// Code of the bank.
    /// </summary>
    /// <example>BANK001</example>
    public string BankCode { get; init; } = string.Empty;

    /// <summary>
    /// Unique identifier of the account type.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid AccountTypeId { get; init; }

    /// <summary>
    /// Name of the account type.
    /// </summary>
    /// <example>Current Account</example>
    public string AccountTypeName { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the partner account is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}