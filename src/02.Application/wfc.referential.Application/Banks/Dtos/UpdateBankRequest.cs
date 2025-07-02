using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Banks.Dtos;

public record UpdateBankRequest
{
    /// <summary>
    /// A unique code identifier for the Bank.
    /// </summary>
    /// <example>AWB</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// A human-readable name for the Bank.
    /// </summary>
    /// <example>Attijariwafa Bank</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// An abbreviation for the Bank.
    /// </summary>
    /// <example>AWB</example>
    public string Abbreviation { get; init; } = string.Empty;

    /// <summary>
    /// Whether the bank is enabled or disabled
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; } = true;
}