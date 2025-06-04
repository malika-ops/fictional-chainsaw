using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Services.Queries.GetAllServices;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IServiceRepository : IRepositoryBase<Service, ServiceId>
{
    Task<List<Service>> GetAllServicesAsync(CancellationToken cancellationToken);
    Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Service?> GetByCodeAsync(string ServiceCode, CancellationToken cancellationToken);
    Task<Service> AddServiceAsync(Service Service, CancellationToken cancellationToken);
    Task UpdateServiceAsync(Service Service, CancellationToken cancellationToken);
    Task DeleteServiceAsync(Service Service, CancellationToken cancellationToken);
    Task<List<Service>> GetServicesByCriteriaAsync(GetAllServicesQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllServicesQuery request, CancellationToken cancellationToken);

}
