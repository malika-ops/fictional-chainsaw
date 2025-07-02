using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;

namespace wfc.referential.Application.Cities.Queries.GetCityById;

public class GetCityByIdQueryHandler : IQueryHandler<GetCityByIdQuery, GetCitiyResponse>
{
    private readonly ICityRepository _cityRepository;

    public GetCityByIdQueryHandler(ICityRepository cityRepository)
    {
        _cityRepository = cityRepository;
    }

    public async Task<GetCitiyResponse> Handle(GetCityByIdQuery query, CancellationToken ct)
    {
        var id = CityId.Of(query.CityId);
        var entity = await _cityRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"City with id '{query.CityId}' not found.");

        return entity.Adapt<GetCitiyResponse>();
    }
} 