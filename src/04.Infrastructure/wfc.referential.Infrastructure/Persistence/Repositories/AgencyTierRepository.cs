using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class AgencyTierRepository : BaseRepository<AgencyTier, AgencyTierId>, IAgencyTierRepository
{

    public AgencyTierRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

}
