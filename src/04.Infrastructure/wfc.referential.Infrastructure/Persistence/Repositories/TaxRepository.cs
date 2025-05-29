using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class TaxRepository : BaseRepository<Tax, TaxId> , ITaxRepository
{

    public TaxRepository(ApplicationDbContext context) : base(context)
    {
    }
}
