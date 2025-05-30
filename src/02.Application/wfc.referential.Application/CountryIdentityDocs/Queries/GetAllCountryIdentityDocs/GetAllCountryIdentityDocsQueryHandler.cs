using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.CountryIdentityDocs.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.CountryIdentityDocs.Queries.GetAllCountryIdentityDocs;

public class GetAllCountryIdentityDocsQueryHandler : IQueryHandler<GetAllCountryIdentityDocsQuery, PagedResult<GetCountryIdentityDocsResponse>>
{
    private readonly ICountryIdentityDocRepository _countryIdentityDocRepository;

    public GetAllCountryIdentityDocsQueryHandler(ICountryIdentityDocRepository countryIdentityDocRepository)
        => _countryIdentityDocRepository = countryIdentityDocRepository;

    public async Task<PagedResult<GetCountryIdentityDocsResponse>> Handle(GetAllCountryIdentityDocsQuery query, CancellationToken ct)
    {
        var countryIdentityDocs = await _countryIdentityDocRepository.GetPagedByCriteriaAsync(
            query,
            query.PageNumber,
            query.PageSize,
            ct);

        return new PagedResult<GetCountryIdentityDocsResponse>(
            countryIdentityDocs.Items.Adapt<List<GetCountryIdentityDocsResponse>>(),
            countryIdentityDocs.TotalCount,
            countryIdentityDocs.PageNumber,
            countryIdentityDocs.PageSize);
    }
}