using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.IdentityDocuments.Dtos;

namespace wfc.referential.Application.IdentityDocuments.Queries.GetAllIdentityDocuments;

public record GetAllIdentityDocumentsQuery : IQuery<PagedResult<GetIdentityDocumentsResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; } = true;
}