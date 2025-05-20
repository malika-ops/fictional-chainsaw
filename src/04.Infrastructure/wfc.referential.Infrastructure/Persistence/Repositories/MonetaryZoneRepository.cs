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
         => await _context.MonetaryZones.ToListAsync(cancellationToken);

    public IQueryable<MonetaryZone> GetAllMonetaryZonesQueryable(CancellationToken cancellationToken)
        => _context.MonetaryZones
            .Include(mz => mz.Countries)
                .ThenInclude(c => c.Currency)
            .Include(mz => mz.Countries)
                 .ThenInclude(a => a.Regions)
                        .ThenInclude(r => r.Cities)
            .AsNoTracking();

    public async Task<MonetaryZone?> GetByIdAsync(MonetaryZoneId id, CancellationToken cancellationToken)
        => await _context.MonetaryZones
            .Include(mz => mz.Countries)
            .FirstOrDefaultAsync(mz => mz.Id == id, cancellationToken);

    public async Task<MonetaryZone?> GetByCodeAsync(string monetaryZoneCode, CancellationToken cancellationToken)
       => await _context.MonetaryZones
           .FirstOrDefaultAsync(mz => mz.Code.ToLower() == monetaryZoneCode.ToLower(), cancellationToken);

    public async Task<MonetaryZone> AddMonetaryZoneAsync(MonetaryZone monetaryZone, CancellationToken cancellationToken)
    {
        await _context.MonetaryZones.AddAsync(monetaryZone, cancellationToken);
        return monetaryZone;
    }

    public Task UpdateMonetaryZoneAsync(MonetaryZone monetaryZone, CancellationToken cancellationToken)
    {
        _context.MonetaryZones.Update(monetaryZone);
        return Task.CompletedTask;
    }

    public Task DeleteMonetaryZoneAsync(MonetaryZone monetaryZone, CancellationToken cancellationToken)
    {
        _context.MonetaryZones.Remove(monetaryZone);
        return Task.CompletedTask;
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

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
          => _context.SaveChangesAsync(cancellationToken);

    // --- Private Helpers ---
    private static List<Expression<Func<MonetaryZone, bool>>> BuildFilters(GetAllMonetaryZonesQuery request)
    {
        var filters = new List<Expression<Func<MonetaryZone, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.Code))
            filters.Add(mz => mz.Code.ToUpper() == request.Code.ToUpper());

        if (!string.IsNullOrWhiteSpace(request.Name))
            filters.Add(mz => mz.Name.ToUpper() == request.Name.ToUpper());

        if (!string.IsNullOrWhiteSpace(request.Description))
            filters.Add(mz => mz.Description.ToUpper() == request.Description.ToUpper());

        if (request.IsEnabled != null)
            filters.Add(mz => mz.IsEnabled == request.IsEnabled);

        return filters;
    }

}
