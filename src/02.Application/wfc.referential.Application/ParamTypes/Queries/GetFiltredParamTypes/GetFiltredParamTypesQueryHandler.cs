using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using BuildingBlocks.Infrastructure.CachingManagement;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.ParamTypes.Dtos;

namespace wfc.referential.Application.ParamTypes.Queries.GetFiltredParamTypes;

public class GetFiltredParamTypesQueryHandler : IQueryHandler<GetFiltredParamTypesQuery, PagedResult<ParamTypesResponse>>
{
    private readonly IParamTypeRepository _paramTypeRepository;
    private readonly CacheService _cacheService;

    public GetFiltredParamTypesQueryHandler(IParamTypeRepository paramTypeRepository, CacheService cacheService)
    {
        _paramTypeRepository = paramTypeRepository;
        _cacheService = cacheService;
    }

    public async Task<PagedResult<ParamTypesResponse>> Handle(GetFiltredParamTypesQuery request, CancellationToken cancellationToken)
    {
        var paramtypes = await _paramTypeRepository
            .GetPagedByCriteriaAsync(request, request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<ParamTypesResponse>(
            paramtypes.Items.Adapt<List<ParamTypesResponse>>(),
            paramtypes.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}