using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Contracts.Dtos;

namespace wfc.referential.Application.Contracts.Queries.GetFiltredContracts;

public record GetFiltredContractsQuery : IQuery<PagedResult<GetContractsResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Code { get; init; }
    public Guid? PartnerId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool? IsEnabled { get; init; } = true;
}