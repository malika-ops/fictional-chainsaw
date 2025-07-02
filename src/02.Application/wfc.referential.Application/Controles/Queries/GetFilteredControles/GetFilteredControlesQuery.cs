using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Controles.Dtos;

namespace wfc.referential.Application.Controles.Queries.GetFilteredControles;

public record GetFilteredControlesQuery : IQuery<PagedResult<GetControleResponse>>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; }
}