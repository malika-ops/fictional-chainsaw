using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Corridors.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Corridors.Queries.GetAllCorridors;

public class GetAllCorridorsQueryHandler(ICorridorRepository _corridorRepository)
    : IQueryHandler<GetAllCorridorsQuery, PagedResult<GetAllCorridorsResponse>>
{
    public async Task<PagedResult<GetAllCorridorsResponse>> Handle(GetAllCorridorsQuery request, CancellationToken cancellationToken)
    {
        var corridors = await _corridorRepository.GetPagedByCriteriaAsync(request, request.PageNumber, request.PageSize, cancellationToken);
        var result = new PagedResult<GetAllCorridorsResponse>(
            corridors.Items.Adapt<List<GetAllCorridorsResponse>>(),
            corridors.TotalCount, request.PageNumber, request.PageSize);
        return result;
    }
}
