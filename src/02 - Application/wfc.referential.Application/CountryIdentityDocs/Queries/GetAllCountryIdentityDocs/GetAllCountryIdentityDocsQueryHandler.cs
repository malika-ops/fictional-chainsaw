using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.CountryIdentityDocs.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.CountryIdentityDocs.Queries.GetAllCountryIdentityDocs;

public class GetAllCountryIdentityDocsQueryHandler : IQueryHandler<GetAllCountryIdentityDocsQuery, PagedResult<GetCountryIdentityDocResponse>>
{
    private readonly ICountryIdentityDocRepository _countryIdentityDocRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IIdentityDocumentRepository _identityDocumentRepository;

    public GetAllCountryIdentityDocsQueryHandler(
        ICountryIdentityDocRepository countryIdentityDocRepository,
        ICountryRepository countryRepository,
        IIdentityDocumentRepository identityDocumentRepository)
    {
        _countryIdentityDocRepository = countryIdentityDocRepository;
        _countryRepository = countryRepository;
        _identityDocumentRepository = identityDocumentRepository;
    }

    public async Task<PagedResult<GetCountryIdentityDocResponse>> Handle(GetAllCountryIdentityDocsQuery request, CancellationToken cancellationToken)
    {
        var countryIdentityDocs = await _countryIdentityDocRepository
            .GetFilteredAsync(request, cancellationToken);

        int totalCount = await _countryIdentityDocRepository
            .GetCountTotalAsync(request, cancellationToken);

        // Extract IDs for related entities
        var countryIds = countryIdentityDocs.Select(x => x.CountryId.Value).Distinct().ToList();
        var documentIds = countryIdentityDocs.Select(x => x.IdentityDocumentId.Value).Distinct().ToList();

        // Fetch all countries and filter in memory
        var allCountries = await _countryRepository.GetAllCountriesAsync(cancellationToken);
        var countries = allCountries
            .Where(c => countryIds.Contains(c.Id.Value))
            .ToDictionary(c => c.Id.Value, c => c);

        // Fetch all documents
        var allDocuments = await _identityDocumentRepository.GetAllAsync(cancellationToken);
        var documents = allDocuments
            .Where(d => documentIds.Contains(d.Id.Value))
            .ToDictionary(d => d.Id.Value, d => d);

        // Construct responses
        var responses = countryIdentityDocs.Select(doc => new GetCountryIdentityDocResponse(
            doc.Id.Value,
            doc.CountryId.Value,
            doc.IdentityDocumentId.Value,
            doc.IsEnabled,
            countries.GetValueOrDefault(doc.CountryId.Value)?.Name ?? "Unknown Country",
            countries.GetValueOrDefault(doc.CountryId.Value)?.Code ?? "Unknown Code",
            documents.GetValueOrDefault(doc.IdentityDocumentId.Value)?.Name ?? "Unknown Document",
            documents.GetValueOrDefault(doc.IdentityDocumentId.Value)?.Code ?? "Unknown Code"
        )).ToList();

        return new PagedResult<GetCountryIdentityDocResponse>(
            responses,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}