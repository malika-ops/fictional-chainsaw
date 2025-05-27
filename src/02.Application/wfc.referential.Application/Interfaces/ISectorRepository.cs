using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ISectorRepository : IRepositoryBase<Sector, SectorId>
{
}