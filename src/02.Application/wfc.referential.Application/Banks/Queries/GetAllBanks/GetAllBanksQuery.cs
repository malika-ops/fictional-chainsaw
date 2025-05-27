using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Banks.Dtos;

namespace wfc.referential.Application.Banks.Queries.GetAllBanks;

public record GetAllBanksQuery : IQuery<PagedResult<GetBanksResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Abbreviation { get; init; }
    public bool? IsEnabled { get; init; } = true;
}