using wfc.referential.Application.Agencies.Queries.GetAllAgencies;
using wfc.referential.Domain.AgencyAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IAgencyRepository
{
    Task<Agency?> GetByCodeAsync(string code, CancellationToken ct);
    Task<Agency> AddAsync(Agency agency, CancellationToken ct);
    Task<List<Agency>> GetAllAgenciesPaginatedAsyncFiltered(GetAllAgenciesQuery query, CancellationToken ct);
    Task<int> GetCountTotalAsync(GetAllAgenciesQuery query, CancellationToken ct);
    Task UpdateAsync(Agency agency, CancellationToken ct);
    Task<Agency?> GetByIdAsync(Guid id, CancellationToken ct);

}
