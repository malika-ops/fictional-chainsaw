namespace wfc.referential.Application.SupportAccounts.Dtos;

public record PatchSupportAccountRequest
{
    /// <summary>
    /// The ID of the Support Account to patch.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f7</example>
    public Guid SupportAccountId { get; init; }

    /// <summary>
    /// If provided, updates the code. If omitted, code remains unchanged.
    /// </summary>
    /// <example>SUPAC002</example>
    public string? Code { get; init; }

    /// <summary>
    /// If provided, updates the name. If omitted, name remains unchanged.
    /// </summary>
    /// <example>Support Secondaire</example>
    public string? Name { get; init; }

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
    /// If provided, updates the partner. If omitted, partner remains unchanged.
    /// </summary>
    /// <example>9c583b69-6e16-5b2c-9c8f-69627ee725d6</example>
    public Guid? PartnerId { get; init; }

    /// <summary>
    /// If provided, updates the support account type. If omitted, support account type remains unchanged.
    /// </summary>
    /// <example>Individuel</example>
    public string? SupportAccountType { get; init; }

    /// <summary>
    /// If provided, updates the enabled status. If omitted, enabled status remains unchanged.
    /// </summary>
    /// <example>false</example>
    public bool? IsEnabled { get; init; }
}