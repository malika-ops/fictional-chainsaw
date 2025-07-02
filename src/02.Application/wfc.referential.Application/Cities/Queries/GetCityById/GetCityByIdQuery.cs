using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Cities.Dtos;

namespace wfc.referential.Application.Cities.Queries.GetCityById;

public record GetCityByIdQuery : IQuery<GetCitiyResponse>
{
    public Guid CityId { get; init; }
} 