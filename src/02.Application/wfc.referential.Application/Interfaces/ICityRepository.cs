using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.CityAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ICityRepository : IRepositoryBase<City, CityId>
{
}
