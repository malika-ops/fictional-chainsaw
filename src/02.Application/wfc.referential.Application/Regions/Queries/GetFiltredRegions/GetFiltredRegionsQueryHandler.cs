using System.Linq.Expressions;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.RegionManagement.Dtos;

namespace wfc.referential.Application.RegionManagement.Queries.GetFiltredRegions;

public class GetFiltredRegionsQueryHandler(IRegionRepository regionRepository)
    : IQueryHandler<GetFiltredRegionsQuery, PagedResult<GetRegionsResponse>>
{

    public async Task<PagedResult<GetRegionsResponse>> Handle(GetFiltredRegionsQuery request, CancellationToken cancellationToken)
    {
        var regions = await regionRepository
        .GetPagedByCriteriaAsync(request,request.PageNumber, request.PageSize, cancellationToken, r => r.Cities);

        var result = new PagedResult<GetRegionsResponse>(
            regions.Items.Adapt<List<GetRegionsResponse>>(),
            regions.TotalCount, request.PageNumber, request.PageSize);

        return result;
    }
}
