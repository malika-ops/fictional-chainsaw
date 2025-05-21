using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.IdentityDocuments.Dtos;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.IdentityDocuments.Queries.GetAllIdentityDocuments;

public record GetAllIdentityDocumentsQuery(int PageNumber, int PageSize, string? Code, string? Name, bool? IsEnabled) 
    : IQuery<PagedResult<GetAllIdentityDocumentsResponse>>, ICacheableQuery
{
    public string CacheKey => $"{nameof(IdentityDocument)}_page{PageNumber}_size{PageSize}_code{Code}_name{Name}_status{IsEnabled}";
    public int CacheExpiration => 5;
}