using wfc.referential.Application.RegionManagement.Queries.GetAllRegions;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IRegionRepository
{
    Task<List<Region>> GetAllRegionsAsync(CancellationToken cancellationToken);
    Task<Region?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Region?> GetByCodeAsync(string regionCode, CancellationToken cancellationToken);

    Task<List<City>> GetCitiesByRegionIdAsync(Guid regionId, CancellationToken cancellationToken);
    Task<Region> AddRegionAsync(Region Region, CancellationToken cancellationToken);
    Task UpdateRegionAsync(Region Region, CancellationToken cancellationToken);
    Task DeleteRegionAsync(Region Region, CancellationToken cancellationToken);
    Task<List<Region>> GetRegionsByCriteriaAsync(GetAllRegionsQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllRegionsQuery request, CancellationToken cancellationToken);

}
