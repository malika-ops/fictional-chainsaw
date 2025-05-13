using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using wfc.referential.Application.Countries.Queries.GetAllCounties;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Infrastructure.Data.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly ApplicationDbContext _context;

        public CountryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Country>> GetAllCountriesAsync(CancellationToken cancellationToken)
        {
            return await _context.Countries.ToListAsync(cancellationToken);
        }

        public IQueryable<Country> GetAllCountriesQueryable(CancellationToken cancellationToken)
        {
            return _context.Countries.Include(c => c.Currency).AsNoTracking();
        }

        public async Task<Country?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Countries.Include(c => c.Regions)
                .FirstOrDefaultAsync(x => x.Id == new CountryId(id), cancellationToken);
        }

        public async Task<Country?> GetByCodeAsync(string countryCode, CancellationToken cancellationToken)
        {
            return await _context.Countries
                .Where(mz => mz.Code.ToLower() == countryCode.ToLower())
                .FirstOrDefaultAsync(cancellationToken);
        }


        public async Task<Country> AddAsync(Country country, CancellationToken cancellationToken)
        {
            await _context.Countries.AddAsync(country, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return country;
        }

        public async Task UpdateAsync(Country country, CancellationToken cancellationToken)
        {
            _context.Countries.Update(country);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Country country, CancellationToken cancellationToken)
        {
            _context.Countries.Remove(country);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Country>> GetAllCountriesPaginatedAsyncFiltred(GetAllCountriesQuery request, CancellationToken cancellationToken)
        {
            var filters = BuildFilters(request);

            var query = _context.Countries.AsQueryable()
                .Include(c => c.Currency) 
                .AsNoTracking()
                .ApplyFilters(filters);

            return await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetCountTotalAsync(GetAllCountriesQuery request, CancellationToken cancellationToken)
        {
            var filters = BuildFilters(request);

            var query = _context.Countries
                .AsQueryable()
                .AsNoTracking()
                .ApplyFilters(filters);

            return await query.CountAsync(cancellationToken);
        }

        private List<Expression<Func<Country, bool>>> BuildFilters(GetAllCountriesQuery request)
        {
            var filters = new List<Expression<Func<Country, bool>>>();

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                filters.Add(c => c.Name.ToUpper().Equals(request.Name.ToUpper()));
            }

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                filters.Add(c => c.Code.ToUpper().Equals(request.Code.ToUpper()));
            }

            if (!string.IsNullOrWhiteSpace(request.ISO2))
            {
                filters.Add(c => c.ISO2.ToUpper().Equals(request.ISO2.ToUpper()));
            }

            if (!string.IsNullOrWhiteSpace(request.ISO3))
            {
                filters.Add(c => c.ISO3.ToUpper().Equals(request.ISO3.ToUpper()));
            }

            if (!string.IsNullOrWhiteSpace(request.DialingCode))
            {
                filters.Add(c => c.DialingCode.ToUpper().Equals(request.DialingCode.ToUpper()));
            }

            if (!string.IsNullOrWhiteSpace(request.TimeZone))
            {
                filters.Add(c => c.TimeZone.ToUpper().Equals(request.TimeZone.ToUpper()));
            }


            if (!string.IsNullOrWhiteSpace(request.Abbreviation))
            {
                filters.Add(c => c.Abbreviation.ToUpper().Equals(request.Abbreviation.ToUpper()));
            }

            if (!string.IsNullOrWhiteSpace(request.IsEnabled.ToString()))
            {
                filters.Add(reg => reg.IsEnabled == request.IsEnabled);
            }
            return filters;
        }
    }
}
