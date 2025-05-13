using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.CityAggregate;

namespace wfc.referential.Application.Cities.Commands.DeleteCity;

public record DeleteCityCommand : ICommand<Result<bool>>, ICacheableQuery
{
    public Guid CityId { get; init; }
    public string CacheKey => $"{nameof(City)}_{CityId}";
    public int CacheExpiration => 5;
}