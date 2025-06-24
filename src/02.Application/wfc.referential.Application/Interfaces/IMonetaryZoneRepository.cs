using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IMonetaryZoneRepository : IRepositoryBase<MonetaryZone, MonetaryZoneId>
{

}
