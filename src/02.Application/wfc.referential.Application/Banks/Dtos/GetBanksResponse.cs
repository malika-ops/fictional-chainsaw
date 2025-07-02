namespace wfc.referential.Application.Banks.Dtos;

public record GetBanksResponse
{
    /// <summary>
    /// Unique identifier of the bank.
    /// </summary>
    /// <example>c1a2b3d4-e5f6-7890-abcd-1234567890ef</example>
    public Guid BankId { get; init; }

    /// <summary>
    /// Unique code of the bank.
    /// </summary>
    /// <example>BANK001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Name of the bank.
    /// </summary>
    /// <example>National Bank</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Abbreviation of the bank name.
    /// </summary>
    /// <example>NATBANK</example>
    public string Abbreviation { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the bank is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}