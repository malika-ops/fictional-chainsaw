using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CountryIdentityDocAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class CountryIdentityDocRepository : BaseRepository<CountryIdentityDoc, CountryIdentityDocId>, ICountryIdentityDocRepository
{
    public CountryIdentityDocRepository(ApplicationDbContext context) : base(context)
    {
    }
}