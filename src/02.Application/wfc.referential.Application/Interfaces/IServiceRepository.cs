using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IServiceRepository : IRepositoryBase<Service, ServiceId>
{
}
