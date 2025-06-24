using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Corridors.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Corridors.Queries.GetFiltredCorridors;

public class GetFiltredCorridorsQueryHandler(ICorridorRepository _corridorRepository)
    : IQueryHandler<GetFiltredCorridorsQuery, PagedResult<GetCorridorResponse>>
{
    public async Task<PagedResult<GetCorridorResponse>> Handle(GetFiltredCorridorsQuery request, CancellationToken cancellationToken)
    {
        var corridors = await _corridorRepository.GetPagedByCriteriaAsync(request, request.PageNumber, request.PageSize, cancellationToken);
        var result = new PagedResult<GetCorridorResponse>(
            corridors.Items.Adapt<List<GetCorridorResponse>>(),
            corridors.TotalCount, request.PageNumber, request.PageSize);
        return result;
    }
}
