using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.MonetaryZones.Dtos;

namespace wfc.referential.Application.MonetaryZones.Queries.GetFiltredMonetaryZones;

public record GetFiltredMonetaryZonesQuery : IQuery<PagedResult<MonetaryZoneResponse>>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; }

}
