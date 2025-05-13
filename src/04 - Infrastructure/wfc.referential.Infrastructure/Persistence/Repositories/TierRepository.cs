using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Tiers.Queries.GetAllTiers;
using wfc.referential.Domain.TierAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class TierRepository : ITierRepository
{
    private readonly ApplicationDbContext _ctx;
    public TierRepository(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<Tier?> GetByNameAsync(string name, CancellationToken ct)
        => await _ctx.Tiers
                     .AsNoTracking()
                     .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower(), ct);

    public async Task<Tier> AddAsync(Tier entity, CancellationToken ct)
    {
        await _ctx.Tiers.AddAsync(entity, ct);
        await _ctx.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Tier?> GetByIdAsync(TierId id, CancellationToken ct)
    => await _ctx.Tiers.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task UpdateAsync(Tier entity, CancellationToken ct)
    {
        _ctx.Tiers.Update(entity);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task<List<Tier>> GetFilteredTiersAsync(
        GetAllTiersQuery q,
        CancellationToken ct)
    {
        var filters = BuildFilters(q);

        var query = _ctx.Tiers
                            .AsNoTracking()
                            .ApplyFilters(filters);

        return await query
             .Skip((q.PageNumber - 1) * q.PageSize)
             .Take(q.PageSize)
             .ToListAsync(ct);
    }

    public async Task<int> GetCountTotalAsync(
        GetAllTiersQuery q,
        CancellationToken ct)
    {
        var filters = BuildFilters(q);

        return await _ctx.Tiers
                             .AsNoTracking()
                             .ApplyFilters(filters)
                             .CountAsync(ct);
    }

    private static List<Expression<Func<Tier, bool>>> BuildFilters(GetAllTiersQuery q)
    {
        var f = new List<Expression<Func<Tier, bool>>>();

        if (!string.IsNullOrWhiteSpace(q.Name))
            f.Add(t => t.Name.ToUpper().Equals(q.Name.ToUpper()));

        if (!string.IsNullOrWhiteSpace(q.Description))
            f.Add(t => t.Description.ToUpper().Equals(q.Description));

        if (q.IsEnabled.HasValue)
            f.Add(t => t.IsEnabled == q.IsEnabled.Value);

        return f;
    }
}