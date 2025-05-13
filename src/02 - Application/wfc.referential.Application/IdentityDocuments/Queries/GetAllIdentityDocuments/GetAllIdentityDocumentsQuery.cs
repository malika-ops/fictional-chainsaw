using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.IdentityDocuments.Dtos;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.IdentityDocuments.Queries.GetAllIdentityDocuments;

public record GetAllIdentityDocumentsQuery : IQuery<PagedResult<GetAllIdentityDocumentsResponse>>, ICacheableQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; }

    public string CacheKey => $"{nameof(IdentityDocument)}_page{PageNumber}_size{PageSize}_code{Code}_name{Name}_status{IsEnabled}";
    public int CacheExpiration => 5;
}