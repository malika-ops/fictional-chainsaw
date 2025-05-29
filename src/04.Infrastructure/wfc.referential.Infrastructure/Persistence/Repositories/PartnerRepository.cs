using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class PartnerRepository : BaseRepository<Partner, PartnerId>, IPartnerRepository
{
    public PartnerRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}