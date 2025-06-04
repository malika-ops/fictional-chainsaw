using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PricingAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class PricingRepository : BaseRepository<Pricing, PricingId>, IPricingRepository
{
    public PricingRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
