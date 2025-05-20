using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Cities.Queries.GetAllCities;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class CityRepository : ICityRepository
{
    private readonly ApplicationDbContext _context;

    public CityRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<City>> GetAllCitiesAsync(CancellationToken cancellationToken)
    {
        return await _context.Cities.ToListAsync(cancellationToken);
    }
    public async Task<City?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Cities
            .Where(city => city.Id == CityId.Of(id))
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<City?> GetByCodeAsync(string cityCode, CancellationToken cancellationToken)
    {
        return await _context.Cities
            .Where(city => city.Code == cityCode)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<City> AddCityAsync(City city, CancellationToken cancellationToken)
    {
        await _context.Cities.AddAsync(city);
        await _context.SaveChangesAsync(cancellationToken);

        return city;
    }

    public async Task UpdateCityAsync(City city, CancellationToken cancellationToken)
    {
        _context.Cities.Update(city);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCityAsync(City city, CancellationToken cancellationToken)
    {

        _context.Cities.Remove(city);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<City>> GetCitiesByCriteriaAsync(GetAllCitiesQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Cities
            .Include(region => region.Sectors)
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllCitiesQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Cities
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    private List<Expression<Func<City, bool>>> BuildFilters(GetAllCitiesQuery request)
    {
        var filters = new List<Expression<Func<City, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            filters.Add(reg => reg.Code.ToUpper()!.Equals(request.Code.ToUpper()));
        }
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            filters.Add(reg => reg.Name.ToUpper()!.Equals(request.Name.ToUpper()));
        }
        if (!string.IsNullOrWhiteSpace(request.Abbreviation))
        {
            filters.Add(reg => reg.Abbreviation!.ToUpper()!.Equals(request.Abbreviation.ToUpper()));
        }
        if (!string.IsNullOrWhiteSpace(request.TimeZone))
        {
            filters.Add(reg => reg.TimeZone.ToUpper()!.Equals(request.TimeZone.ToUpper()));
        }
        if (request.RegionId is not null)
        {
            filters.Add(reg => reg.RegionId.Equals(request.RegionId));
        }
        if (request.IsEnabled.HasValue)
        {
            filters.Add(reg => reg.IsEnabled.Equals(request.IsEnabled));
        }

        return filters;
    }

    public async Task<bool> HasAgencyAsync(CityId cityId, CancellationToken cancellationToken)
    {
        return await _context.Agencies
            .AsNoTracking()
            .AnyAsync(agency => agency.CityId == cityId, cancellationToken);
    }

    public async Task<bool> HasSectorAsync(CityId cityId, CancellationToken cancellationToken)
    {
        return await _context.Sectors
            .AsNoTracking()
            .AnyAsync(sector => sector.CityId == cityId, cancellationToken);
    }

    public async Task<bool> HasCorridorAsync(CityId cityId, CancellationToken cancellationToken)
    {
        return await _context.Corridors
            .AsNoTracking()
            .AnyAsync(corridor => corridor.SourceCityId == cityId || corridor.DestinationCityId == cityId, cancellationToken);
    }
}
