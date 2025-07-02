using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.TypeDefinitions.Dtos;

namespace wfc.referential.Application.TypeDefinitions.Queries.GetFiltredTypeDefinitions;

public class GetFiltredTypeDefinitionsQueryHandler : IQueryHandler<GetFiltredTypeDefinitionsQuery, PagedResult<GetTypeDefinitionsResponse>>
{
    private readonly ITypeDefinitionRepository _repo;
    private readonly ICacheService _cacheService;

    public GetFiltredTypeDefinitionsQueryHandler(ITypeDefinitionRepository repo, ICacheService cacheService)
    {
        _repo = repo;
        _cacheService = cacheService;
    }

    public async Task<PagedResult<GetTypeDefinitionsResponse>> Handle(GetFiltredTypeDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _repo.GetPagedByCriteriaAsync(
            request,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return new PagedResult<GetTypeDefinitionsResponse>(
            paged.Items.Adapt<List<GetTypeDefinitionsResponse>>(),
            paged.TotalCount,
            paged.PageNumber,
            paged.PageSize);
    }
}