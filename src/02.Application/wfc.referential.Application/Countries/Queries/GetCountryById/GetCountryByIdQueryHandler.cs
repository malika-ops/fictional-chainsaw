using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Countries.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Countries.Queries.GetCountryById;

public class GetCountryByIdQueryHandler : IQueryHandler<GetCountryByIdQuery, GetCountriesResponce>
{
    private readonly ICountryRepository _countryRepository;

    public GetCountryByIdQueryHandler(ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;
    }

    public async Task<GetCountriesResponce> Handle(GetCountryByIdQuery query, CancellationToken ct)
    {
        var id = CountryId.Of(query.CountryId);
        var entity = await _countryRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Country with id '{query.CountryId}' not found.");

        return entity.Adapt<GetCountriesResponce>();
    }
} 