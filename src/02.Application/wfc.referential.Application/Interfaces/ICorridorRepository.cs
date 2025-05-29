using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.CorridorAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ICorridorRepository : IRepositoryBase<Corridor, CorridorId>
{
}
