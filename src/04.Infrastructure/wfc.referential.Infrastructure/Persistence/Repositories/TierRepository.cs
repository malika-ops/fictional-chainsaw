using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TierAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class TierRepository : BaseRepository<Tier, TierId>, ITierRepository
{
    public TierRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
  
}