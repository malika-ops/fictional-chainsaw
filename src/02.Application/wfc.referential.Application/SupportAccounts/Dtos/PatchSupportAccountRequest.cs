namespace wfc.referential.Application.SupportAccounts.Dtos;

public record PatchSupportAccountRequest
{
    /// <summary>
    /// If provided, updates the code. If omitted, code remains unchanged.
    /// </summary>
    /// <example>SUPAC002</example>
    public string? Code { get; init; }

    /// <summary>
    /// If provided, updates the description. If omitted, description remains unchanged.
    /// </summary>
    /// <example>Support Secondaire</example>
    public string? Description { get; init; }

    /// <summary>
    /// If provided, updates the threshold. If omitted, threshold remains unchanged.
    /// </summary>
    /// <example>15000.00</example>
    public decimal? Threshold { get; init; }

    /// <summary>
    /// If provided, updates the limit. If omitted, limit remains unchanged.
    /// </summary>
    /// <example>25000.00</example>
    public decimal? Limit { get; init; }

    /// <summary>
    /// If provided, updates the account balance. If omitted, account balance remains unchanged.
    /// </summary>
    /// <example>30000.00</example>
    public decimal? AccountBalance { get; init; }

    /// <summary>
    /// If provided, updates the accounting number. If omitted, accounting number remains unchanged.
    /// </summary>
    /// <example>ACC654321</example>
    public string? AccountingNumber { get; init; }

    /// <summary>
    /// If provided, updates the enabled status. If omitted, enabled status remains unchanged.
    /// </summary>
    /// <example>false</example>
    public bool? IsEnabled { get; init; }
}