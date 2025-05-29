using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class CityRepository : BaseRepository<City, CityId>, ICityRepository
{

    public CityRepository(ApplicationDbContext context) : base(context)
    {
    }
    //public async Task<City?> GetCityByIdAsync(CityId id, CancellationToken cancellationToken)
    //{
    //    return await GetByIdAsync(id, cancellationToken);
    //}
    //public async Task<City?> GetByCodeAsync(string cityCode, CancellationToken cancellationToken)
    //{
    //    return await GetOneByConditionAsync(city => city.Code == cityCode, cancellationToken);
    //}

    //public async Task<City> AddCityAsync(City city, CancellationToken cancellationToken)
    //{
    //    return await AddAsync(city);
    //}

    //public void UpdateCity(City city)
    //{
    //    Update(city);
    //}

    //public void DeleteCity(City city)
    //{
    //    Delete(city);
    //}

    //public async Task<PagedResult<City>> GetCitiesByCriteriaAsync(GetAllCitiesQuery request, CancellationToken cancellationToken)
    //{
    //    return await GetPagedByCriteriaAsync(request, request.PageNumber, request.PageSize, cancellationToken);
    //}

    //public async Task<bool> HasAgencyAsync(CityId cityId, CancellationToken cancellationToken)
    //{
    //    return await _context.Agencies
    //        .AsNoTracking()
    //        .AnyAsync(agency => agency.CityId == cityId, cancellationToken);
    //}

    //public async Task<bool> HasSectorAsync(CityId cityId, CancellationToken cancellationToken)
    //{
    //    return await _context.Sectors
    //        .AsNoTracking()
    //        .AnyAsync(sector => sector.CityId == cityId, cancellationToken);
    //}

    //public async Task<bool> HasCorridorAsync(CityId cityId, CancellationToken cancellationToken)
    //{
    //    return await _context.Corridors
    //        .AsNoTracking()
    //        .AnyAsync(corridor => corridor.SourceCityId == cityId || corridor.DestinationCityId == cityId, cancellationToken);
    //}
}
