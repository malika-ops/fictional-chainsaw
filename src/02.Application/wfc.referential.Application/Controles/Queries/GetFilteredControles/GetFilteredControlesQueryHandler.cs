using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Controles.Dtos;
using wfc.referential.Application.Controles.Queries.GetFilteredControles;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Controles.Queries.GetAllControles;

public class GetFilteredControlesQueryHandler 
    : IQueryHandler<GetFilteredControlesQuery, PagedResult<GetControleResponse>>
{
    private readonly IControleRepository _controleRepo;

    public GetFilteredControlesQueryHandler(IControleRepository controleRepo)
    {
        _controleRepo = controleRepo;
    }

    public async Task<PagedResult<GetControleResponse>> Handle(
        GetFilteredControlesQuery query, CancellationToken ct)
    {
        var page = await _controleRepo.GetPagedByCriteriaAsync(
            query,
            query.PageNumber,
            query.PageSize,
            ct);

        var items = page.Items.Adapt<List<GetControleResponse>>();
        return new PagedResult<GetControleResponse>(
            items,
            page.TotalCount,
            page.PageNumber,
            page.PageSize);
    }
}
