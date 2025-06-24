using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CountryServiceAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class CountryServiceRepository : BaseRepository<CountryService, CountryServiceId>, ICountryServiceRepository
{
    public CountryServiceRepository(ApplicationDbContext context) : base(context)
    {
    }
}