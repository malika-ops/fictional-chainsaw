using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class MonetaryZoneRepository : BaseRepository<MonetaryZone, MonetaryZoneId>, IMonetaryZoneRepository
{
    public MonetaryZoneRepository(ApplicationDbContext context) : base(context)
    {
    }
}
