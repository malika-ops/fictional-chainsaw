using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.CountryServiceAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ICountryServiceRepository : IRepositoryBase<CountryService, CountryServiceId>
{
}