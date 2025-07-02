using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ServiceControleAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class ServiceControleRepository 
    : BaseRepository<ServiceControle, ServiceControleId>, IServiceControleRepository
{
    public ServiceControleRepository(ApplicationDbContext dbContext)
        : base(dbContext) { }
}