using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class SectorRepository : BaseRepository<Sector, SectorId>, ISectorRepository
{
    public SectorRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}