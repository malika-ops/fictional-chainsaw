using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using BuildingBlocks.Infrastructure.CachingManagement;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.ParamTypes.Dtos;

namespace wfc.referential.Application.ParamTypes.Queries.GetFiltredParamTypes;

public class GetFiltredParamTypesQueryHandler : IQueryHandler<GetFiltredParamTypesQuery, PagedResult<GetFiltredParamTypesResponse>>
{
    private readonly IParamTypeRepository _paramTypeRepository;
    private readonly CacheService _cacheService;

    public GetFiltredParamTypesQueryHandler(IParamTypeRepository paramTypeRepository, CacheService cacheService)
    {
        _paramTypeRepository = paramTypeRepository;
        _cacheService = cacheService;
    }

    public async Task<PagedResult<GetFiltredParamTypesResponse>> Handle(GetFiltredParamTypesQuery request, CancellationToken cancellationToken)
    {
        var paramtypes = await _paramTypeRepository
            .GetPagedByCriteriaAsync(request, request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<GetFiltredParamTypesResponse>(
            paramtypes.Items.Adapt<List<GetFiltredParamTypesResponse>>(),
            paramtypes.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}