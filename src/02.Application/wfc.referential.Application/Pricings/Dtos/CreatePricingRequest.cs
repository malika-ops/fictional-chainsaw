using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Pricings.Dtos;

public record CreatePricingRequest
{
    /// <summary>
    /// Unique alphanumeric code identifying the pricing line.
    /// </summary>
    /// <example>PRC001</example>
    [Required]
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Distribution channel.
    /// </summary>
    /// <example>Online</example>
    [Required]
    public string Channel { get; init; } = string.Empty;

    /// <summary>
    /// Minimum transaction amount allowed.
    /// </summary>
    /// <example>50.00</example>
    [Required]
    public decimal MinimumAmount { get; init; }

    /// <summary>
    /// Maximum transaction amount allowed.
    /// </summary>
    /// <example>1000.00</example>
    [Required]
    public decimal MaximumAmount { get; init; }

    /// <summary>
    /// Optional fixed fee (e.g., 5 MAD).
    /// </summary>
    /// <example>5.00</example>
    public decimal? FixedAmount { get; init; }

    /// <summary>
    /// Optional rate fee (percentage expressed as 0-1, e.g., 0.015 = 1.5%).
    /// </summary>
    /// <example>0.015</example>
    public decimal? Rate { get; init; }

    /// <summary>
    /// Corridor identifier.
    /// </summary>
    /// <example>c2f1e6d4-7a5b-4c2f-8d3e-1a9b2c3d4e5f</example>
    [Required]
    public Guid CorridorId { get; init; }

    /// <summary>
    /// Service identifier (GUID).
    /// </summary>
    /// <example>c2f1e6d4-7a5b-4c2f-8d3e-1a9b2c3d4e5f</example>
    [Required]
    public Guid ServiceId { get; init; }

    /// <summary>
    /// Optional affiliate identifier (GUID).
    /// </summary>
    /// <example>e5f4d3c2-b1a0-4e9d-8c7f-6b5a4d3c2b1a</example>
    public Guid? AffiliateId { get; init; }
}
