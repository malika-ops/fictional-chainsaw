namespace wfc.referential.Application.Pricings.Dtos;

public record PricingResponse
{
    /// <summary>
    /// Unique identifier of the pricing.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid Id { get; init; }

    /// <summary>
    /// Unique code of the pricing.
    /// </summary>
    /// <example>PRC001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Channel associated with the pricing (e.g., Online, Branch).
    /// </summary>
    /// <example>Online</example>
    public string Channel { get; init; } = string.Empty;

    /// <summary>
    /// Minimum transaction amount for which this pricing applies.
    /// </summary>
    /// <example>100.00</example>
    public decimal MinimumAmount { get; init; }

    /// <summary>
    /// Maximum transaction amount for which this pricing applies.
    /// </summary>
    /// <example>10000.00</example>
    public decimal MaximumAmount { get; init; }

    /// <summary>
    /// Fixed fee amount, if applicable.
    /// </summary>
    /// <example>10.00</example>
    public decimal? FixedAmount { get; init; }

    /// <summary>
    /// Percentage rate applied to the transaction, if applicable.
    /// </summary>
    /// <example>0.05</example>
    public decimal? Rate { get; init; }

    /// <summary>
    /// Indicates whether the pricing is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Unique identifier of the corridor associated with the pricing.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid CorridorId { get; init; }

    /// <summary>
    /// Unique identifier of the service associated with the pricing.
    /// </summary>
    /// <example>0a1b2c3d-4e5f-6789-abcd-ef0123456789</example>
    public Guid ServiceId { get; init; }

    /// <summary>
    /// Unique identifier of the affiliate, if applicable.
    /// </summary>
    /// <example>c1a2b3d4-e5f6-7890-abcd-1234567890ef</example>
    public Guid? AffiliateId { get; init; }

    /// <summary>
    /// Date and time when the pricing was created.
    /// </summary>
    /// <example>2024-01-01T12:00:00+00:00</example>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Date and time when the pricing was last modified.
    /// </summary>
    /// <example>2024-06-01T12:00:00+00:00</example>
    public DateTimeOffset LastModified { get; init; }

    /// <summary>
    /// User who created the pricing.
    /// </summary>
    /// <example>admin</example>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// User who last modified the pricing.
    /// </summary>
    /// <example>editor</example>
    public string LastModifiedBy { get; init; } = string.Empty;
}
