using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryServiceAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ICountryServiceRepository : IRepositoryBase<CountryService, CountryServiceId>
{
    Task<bool> ExistsByCountryAndServiceAsync(CountryId countryId, ServiceId serviceId, CancellationToken cancellationToken = default);
}