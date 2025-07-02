namespace wfc.referential.Application.Banks.Dtos;

public record PatchBankRequest
{
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