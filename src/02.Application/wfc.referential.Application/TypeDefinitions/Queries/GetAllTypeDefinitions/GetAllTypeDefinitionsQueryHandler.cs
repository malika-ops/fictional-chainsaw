using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.TypeDefinitions.Dtos;


namespace wfc.referential.Application.TypeDefinitions.Queries.GetAllTypeDefinitions;

public class GetAllTypeDefinitionsQueryHandler(ITypeDefinitionRepository _typeDefinitionRepository, ICacheService _cacheService) : IQueryHandler<GetAllTypeDefinitionsQuery, PagedResult<GetAllTypeDefinitionsResponse>>
{
    public async Task<PagedResult<GetAllTypeDefinitionsResponse>> Handle(GetAllTypeDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var typedefinitions = await _typeDefinitionRepository
                .GetFilteredTypeDefinitionsAsync(request, cancellationToken);

        int totalCount = await _typeDefinitionRepository
            .GetCountTotalAsync(request, cancellationToken);

        var typedefinitionsResponse = typedefinitions.Adapt<List<GetAllTypeDefinitionsResponse>>();

        var result = new PagedResult<GetAllTypeDefinitionsResponse>(typedefinitionsResponse, totalCount, request.PageNumber, request.PageSize);
        
        await _cacheService.SetAsync(request.CacheKey,
            result,
            TimeSpan.FromMinutes(request.CacheExpiration),
            cancellationToken);

        return result;
    }
}
