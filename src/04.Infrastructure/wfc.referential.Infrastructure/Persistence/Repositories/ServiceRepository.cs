using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class ServiceRepository : BaseRepository<Service, ServiceId> , IServiceRepository
{
    public ServiceRepository(ApplicationDbContext context) : base(context)
    {
    }
}