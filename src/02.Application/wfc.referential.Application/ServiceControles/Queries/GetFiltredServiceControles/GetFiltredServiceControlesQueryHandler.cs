using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.ServiceControles.Dtos;


namespace wfc.referential.Application.ServiceControles.Queries.GetFiltredServiceControles;

public class GetFiltredServiceControlesQueryHandler 
    : IQueryHandler<GetFiltredServiceControlesQuery, PagedResult<ServiceControleResponse>>
{
    private readonly IServiceControleRepository _repo;

    public GetFiltredServiceControlesQueryHandler(IServiceControleRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<ServiceControleResponse>> Handle(
        GetFiltredServiceControlesQuery q,
        CancellationToken ct)
    {
        var page = await _repo.GetPagedByCriteriaAsync(
            q,
            q.PageNumber,
            q.PageSize,
            ct,
            sc => sc.Channel!
        );

        var items = page.Items.Adapt<List<ServiceControleResponse>>();

        return new PagedResult<ServiceControleResponse>(
            items,
            page.TotalCount,
            page.PageNumber,
            page.PageSize);
    }
}