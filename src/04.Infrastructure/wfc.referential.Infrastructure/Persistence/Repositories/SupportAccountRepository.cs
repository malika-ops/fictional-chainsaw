using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class SupportAccountRepository : BaseRepository<SupportAccount, SupportAccountId>, ISupportAccountRepository
{
    public SupportAccountRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}