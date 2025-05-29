using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IRegionRepository : IRepositoryBase<Region, RegionId>
{
}
