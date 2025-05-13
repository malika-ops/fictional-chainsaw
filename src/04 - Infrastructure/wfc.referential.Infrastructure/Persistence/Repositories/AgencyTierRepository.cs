using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using wfc.referential.Application.AgencyTiers.Queries.GetAllAgencyTiers;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.TierAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class AgencyTierRepository : IAgencyTierRepository
{
    private readonly ApplicationDbContext _context;

    public AgencyTierRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(Guid agencyId, Guid tierId, string code, CancellationToken ct) =>
    await _context.AgencyTiers
        .AnyAsync(at =>
                  at.AgencyId == new AgencyId(agencyId) &&
                  at.TierId == new TierId(tierId) &&
                  at.Code.ToLower() == code.ToLower(), ct);

    public async Task<AgencyTier> AddAsync(AgencyTier entity, CancellationToken ct)
    {
        await _context.AgencyTiers.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(AgencyTier at, CancellationToken ct)
    {
        _context.AgencyTiers.Update(at);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<AgencyTier?> GetByIdAsync(AgencyTierId id, CancellationToken ct)
    {
        return await _context.AgencyTiers.AsNoTracking()
                             .FirstOrDefaultAsync(a => a.Id == id, ct);
    }


    public async Task<AgencyTier?> GetByCodeAsync(string code, CancellationToken ct)
    {
        return await _context.AgencyTiers.AsNoTracking()
                      .FirstOrDefaultAsync(a => a.Code.ToLower() == code.ToLower(), ct);
    }


    public async Task<List<AgencyTier>> GetFilteredAgencyTiersAsync(
    GetAllAgencyTiersQuery q, CancellationToken ct)
    {
        var filters = BuildFilters(q);

        var query = _context.AgencyTiers.AsNoTracking().ApplyFilters(filters);

        return await query
            .Skip((q.PageNumber - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetCountTotalAsync(GetAllAgencyTiersQuery q, CancellationToken ct)
    {
        var filters = BuildFilters(q);
        return await _context.AgencyTiers.AsNoTracking().ApplyFilters(filters).CountAsync(ct);
    }

    private static List<Expression<Func<AgencyTier, bool>>> BuildFilters(GetAllAgencyTiersQuery q)
    {
        var f = new List<Expression<Func<AgencyTier, bool>>>();

        if (q.AgencyId.HasValue) f.Add(x => x.AgencyId == AgencyId.Of(q.AgencyId.Value));
        if (q.TierId.HasValue) f.Add(x => x.TierId == TierId.Of(q.TierId.Value));
        if (!string.IsNullOrWhiteSpace(q.Code)) f.Add(x => x.Code.ToUpper().Equals(q.Code.ToUpper()));
        if (q.IsEnabled.HasValue) f.Add(x => x.IsEnabled == q.IsEnabled.Value);

        return f;
    }
}
