using wfc.referential.Application.AgencyTiers.Queries.GetAllAgencyTiers;
using wfc.referential.Domain.AgencyTierAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IAgencyTierRepository
{
    Task<bool> ExistsAsync(Guid agencyId, Guid tierId, string code, CancellationToken ct);
    Task<AgencyTier> AddAsync(AgencyTier entity, CancellationToken ct);
    Task UpdateAsync(AgencyTier agencyTier, CancellationToken ct);
    Task<AgencyTier?> GetByIdAsync(AgencyTierId id, CancellationToken ct);
    Task<AgencyTier?> GetByCodeAsync(string code, CancellationToken ct);
    Task<List<AgencyTier>> GetFilteredAgencyTiersAsync(GetAllAgencyTiersQuery q, CancellationToken ct);
    Task<int> GetCountTotalAsync(GetAllAgencyTiersQuery q, CancellationToken ct);

}
