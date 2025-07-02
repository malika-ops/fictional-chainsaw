using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.CountryServices.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CountryServiceAggregate;

namespace wfc.referential.Application.CountryServices.Queries.GetCountryServiceById;

public class GetCountryServiceByIdQueryHandler : IQueryHandler<GetCountryServiceByIdQuery, GetCountryServicesResponse>
{
    private readonly ICountryServiceRepository _countryServiceRepository;

    public GetCountryServiceByIdQueryHandler(ICountryServiceRepository countryServiceRepository)
    {
        _countryServiceRepository = countryServiceRepository;
    }

    public async Task<GetCountryServicesResponse> Handle(GetCountryServiceByIdQuery query, CancellationToken ct)
    {
        var id = CountryServiceId.Of(query.CountryServiceId);
        var entity = await _countryServiceRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"CountryService with id '{query.CountryServiceId}' not found.");

        return entity.Adapt<GetCountryServicesResponse>();
    }
} 