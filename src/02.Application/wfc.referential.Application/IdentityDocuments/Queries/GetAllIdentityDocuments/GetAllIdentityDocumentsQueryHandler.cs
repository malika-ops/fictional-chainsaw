using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.IdentityDocuments.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.IdentityDocuments.Queries.GetAllIdentityDocuments;

public class GetAllIdentityDocumentsQueryHandler(IIdentityDocumentRepository identitydocumentRepository, ICacheService cacheService)
    : IQueryHandler<GetAllIdentityDocumentsQuery, PagedResult<GetAllIdentityDocumentsResponse>>
{
    public async Task<PagedResult<GetAllIdentityDocumentsResponse>> Handle(GetAllIdentityDocumentsQuery request, CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<PagedResult<GetAllIdentityDocumentsResponse>>(request.CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var entities = await identitydocumentRepository.GetByCriteriaAsync(request, cancellationToken);
        int totalCount = await identitydocumentRepository.GetCountAsync(request, cancellationToken);

        var mapped = entities.Adapt<List<GetAllIdentityDocumentsResponse>> ();
        var result = new PagedResult<GetAllIdentityDocumentsResponse>(mapped, totalCount, request.PageNumber, request.PageSize);

        await cacheService.SetAsync(request.CacheKey, result, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return result;
    }
}