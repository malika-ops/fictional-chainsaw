using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Cities.Commands.DeleteCity;

public record DeleteCityCommand : ICommand<Result<bool>>
{
    public Guid CityId { get; init; }
}