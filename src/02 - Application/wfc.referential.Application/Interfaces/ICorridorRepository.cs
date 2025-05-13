using wfc.referential.Application.Corridors.Queries.GetAllCorridors;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ICorridorRepository
{
    Task<Corridor?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Corridor> AddCorridorAsync(Corridor corridor, CancellationToken cancellationToken);
    Task<List<City>> GetTaxRulesByCorridorIdAsync(Guid corridorId, CancellationToken cancellationToken);
    Task UpdateCorridorAsync(Corridor corridor, CancellationToken cancellationToken);
    Task DeleteCorridorAsync(Corridor corridor, CancellationToken cancellationToken);
    Task<List<Corridor>> GetCorridorsByCriteriaAsync(GetAllCorridorsQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllCorridorsQuery request, CancellationToken cancellationToken);
}
