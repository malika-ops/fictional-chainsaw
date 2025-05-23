using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class AgencyRepository : BaseRepository<Agency, AgencyId>, IAgencyRepository
{
    
    public AgencyRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}