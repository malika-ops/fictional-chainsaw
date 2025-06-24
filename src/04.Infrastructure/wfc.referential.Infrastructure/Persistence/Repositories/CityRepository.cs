using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class CityRepository : BaseRepository<City, CityId>, ICityRepository
{

    public CityRepository(ApplicationDbContext context) : base(context)
    {
    }
}
