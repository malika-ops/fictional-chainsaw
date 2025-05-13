using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Corridors.Queries.GetAllCorridors;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class CorridorRepository : ICorridorRepository
{
    private readonly ApplicationDbContext _context;
    public CorridorRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Corridor?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Corridors
            .Where(corridor => corridor.Id == CorridorId.Of(id))
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<Corridor> AddCorridorAsync(Corridor corridor, CancellationToken cancellationToken)
    {
        await _context.Corridors.AddAsync(corridor);
        await _context.SaveChangesAsync(cancellationToken);
        return corridor;
    }
    public async Task UpdateCorridorAsync(Corridor corridor, CancellationToken cancellationToken)
    {
        _context.Corridors.Update(corridor);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task DeleteCorridorAsync(Corridor corridor, CancellationToken cancellationToken)
    {
        _context.Corridors.Remove(corridor);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<List<City>> GetTaxRulesByCorridorIdAsync(Guid corridorId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Corridor>> GetCorridorsByCriteriaAsync(GetAllCorridorsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Corridors
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllCorridorsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Corridors
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }
    private List<Expression<Func<Corridor, bool>>> BuildFilters(GetAllCorridorsQuery request)
    {
        var filters = new List<Expression<Func<Corridor, bool>>>();

        if (request.SourceCountryId is not null)
            filters.Add(c => c.SourceCountryId == request.SourceCountryId);

        if (request.DestinationCountryId is not null)
            filters.Add(c => c.DestinationCountryId == request.DestinationCountryId);

        if (request.SourceCityId is not null)
            filters.Add(c => c.SourceCityId == request.SourceCityId);

        if (request.DestinationCityId is not null)
            filters.Add(c => c.DestinationCityId == request.DestinationCityId);

        if (request.SourceAgencyId is not null)
            filters.Add(c => c.SourceAgencyId == request.SourceAgencyId);

        if (request.DestinationAgencyId is not null)
            filters.Add(c => c.DestinationAgencyId == request.DestinationAgencyId);

        if (request.IsEnabled is not null)
            filters.Add(c => c.IsEnabled == request.IsEnabled);

        return filters;
    }

}