using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.MonetaryZones.Queries.GetAllMonetaryZones;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class MonetaryZoneRepository : IMonetaryZoneRepository
{
    private readonly ApplicationDbContext _context;

    public MonetaryZoneRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MonetaryZone>> GetAllMonetaryZonesAsync(CancellationToken cancellationToken)
    {
        return await _context.MonetaryZones.ToListAsync();
    }

    public IQueryable<MonetaryZone> GetAllMonetaryZonesQueryable(CancellationToken cancellationToken)
    {
        return _context.MonetaryZones
            .Include(mz => mz.Countries)
                .ThenInclude(c => c.Currency)
            .Include(mz => mz.Countries)
                 .ThenInclude(a => a.Regions)
                        .ThenInclude(r => r.Cities)
            .AsNoTracking();
    }

    public async Task<MonetaryZone?> GetByIdAsync(MonetaryZoneId id, CancellationToken cancellationToken)
    {
        return await _context.MonetaryZones
            .Where(mz => mz.Id == id)
            .Include(mz => mz.Countries)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MonetaryZone?> GetByCodeAsync(string monetaryZoneCode, CancellationToken cancellationToken)
    {
        return await _context.MonetaryZones
            .Where(mz => mz.Code.ToLower() == monetaryZoneCode.ToLower())
            .FirstOrDefaultAsync(cancellationToken);
    }


    public async Task<MonetaryZone> AddMonetaryZoneAsync(MonetaryZone monetaryZone, CancellationToken cancellationToken)
    {
        await _context.MonetaryZones.AddAsync(monetaryZone);
        await _context.SaveChangesAsync(cancellationToken);

        return monetaryZone;
    }

    public async Task UpdateMonetaryZoneAsync(MonetaryZone monetaryZone, CancellationToken cancellationToken)
    {
        _context.MonetaryZones.Update(monetaryZone);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteMonetaryZoneAsync(MonetaryZone monetaryZone, CancellationToken cancellationToken)
    {
        _context.MonetaryZones.Remove(monetaryZone);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<MonetaryZone>> GetFilteredMonetaryZonesAsync(GetAllMonetaryZonesQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.MonetaryZones
            .Include(mz => mz.Countries)
                .ThenInclude(c => c.Currency)
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllMonetaryZonesQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.MonetaryZones
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    private List<Expression<Func<MonetaryZone, bool>>> BuildFilters(GetAllMonetaryZonesQuery request)
    {
        var filters = new List<Expression<Func<MonetaryZone, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            filters.Add(mz => mz.Code.ToUpper().Equals(request.Code.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            filters.Add(mz => mz.Name.ToUpper().Equals(request.Name.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            filters.Add(mz => mz.Description.ToUpper().Equals(request.Description.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.IsEnabled.ToString()))
        {
            filters.Add(reg => reg.IsEnabled == request.IsEnabled);
        }

        return filters;
    }

}
