using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class BankRepository : BaseRepository<Bank, BankId>, IBankRepository
{
    public BankRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}