using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class CountryRepository : BaseRepository<Country, CountryId>, ICountryRepository
{

    public CountryRepository(ApplicationDbContext context) : base(context) { }
}