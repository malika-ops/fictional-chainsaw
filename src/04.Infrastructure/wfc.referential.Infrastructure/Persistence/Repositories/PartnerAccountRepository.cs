
using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class PartnerAccountRepository : BaseRepository<PartnerAccount, PartnerAccountId>, IPartnerAccountRepository
{
    public PartnerAccountRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}