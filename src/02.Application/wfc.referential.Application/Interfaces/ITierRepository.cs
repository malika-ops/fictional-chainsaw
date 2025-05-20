using wfc.referential.Application.Tiers.Queries.GetAllTiers;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ITierRepository
{
    Task<Tier?> GetByNameAsync(string name, CancellationToken ct);
    Task<Tier> AddAsync(Tier entity, CancellationToken ct);
    Task<Tier?> GetByIdAsync(TierId id, CancellationToken ct);
    Task UpdateAsync(Tier entity, CancellationToken ct);
    Task<List<Tier>> GetFilteredTiersAsync(GetAllTiersQuery q, CancellationToken ct);
    Task<int> GetCountTotalAsync(GetAllTiersQuery q, CancellationToken ct);
}
