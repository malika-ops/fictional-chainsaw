namespace wfc.referential.Application.Contracts.Dtos;

public record GetContractsResponse
{
    /// <summary>
    /// Unique identifier of the contract.
    /// </summary>
    /// <example>c1a2b3d4-e5f6-7890-abcd-1234567890ef</example>
    public Guid ContractId { get; init; }

    /// <summary>
    /// Unique code of the contract.
    /// </summary>
    /// <example>CONTRACT001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Unique identifier of the partner associated with the contract.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid PartnerId { get; init; }

    /// <summary>
    /// Start date of the contract.
    /// </summary>
    /// <example>2024-01-01T00:00:00Z</example>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// End date of the contract.
    /// </summary>
    /// <example>2025-01-01T00:00:00Z</example>
    public DateTime EndDate { get; init; }

    /// <summary>
    /// Indicates whether the contract is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}
