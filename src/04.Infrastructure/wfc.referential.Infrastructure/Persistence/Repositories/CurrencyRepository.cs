using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class CurrencyRepository : BaseRepository<Currency, CurrencyId>, ICurrencyRepository
{
    public CurrencyRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}