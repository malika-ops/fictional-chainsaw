namespace wfc.referential.Application.Banks.Dtos;

public record PatchBankRequest
{
    /// <summary>
    /// The ID of the Bank to patch.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f6</example>
    public Guid BankId { get; init; }

    /// <summary>
    /// If provided, updates the code. If omitted, code remains unchanged.
    /// </summary>
    /// <example>AWB-NEW</example>
    public string? Code { get; init; }

    /// <summary>
    /// If provided, updates the name. If omitted, name remains unchanged.
    /// </summary>
    /// <example>Attijariwafa Bank New</example>
    public string? Name { get; init; }

    /// <summary>
    /// If provided, updates the abbreviation. If omitted, abbreviation remains unchanged.
    /// </summary>
    /// <example>AWB-N</example>
    public string? Abbreviation { get; init; }

    /// <summary>
    /// If provided, updates the enabled status. If omitted, status remains unchanged.
    /// </summary>
    /// <example>false</example>
    public bool? IsEnabled { get; init; }
}