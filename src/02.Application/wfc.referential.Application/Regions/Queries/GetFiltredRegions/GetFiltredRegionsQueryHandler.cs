using System.Linq.Expressions;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.RegionManagement.Dtos;

namespace wfc.referential.Application.RegionManagement.Queries.GetFiltredRegions;

public class GetFiltredRegionsQueryHandler(IRegionRepository regionRepository)
    : IQueryHandler<GetFiltredRegionsQuery, PagedResult<GetFiltredRegionsResponse>>
{

    public async Task<PagedResult<GetFiltredRegionsResponse>> Handle(GetFiltredRegionsQuery request, CancellationToken cancellationToken)
    {
        var regions = await regionRepository
        .GetPagedByCriteriaAsync(request,request.PageNumber, request.PageSize, cancellationToken, r => r.Cities);

        var result = new PagedResult<GetFiltredRegionsResponse>(
            regions.Items.Adapt<List<GetFiltredRegionsResponse>>(),
            regions.TotalCount, request.PageNumber, request.PageSize);

        return result;
    }
}
