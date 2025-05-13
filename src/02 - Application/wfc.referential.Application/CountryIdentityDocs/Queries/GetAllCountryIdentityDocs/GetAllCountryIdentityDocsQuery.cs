using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.CountryIdentityDocs.Dtos;

namespace wfc.referential.Application.CountryIdentityDocs.Queries.GetAllCountryIdentityDocs;

public class GetAllCountryIdentityDocsQuery : IQuery<PagedResult<GetCountryIdentityDocResponse>>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public Guid? CountryId { get; init; }
    public Guid? IdentityDocumentId { get; init; }
    public bool? IsEnabled { get; init; }

    public GetAllCountryIdentityDocsQuery(
        int pageNumber,
        int pageSize,
        Guid? countryId = null,
        Guid? identityDocumentId = null,
        bool? isEnabled = true)
    {
        CountryId = countryId;
        IdentityDocumentId = identityDocumentId;
        IsEnabled = isEnabled;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}