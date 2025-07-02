namespace wfc.referential.Application.Tiers.Dtos;

public record TierResponse
{
    /// <summary>
    /// Unique identifier of the tier.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid TierId { get; init; }

    /// <summary>
    /// Name of the tier.
    /// </summary>
    /// <example>Gold</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of the tier.
    /// </summary>
    /// <example>Premium tier with additional benefits</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the tier is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}
