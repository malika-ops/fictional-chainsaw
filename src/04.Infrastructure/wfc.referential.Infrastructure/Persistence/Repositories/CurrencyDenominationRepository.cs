using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyDenominationAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class CurrencyDenominationRepository : BaseRepository<CurrencyDenomination, CurrencyDenominationId>, ICurrencyDenominationRepository
{
    public CurrencyDenominationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}