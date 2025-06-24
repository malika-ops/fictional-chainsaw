using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Pricings.Dtos;

public record PatchPricingRequest
{
    /// <summary>Pricing ID (route).</summary>
    [Required] 
    public Guid PricingId { get; init; }

    /// <summary>Optional new code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional channel.</summary>
    public string? Channel { get; init; }

    /// <summary>Optional minimum amount.</summary>
    public decimal? MinimumAmount { get; init; }

    /// <summary>Optional maximum amount.</summary>
    public decimal? MaximumAmount { get; init; }

    /// <summary>Optional fixed fee.</summary>
    public decimal? FixedAmount { get; init; }

    /// <summary>Optional rate fee.</summary>
    public decimal? Rate { get; init; }

    /// <summary>Optional corridor ID.</summary>
    public Guid? CorridorId { get; init; }

    /// <summary>Optional service ID.</summary>
    public Guid? ServiceId { get; init; }

    /// <summary>Optional affiliate ID.</summary>
    public Guid? AffiliateId { get; init; }

    /// <summary>Optional enabled flag.</summary>
    public bool? IsEnabled { get; init; }
}
