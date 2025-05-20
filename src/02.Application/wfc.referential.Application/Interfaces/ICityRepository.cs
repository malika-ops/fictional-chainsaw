using wfc.referential.Application.Cities.Queries.GetAllCities;
using wfc.referential.Application.RegionManagement.Queries.GetAllRegions;
using wfc.referential.Domain.CityAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ICityRepository
{
    Task<List<City>> GetAllCitiesAsync(CancellationToken cancellationToken);
    Task<City?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<City?> GetByCodeAsync(string cityCode, CancellationToken cancellationToken);

    Task<City> AddCityAsync(City city, CancellationToken cancellationToken);
    Task UpdateCityAsync(City city, CancellationToken cancellationToken);
    Task DeleteCityAsync(City city, CancellationToken cancellationToken);

    Task<List<City>> GetCitiesByCriteriaAsync(GetAllCitiesQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllCitiesQuery request, CancellationToken cancellationToken);
    Task<bool> HasAgencyAsync(CityId cityId, CancellationToken cancellationToken);
    Task<bool> HasSectorAsync(CityId cityId, CancellationToken cancellationToken);
    Task<bool> HasCorridorAsync(CityId cityId, CancellationToken cancellationToken);
}
