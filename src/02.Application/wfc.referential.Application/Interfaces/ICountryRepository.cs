using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Interfaces
{
    public interface ICountryRepository : IRepositoryBase<Country, CountryId>
    {
       
    }
}
