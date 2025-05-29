using BuildingBlocks.Core.Abstraction.Repositories;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Cities.Queries.GetAllCities;
using wfc.referential.Domain.CityAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ICityRepository : IRepositoryBase<City, CityId>
{
    //Task<City?> GetCityByIdAsync(CityId id, CancellationToken cancellationToken);
    //Task<City?> GetByCodeAsync(string cityCode, CancellationToken cancellationToken);

    //Task<City> AddCityAsync(City city, CancellationToken cancellationToken);
    //void UpdateCity(City city);
    //void DeleteCity(City city);

    //Task<PagedResult<City>> GetCitiesByCriteriaAsync(GetAllCitiesQuery request, CancellationToken cancellationToken);
    //Task<bool> HasAgencyAsync(CityId cityId, CancellationToken cancellationToken);
    //Task<bool> HasSectorAsync(CityId cityId, CancellationToken cancellationToken);
    //Task<bool> HasCorridorAsync(CityId cityId, CancellationToken cancellationToken);
}
