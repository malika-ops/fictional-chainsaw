using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Sectors.Queries.GetAllSectors;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class SectorRepository : ISectorRepository
{
    private readonly ApplicationDbContext _context;

    public SectorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Sector>> GetAllSectorsAsync(CancellationToken cancellationToken)
    {
        return await _context.Sectors.ToListAsync(cancellationToken);
    }

    public IQueryable<Sector> GetAllSectorsQueryable(CancellationToken cancellationToken)
    {
        return _context.Sectors
            .Include(s => s.City)
            .AsNoTracking();
    }

    public async Task<Sector?> GetByIdAsync(SectorId id, CancellationToken cancellationToken)
    {
        return await _context.Sectors
            .Where(s => s.Id == id)
            .Include(s => s.City)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Sector?> GetByCodeAsync(string sectorCode, CancellationToken cancellationToken)
    {
        return await _context.Sectors
            .Where(s => s.Code.ToLower() == sectorCode.ToLower())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Sector> AddSectorAsync(Sector sector, CancellationToken cancellationToken)
    {
        await _context.Sectors.AddAsync(sector, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return sector;
    }

    public async Task UpdateSectorAsync(Sector sector, CancellationToken cancellationToken)
    {
        _context.Sectors.Update(sector);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteSectorAsync(Sector sector, CancellationToken cancellationToken)
    {
        _context.Sectors.Remove(sector);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Sector>> GetFilteredSectorsAsync(GetAllSectorsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Sectors
            .Include(s => s.City)
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllSectorsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Sectors
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> HasLinkedAgenciesAsync(SectorId sectorId, CancellationToken cancellationToken)
    {
        return await _context.Agencies
            .AnyAsync(a => a.SectorId == sectorId, cancellationToken);
    }

    private List<Expression<Func<Sector, bool>>> BuildFilters(GetAllSectorsQuery request)
    {
        var filters = new List<Expression<Func<Sector, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            filters.Add(s => s.Code.ToUpper().Equals(request.Code.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            filters.Add(s => s.Name.ToUpper().Equals(request.Name.ToUpper()));
        }

        if (request.CityId.HasValue && request.CityId != Guid.Empty)
        {
            filters.Add(s => s.City.Id.Value == request.CityId);
        }

        if (request.IsEnabled.HasValue)
        {
            filters.Add(s => s.IsEnabled == request.IsEnabled.Value);
        }

        return filters;
    }
}