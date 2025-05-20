using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.PartnerCountries.Queries.GetAllPartnerCountries;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class PartnerCountryRepository : IPartnerCountryRepository
{
    private readonly ApplicationDbContext _context;

    public PartnerCountryRepository(ApplicationDbContext context)
        => _context = context;

    public async Task<PartnerCountry?> GetByPartnerAndCountryAsync(
        Guid partnerId, Guid countryId, CancellationToken ct)
    {
        return await _context.PartnerCountries
            .FirstOrDefaultAsync(pc =>
                   pc.PartnerId == new PartnerId(partnerId) &&
                   pc.CountryId == new CountryId(countryId), ct);
    }

    public async Task<PartnerCountry> AddAsync(PartnerCountry entity,
                                               CancellationToken ct)
    {
        await _context.PartnerCountries.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<PartnerCountry?> GetByIdAsync(Guid id, CancellationToken ct)
    => await _context.PartnerCountries
                     .FirstOrDefaultAsync(pc => pc.Id == new PartnerCountryId(id), ct);

    public async Task UpdateAsync(PartnerCountry entity, CancellationToken ct)
    {
        _context.PartnerCountries.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<PartnerCountry>> GetAllPaginatedAsyncFiltered(
        GetAllPartnerCountriesQuery q, CancellationToken ct)
    {
        var filters = BuildFilters(q);

        var query = _context.PartnerCountries
                        .AsNoTracking()
                        .ApplyFilters(filters);

        return await query
                .Skip((q.PageNumber - 1) * q.PageSize)
                .Take(q.PageSize)
                .ToListAsync(ct);
    }

    public async Task<int> GetTotalCountAsync(GetAllPartnerCountriesQuery q, CancellationToken ct)
    {
        var filters = BuildFilters(q);
        return await _context.PartnerCountries
                         .AsNoTracking()
                         .ApplyFilters(filters)
                         .CountAsync(ct);
    }

    private static List<Expression<Func<PartnerCountry, bool>>> BuildFilters(GetAllPartnerCountriesQuery q)
    {
        var f = new List<Expression<Func<PartnerCountry, bool>>>();

        if (q.PartnerId.HasValue)
            f.Add(pc => pc.PartnerId == new PartnerId(q.PartnerId.Value));

        if (q.CountryId.HasValue)
            f.Add(pc => pc.CountryId == new CountryId(q.CountryId.Value));

        if (q.IsEnabled.HasValue)
            f.Add(pc => pc.IsEnabled == q.IsEnabled);

        return f;
    }
}