using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.RegionManagement.Queries.GetAllRegions;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories
{
    public class RegionRepository : IRegionRepository
    {
        private readonly ApplicationDbContext _context;

        public RegionRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Region>> GetAllRegionsAsync(CancellationToken cancellationToken)
        {
            return await _context.Regions.ToListAsync(cancellationToken);
        }
        public async Task<Region?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Regions
                .Where(region => region.Id == RegionId.Of(id))
                .Include(region => region.Cities)
                .FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<Region?> GetByCodeAsync(string regionCode, CancellationToken cancellationToken)
        {
            return await _context.Regions
                .Where(region => region.Code == regionCode)
                .Include(region => region.Cities)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Region> AddRegionAsync(Region Region, CancellationToken cancellationToken)
        {
            await _context.Regions.AddAsync(Region);
            await _context.SaveChangesAsync(cancellationToken);

            return Region;
        }

        public async Task UpdateRegionAsync(Region Region, CancellationToken cancellationToken)
        {
            _context.Regions.Update(Region);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteRegionAsync(Region Region, CancellationToken cancellationToken)
        {

            _context.Regions.Remove(Region);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task<List<Region>> GetRegionsByCriteriaAsync(GetAllRegionsQuery request, CancellationToken cancellationToken)
        {
            var filters = BuildFilters(request);

            var query = _context.Regions
                .Include(region => region.Cities)
                .AsNoTracking()
                .ApplyFilters(filters);

            return await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetCountTotalAsync(GetAllRegionsQuery request, CancellationToken cancellationToken)
        {
            var filters = BuildFilters(request);

            var query = _context.Regions
                .AsNoTracking()
                .ApplyFilters(filters);

            return await query.CountAsync(cancellationToken);
        }

        private List<Expression<Func<Region, bool>>> BuildFilters(GetAllRegionsQuery request)
        {
            var filters = new List<Expression<Func<Region, bool>>>();

            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                filters.Add(reg => reg.Code.ToUpper()!.Equals(request.Code.ToUpper()));
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                filters.Add(reg => reg.Name.ToUpper()!.Equals(request.Name.ToUpper()));
            }
            if (request.CountryId is not null)
            {
                filters.Add(reg => reg.CountryId.Equals(request.CountryId));
            }
            filters.Add(reg => reg.IsEnabled == request.IsEnabled);

            return filters;
        }
        public async Task<List<City>> GetCitiesByRegionIdAsync(Guid regionId, CancellationToken cancellationToken)
        {
            return await _context.Cities
                .Where(r => r.RegionId == RegionId.Of(regionId))
                .ToListAsync();
        }
    }
}
