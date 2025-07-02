using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.CountryIdentityDocs.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CountryIdentityDocAggregate;

namespace wfc.referential.Application.CountryIdentityDocs.Queries.GetCountryIdentityDocById;

public class GetCountryIdentityDocByIdQueryHandler : IQueryHandler<GetCountryIdentityDocByIdQuery, GetCountryIdentityDocsResponse>
{
    private readonly ICountryIdentityDocRepository _countryIdentityDocRepository;

    public GetCountryIdentityDocByIdQueryHandler(ICountryIdentityDocRepository countryIdentityDocRepository)
    {
        _countryIdentityDocRepository = countryIdentityDocRepository;
    }

    public async Task<GetCountryIdentityDocsResponse> Handle(GetCountryIdentityDocByIdQuery query, CancellationToken ct)
    {
        var id = CountryIdentityDocId.Of(query.CountryIdentityDocId);
        var entity = await _countryIdentityDocRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"CountryIdentityDoc with id '{query.CountryIdentityDocId}' not found.");

        return entity.Adapt<GetCountryIdentityDocsResponse>();
    }
} 