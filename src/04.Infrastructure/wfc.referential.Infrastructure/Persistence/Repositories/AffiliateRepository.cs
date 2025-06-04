using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class AffiliateRepository : BaseRepository<Affiliate, AffiliateId>, IAffiliateRepository
{
    public AffiliateRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}