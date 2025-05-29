using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.RegionManagement.Dtos;

namespace wfc.referential.Application.RegionManagement.Queries.GetAllRegions;

public class GetAllRegionsQueryHandler(IRegionRepository regionRepository)
    : IQueryHandler<GetAllRegionsQuery, PagedResult<GetAllRegionsResponse>>
{

    public async Task<PagedResult<GetAllRegionsResponse>> Handle(GetAllRegionsQuery request, CancellationToken cancellationToken)
    {
        var regions = await regionRepository
        .GetPagedByCriteriaAsync(request,request.PageNumber, request.PageSize, cancellationToken);

        var result = new PagedResult<GetAllRegionsResponse>(
            regions.Items.Adapt<List<GetAllRegionsResponse>>(),
            regions.TotalCount, request.PageNumber, request.PageSize);

        return result;
    }
}
