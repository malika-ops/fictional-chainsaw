using BuildingBlocks.Core.Abstraction.Repositories;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryServiceAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class CountryServiceRepository : BaseRepository<CountryService, CountryServiceId>, ICountryServiceRepository
{
    private readonly ApplicationDbContext _context;

    public CountryServiceRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByCountryAndServiceAsync(CountryId countryId, ServiceId serviceId, CancellationToken cancellationToken = default)
    {
        return await _context.CountryServices
            .AnyAsync(x => x.CountryId == countryId && x.ServiceId == serviceId, cancellationToken);
    }
}