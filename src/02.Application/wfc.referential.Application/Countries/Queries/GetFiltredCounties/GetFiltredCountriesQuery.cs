using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Countries.Dtos;

namespace wfc.referential.Application.Countries.Queries.GetFiltredCounties;

public record GetFiltredCountriesQuery : IQuery<PagedResult<GetCountriesResponce>>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public string? Name { get; init; }
    public string? Code { get; init; }
    public string? ISO2 { get; init; }
    public string? ISO3 { get; init; }
    public bool? IsEnabled { get; init; }
    public Guid? MonetaryZoneId { get; init; }

}
