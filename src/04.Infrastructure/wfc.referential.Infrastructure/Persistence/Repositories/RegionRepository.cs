using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class RegionRepository : BaseRepository<Region, RegionId>, IRegionRepository
{

    public RegionRepository(ApplicationDbContext context) : base(context)
    {
    }
}
