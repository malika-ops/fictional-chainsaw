using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Pricings.Dtos;

namespace wfc.referential.Application.Pricings.Queries.GetFiltredPricings;

public record GetFiltredPricingsQuery : IQuery<PagedResult<PricingResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public string? Code { get; init; }
    public string? Channel { get; init; }
    public decimal? MinimumAmount { get; init; }
    public decimal? MaximumAmount { get; init; }
    public decimal? FixedAmount { get; init; }
    public decimal? Rate { get; init; }

    public Guid? CorridorId { get; init; }
    public Guid? ServiceId { get; init; }
    public Guid? AffiliateId { get; init; }
    public bool? IsEnabled { get; init; } = true;
}