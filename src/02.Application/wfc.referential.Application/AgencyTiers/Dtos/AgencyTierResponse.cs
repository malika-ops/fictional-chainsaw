namespace wfc.referential.Application.AgencyTiers.Dtos;

public record AgencyTierResponse
{
    /// <summary>
    /// Unique identifier for the Agency–Tier association.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid AgencyTierId { get; init; }

    /// <summary>
    /// Unique identifier of the Agency.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid AgencyId { get; init; }

    /// <summary>
    /// Unique identifier of the Tier.
    /// </summary>
    /// <example>0a1b2c3d-4e5f-6789-abcd-ef0123456789</example>
    public Guid TierId { get; init; }

    /// <summary>
    /// Unique code for this Agency–Tier association.
    /// </summary>
    /// <example>AGENCY_TIER_001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the Agency–Tier association is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}