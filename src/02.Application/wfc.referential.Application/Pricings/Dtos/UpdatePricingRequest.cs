using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Pricings.Dtos;

public record UpdatePricingRequest
{
    /// <summary>Unique alphanumeric code identifying the pricing line.</summary>
    [Required]
    public string Code { get; init; } = string.Empty;

    /// <summary>Distribution channel (e.g., Branch, Online).</summary>
    [Required]
    public string Channel { get; init; } = string.Empty;

    /// <summary>Minimum transaction amount allowed.</summary>
    [Required] 
    public decimal MinimumAmount { get; init; }

    /// <summary>Maximum transaction amount allowed.</summary>
    [Required] 
    public decimal MaximumAmount { get; init; }

    /// <summary>Optional fixed fee (e.g., 5 MAD).</summary>
    public decimal? FixedAmount { get; init; }

    /// <summary>Optional rate fee (percentage expressed as 0-1, e.g., 0.015 = 1.5 %).</summary>
    public decimal? Rate { get; init; }

    /// <summary>Corridor identifier (GUID).</summary>
    [Required] 
    public Guid CorridorId { get; init; }

    /// <summary>Service identifier.</summary>
    [Required] 
    public Guid ServiceId { get; init; }

    /// <summary>Optional affiliate identifier.</summary>
    public Guid? AffiliateId { get; init; }

    /// <summary>Whether the pricing row is enabled (default = true).</summary>
    public bool IsEnabled { get; init; } = true;
}
