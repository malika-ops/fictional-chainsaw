using wfc.referential.Application.Sectors.Queries.GetAllSectors;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ISectorRepository
{
    Task<List<Sector>> GetAllSectorsAsync(CancellationToken cancellationToken);
    IQueryable<Sector> GetAllSectorsQueryable(CancellationToken cancellationToken);
    Task<Sector?> GetByIdAsync(SectorId id, CancellationToken cancellationToken);
    Task<Sector?> GetByCodeAsync(string sectorCode, CancellationToken cancellationToken);
    Task<Sector> AddSectorAsync(Sector sector, CancellationToken cancellationToken);
    Task UpdateSectorAsync(Sector sector, CancellationToken cancellationToken);
    Task DeleteSectorAsync(Sector sector, CancellationToken cancellationToken);
    Task<List<Sector>> GetFilteredSectorsAsync(GetAllSectorsQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllSectorsQuery request, CancellationToken cancellationToken);
    Task<bool> HasLinkedAgenciesAsync(SectorId sectorId, CancellationToken cancellationToken);
}