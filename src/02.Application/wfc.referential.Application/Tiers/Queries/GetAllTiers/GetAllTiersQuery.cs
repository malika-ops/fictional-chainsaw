using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Tiers.Dtos;

namespace wfc.referential.Application.Tiers.Queries.GetAllTiers;

public record GetAllTiersQuery : IQuery<PagedResult<TierResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; } = true;
}