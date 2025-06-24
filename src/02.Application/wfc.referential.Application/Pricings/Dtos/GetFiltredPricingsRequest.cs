namespace wfc.referential.Application.Pricings.Dtos;

public record GetFiltredPricingsRequest : FilterRequest
{
    /// <summary>Filter by pricing Code.</summary>
    public string? Code { get; init; }

    /// <summary>Filter by Channel.</summary>
    public string? Channel { get; init; }

    /// <summary>Filter by minimum amount.</summary>
    public decimal? MinimumAmount { get; init; }

    /// <summary>Filter by maximum amount.</summary>
    public decimal? MaximumAmount { get; init; }

    /// <summary>Filter by fixed amount.</summary>
    public decimal? FixedAmount { get; init; }

    /// <summary>Filter by rate.</summary>
    public decimal? Rate { get; init; }

    /// <summary>Filter by CorridorId.</summary>
    public Guid? CorridorId { get; init; }

    /// <summary>Filter by ServiceId.</summary>
    public Guid? ServiceId { get; init; }

    /// <summary>Filter by AffiliateId.</summary>
    public Guid? AffiliateId { get; init; }
}
