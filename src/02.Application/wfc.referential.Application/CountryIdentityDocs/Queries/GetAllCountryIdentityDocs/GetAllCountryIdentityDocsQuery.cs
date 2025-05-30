using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.CountryIdentityDocs.Dtos;

namespace wfc.referential.Application.CountryIdentityDocs.Queries.GetAllCountryIdentityDocs;

public record GetAllCountryIdentityDocsQuery : IQuery<PagedResult<GetCountryIdentityDocsResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? CountryId { get; init; }
    public Guid? IdentityDocumentId { get; init; }
    public bool? IsEnabled { get; init; } = true;
}