using wfc.referential.Application.Countries.Queries.GetAllCounties;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Interfaces
{
    public interface ICountryRepository
    {
        Task<List<Country>> GetAllCountriesAsync(CancellationToken cancellationToken);
        IQueryable<Country> GetAllCountriesQueryable(CancellationToken cancellationToken);
        Task<Country?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Country?> GetByCodeAsync(string countryCode, CancellationToken cancellationToken);
        Task<Country> AddAsync(Country country, CancellationToken cancellationToken);
        Task UpdateAsync(Country country, CancellationToken cancellationToken);
        Task DeleteAsync(Country country, CancellationToken cancellationToken);
        Task<List<Country>> GetAllCountriesPaginatedAsyncFiltred(GetAllCountriesQuery request, CancellationToken cancellationToken);
        Task<int> GetCountTotalAsync(GetAllCountriesQuery request, CancellationToken cancellationToken);
    }
}
