using wfc.referential.Application.MonetaryZones.Queries.GetAllMonetaryZones;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IMonetaryZoneRepository
{
    Task<List<MonetaryZone>> GetAllMonetaryZonesAsync(CancellationToken cancellationToken);
    IQueryable<MonetaryZone> GetAllMonetaryZonesQueryable(CancellationToken cancellationToken);
    Task<MonetaryZone?> GetByIdAsync(MonetaryZoneId id, CancellationToken cancellationToken);
    Task<MonetaryZone?> GetByCodeAsync(string monetaryZoneCode, CancellationToken cancellationToken);
    Task<MonetaryZone> AddMonetaryZoneAsync(MonetaryZone monetaryZone, CancellationToken cancellationToken);
    Task UpdateMonetaryZoneAsync(MonetaryZone monetaryZone, CancellationToken cancellationToken);
    Task DeleteMonetaryZoneAsync(MonetaryZone monetaryZone, CancellationToken cancellationToken);
    Task<List<MonetaryZone>> GetFilteredMonetaryZonesAsync(GetAllMonetaryZonesQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllMonetaryZonesQuery request, CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

}
