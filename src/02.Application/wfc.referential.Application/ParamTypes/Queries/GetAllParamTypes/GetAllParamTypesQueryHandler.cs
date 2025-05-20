using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using BuildingBlocks.Infrastructure.CachingManagement;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.ParamTypes.Dtos;


namespace wfc.referential.Application.ParamTypes.Queries.GetAllParamTypes;

public class GetAllParamTypesQueryHandler(IParamTypeRepository _paramTypeRepository,CacheService _cacheService) : IQueryHandler<GetAllParamTypesQuery, PagedResult<GetAllParamTypesResponse>>
{
    public async Task<PagedResult<GetAllParamTypesResponse>> Handle(GetAllParamTypesQuery request, CancellationToken cancellationToken)
    {

        var paramtypes = await _paramTypeRepository
        .GetFilteredParamTypesAsync(request, cancellationToken);

        int totalCount = await _paramTypeRepository
            .GetCountTotalAsync(request, cancellationToken);

        var paramtypesResponse = paramtypes.Adapt<List<GetAllParamTypesResponse>>();

        var result = new PagedResult<GetAllParamTypesResponse>(paramtypesResponse, totalCount, request.PageNumber, request.PageSize);

        await _cacheService.SetAsync(request.CacheKey,
            result,
            TimeSpan.FromMinutes(request.CacheExpiration),
            cancellationToken);
        
        return result;


    }
}
